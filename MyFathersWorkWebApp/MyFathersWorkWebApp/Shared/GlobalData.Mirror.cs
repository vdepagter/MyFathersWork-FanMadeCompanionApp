using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MyFathersWorkWebApp;

// Mirror mode: play alongside the official app. Random draws start unanswered (-1); the first draw of an unanswered
// slot aborts the current action, which is then replayed once per possible value in a sandbox to see what each value
// shows. The players pick the variant the official app shows and the action is run again with that value.
public partial class GlobalData
{
    private class MirrorState
    {
        public string                     Json       { get; init; } = string.Empty;
        public Dictionary<string, object> TmpValues  { get; init; } = new();
        public GameplayHub?               Hub        { get; init; }
        public GameplayWindow?            Window     { get; init; }
        public GameplayPopup?             Popup      { get; init; }
        public GameplayInputPopup?        InputPopup { get; init; }
        public int                        UndoCount  { get; init; }
    }

    private const int _MIRROR_CONTEXT_WORDS = 8;
    private const int _MIRROR_CHANGED_WORDS = 40;

    public bool MirrorMode { get; set; }

    [JsonIgnore] public MirrorQuestion? ActiveMirrorQuestion { get; private set; }
    [JsonIgnore] public bool            IsMirrorSimulating   { get; private set; }
    [JsonIgnore] public int             MirrorSimulationValue { get; private set; } // value for unanswered draws while previewing

    // Every entry point into scenario code (links, popups, undo, load, start) goes through here
    public void RunScenarioAction(Action action)
    {
        if (!MirrorMode)
        {
            action();
            return;
        }

        MirrorState state = CaptureMirrorState();

        try
        {
            action();
        }
        catch (MirrorQuestionException question)
        {
            MirrorQuestion? mirrorQuestion = BuildMirrorQuestion(question, action, state);
            RestoreMirrorState(state);

            if (mirrorQuestion == null)
            {
                // The value changes nothing - no need to ask
                TheCostOfDiseaseVars.RandomArray[question.Index] = 0;
                RunScenarioAction(action);
                return;
            }

            ActiveMirrorQuestion = mirrorQuestion;
        }
    }

    public void AnswerMirrorQuestion(int value)
    {
        MirrorQuestion? question = ActiveMirrorQuestion;
        if (question == null) return;

        ActiveMirrorQuestion                             = null;
        TheCostOfDiseaseVars.RandomArray[question.Index] = value;
        RunScenarioAction(question.Retry);
    }

    public void RollMirrorQuestion()
    {
        if (ActiveMirrorQuestion == null) return;
        AnswerMirrorQuestion(RandomNumberGenerator.GetInt32(ActiveMirrorQuestion.Count));
    }

    private MirrorQuestion? BuildMirrorQuestion(MirrorQuestionException question, Action action, MirrorState state)
    {
        if (question.NumericMin != null && question.Count > MirrorQuestion.MaxOptions)
        {
            // Too many values to list (e.g. 1 - 999): show where the number appears and let the players type it
            (string text0, _) = PreviewMirror(action, state, question.Index, 0);
            (string text1, _) = PreviewMirror(action, state, question.Index, 1);
            (string before, _, string after) = DiffMirrorTexts([text0, text1]);

            return new MirrorQuestion
            {
                Index      = question.Index,
                Count      = question.Count,
                Retry      = action,
                Options    = [],
                Before     = before,
                After      = after,
                NumericMin = question.NumericMin
            };
        }

        List<(int value, string text)> outcomes = new();
        HashSet<string>                effects  = new();

        for (int value = 0; value < question.Count; ++value)
        {
            (string text, string json) = PreviewMirror(action, state, question.Index, value);
            outcomes.Add((value, text));
            effects.Add(text + json);
        }

        List<(int value, string text)> distinct = outcomes.DistinctBy(outcome => outcome.text).ToList();

        if (distinct.Count > 1)
        {
            List<string> texts = distinct.Select(outcome => outcome.text).ToList();
            (string before, List<string> changed, string after) = DiffMirrorTexts(texts);

            if (changed.All(text => text.Length <= MirrorQuestion.ShortLabelLength))
            {
                // Same screen, only a word or number changes: one shared context line and short buttons
                return new MirrorQuestion
                {
                    Index   = question.Index,
                    Count   = question.Count,
                    Retry   = action,
                    Options = distinct.Select((outcome, x) => new MirrorOption(outcome.value, changed[x])).ToList(),
                    Before  = before,
                    After   = after
                };
            }

            // Otherwise show each option against the most similar other option, so a changed number stays a short
            // highlight even when another option is a completely different screen
            List<MirrorOption> options = new();

            for (int x = 0; x < texts.Count; ++x)
            {
                int    self    = x;
                string closest = texts.Where((_, y) => y != self).MaxBy(other => MirrorSimilarity(texts[self], other))!;
                (string optionBefore, List<string> optionChanged, string optionAfter) = DiffMirrorTexts([texts[x], closest]);
                options.Add(new MirrorOption(distinct[x].value, optionChanged[0], optionBefore, optionAfter));
            }

            return new MirrorQuestion
            {
                Index   = question.Index,
                Count   = question.Count,
                Retry   = action,
                Options = options
            };
        }

        if (effects.Count == 1) return null;

        // Same screen for every value but the game state differs: the official app only shows it later
        Console.WriteLine($"Mirror: random slot {question.Index} is not visible on the current screen");

        return new MirrorQuestion
        {
            Index   = question.Index,
            Count   = question.Count,
            Retry   = action,
            Options = Enumerable.Range(0, question.Count).Select(value => new MirrorOption(value, question.ValueLabel(value))).DistinctBy(option => option.Changed).ToList(),
            Hidden  = true
        };
    }

    // Screen text for one value of the asked draw. Other unanswered draws on the same screen are filled in twice with
    // different values; words that change between the two runs belong to those draws and are masked with "…"
    private (string text, string json) PreviewMirror(Action action, MirrorState state, int index, int value)
    {
        (string text, string json) = SimulateMirror(action, state, index, value, 0);
        (string other, _)          = SimulateMirror(action, state, index, value, 1);
        return (text == other ? text : MaskMirrorText(text, other), json);
    }

    private (string text, string json) SimulateMirror(Action action, MirrorState state, int index, int value, int otherDraws)
    {
        RestoreMirrorState(state);
        TheCostOfDiseaseVars.RandomArray[index] = value;
        MirrorSimulationValue                   = otherDraws;
        IsMirrorSimulating                      = true;

        try
        {
            action();
            TheCostOfDiseaseVars.RandomArray[index] = -1;
            return (MirrorScreenText(), JsonConvert.SerializeObject(this));
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Mirror: preview of random slot {index} = {value} failed: {exception}");
            return ($"#{value}", string.Empty);
        }
        finally
        {
            IsMirrorSimulating = false;
        }
    }

    // Replaces the words of text that are not in the longest common word subsequence with other by "…"
    private static string MaskMirrorText(string text, string other)
    {
        string[] a = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        string[] b = other.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        int[,] common = new int[a.Length + 1, b.Length + 1];

        for (int x = a.Length - 1; x >= 0; --x)
        for (int y = b.Length - 1; y >= 0; --y)
            common[x, y] = a[x] == b[y] ? common[x + 1, y + 1] + 1 : Math.Max(common[x + 1, y], common[x, y + 1]);

        List<string> result = new();
        int          i      = 0;
        int          j      = 0;

        while (i < a.Length)
        {
            if (j < b.Length && a[i] == b[j])
            {
                result.Add(a[i]);
                ++i;
                ++j;
            }
            else if (j < b.Length && common[i, j + 1] > common[i + 1, j])
            {
                ++j;
            }
            else
            {
                if (result.Count == 0 || result[^1] != "…") result.Add("…");
                ++i;
            }
        }

        return string.Join(" ", result);
    }

    // What Gameplay.razor would show, as plain text
    private string MirrorScreenText()
    {
        List<string> parts = new();

        if (ActivePopup != null)
        {
            parts.Add(ActivePopup.Title);
            parts.Add(ActivePopup.Description);
        }
        else if (ActiveInputPopup != null)
        {
            parts.Add(ActiveInputPopup.Message);
        }
        else if (ActiveWindow != null)
        {
            parts.Add(ActiveWindow.Title);
            parts.Add(ActiveWindow.Subtitle);
            parts.AddRange(ActiveWindow.Elements.Select(element => element.Callback == null ? element.Content : $"[{element.Content}]"));
        }
        else if (ActiveHub != null)
        {
            parts.Add(ActiveHub.Title);
            parts.Add(ActiveHub.Subtitle);

            foreach (GameplayHubSection section in ActiveHub.Sections.Where(section => section.ShouldShow()))
            {
                parts.Add(section.Title);
                parts.AddRange(section.Elements.Select(element => element.Callback == null ? element.Content : $"[{element.Content}]"));
            }
        }

        string text = string.Join(" ", parts);
        text = Regex.Replace(text, "<[^>]+>", " ");
        return WebUtility.HtmlDecode(text);
    }

    // Word-level common prefix / suffix; returns the shared context around the change and the changed words per text
    private static (string before, List<string> changed, string after) DiffMirrorTexts(List<string> texts)
    {
        List<string[]> words     = texts.Select(text => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToList();
        int            minLength = words.Min(list => list.Length);

        int prefix = 0;
        while (prefix < minLength && words.All(list => list[prefix] == words[0][prefix])) ++prefix;

        int suffix = 0;
        while (suffix < minLength - prefix && words.All(list => list[^(suffix + 1)] == words[0][^(suffix + 1)])) ++suffix;

        string before = string.Join(" ", words[0].Skip(Math.Max(prefix - _MIRROR_CONTEXT_WORDS, 0)).Take(Math.Min(prefix, _MIRROR_CONTEXT_WORDS)));
        if (prefix > _MIRROR_CONTEXT_WORDS) before = "… " + before;

        string after = string.Join(" ", words[0].Skip(words[0].Length - suffix).Take(_MIRROR_CONTEXT_WORDS));
        if (suffix > _MIRROR_CONTEXT_WORDS) after += " …";

        List<string> changed = words.Select(list =>
        {
            string[] middle = list.Skip(prefix).Take(list.Length - prefix - suffix).ToArray();
            string   result = string.Join(" ", middle.Take(_MIRROR_CHANGED_WORDS));
            return middle.Length > _MIRROR_CHANGED_WORDS ? result + " …" : result;
        }).ToList();

        return (before, changed, after);
    }

    // Number of words shared at the start and end
    private static int MirrorSimilarity(string first, string second)
    {
        string[] a      = first.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        string[] b      = second.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        int      length = Math.Min(a.Length, b.Length);

        int prefix = 0;
        while (prefix < length && a[prefix] == b[prefix]) ++prefix;

        int suffix = 0;
        while (suffix < length - prefix && a[^(suffix + 1)] == b[^(suffix + 1)]) ++suffix;

        return prefix + suffix;
    }

    private MirrorState CaptureMirrorState()
    {
        return new MirrorState
        {
            Json       = JsonConvert.SerializeObject(this),
            TmpValues  = new Dictionary<string, object>(TmpValues),
            Hub        = ActiveHub,
            Window     = ActiveWindow,
            Popup      = ActivePopup,
            InputPopup = ActiveInputPopup,
            UndoCount  = UndoStack.Count
        };
    }

    private void RestoreMirrorState(MirrorState state)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Reuse,
            NullValueHandling      = NullValueHandling.Ignore
        };
        JsonConvert.PopulateObject(state.Json, this, settings);

        // TmpValues holds typed values (int, string) that do not survive a JSON round trip
        TmpValues.Clear();
        foreach (KeyValuePair<string, object> pair in state.TmpValues) TmpValues[pair.Key] = pair.Value;

        ActiveHub        = state.Hub;
        ActiveWindow     = state.Window;
        ActivePopup      = state.Popup;
        ActiveInputPopup = state.InputPopup;
        ActiveHub?.SetSubtitle(Years); // the end of round popup changes it before calling the next event

        while (UndoStack.Count > state.UndoCount) UndoStack.Pop();
    }
}
