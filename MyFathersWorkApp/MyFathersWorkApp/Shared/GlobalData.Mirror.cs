using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MyFathersWorkApp;

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
        RunScenarioActionCore(() =>
        {
            action();
            RefreshShownHub();
        });
    }

    // Hub text is formatted when the hub is built. When an event chain ends on the hub (no window or popup left), build
    // it again so text that depends on state changed during the chain (Years, drawn buildings, ...) is current.
    private void RefreshShownHub()
    {
        if (ActiveHub == null || ActiveWindow != null || ActivePopup != null || ActiveInputPopup != null) return;
        GetHubMethod()?.Invoke(this);
    }

    private void RunScenarioActionCore(Action action)
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
                ScenarioVars.RandomArray[question.Index] = 0;
                RunScenarioActionCore(action);
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
        ScenarioVars.RandomArray[question.Index] = value;
        RunScenarioActionCore(question.Retry);
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
            MirrorPreview preview0 = PreviewMirror(action, state, question.Index, 0);
            MirrorPreview preview1 = PreviewMirror(action, state, question.Index, 1);
            (string before, _, string after) = DiffMirrorTexts([preview0.Text, preview1.Text]);

            return new MirrorQuestion
            {
                Index      = question.Index,
                Count      = question.Count,
                Retry      = action,
                Options    = [],
                Before     = before,
                After      = after,
                NumericMin = question.NumericMin,
                Heading    = MirrorHeading([preview0, preview1])
            };
        }

        List<(int value, string text, MirrorPreview preview)> outcomes = new();
        HashSet<string>                                       effects  = new();

        for (int value = 0; value < question.Count; ++value)
        {
            MirrorPreview preview = PreviewMirror(action, state, question.Index, value);
            outcomes.Add((value, preview.Text, preview));
            effects.Add(preview.Text + preview.Json);
        }

        List<(int value, string text, MirrorPreview preview)> distinct = outcomes.DistinctBy(outcome => outcome.text).ToList();
        string                                                heading  = MirrorHeading(distinct.Select(outcome => outcome.preview).ToList());

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
                    After   = after,
                    Heading = heading
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
                Options = options,
                Heading = heading
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
            Hidden  = true,
            Heading = outcomes[0].preview.Headings.FirstOrDefault(title => title != string.Empty) ?? string.Empty
        };
    }

    // Text: the screen shown for one value of the asked draw, with the words of other unanswered draws masked.
    // Words / Headings: the unmasked words and, per word, the title of the part of the screen it is in.
    private record MirrorPreview(string Text, string Json, string[] Words, string[] Headings);

    // Screen text for one value of the asked draw. Other unanswered draws on the same screen are filled in twice with
    // different values; words that change between the two runs belong to those draws and are masked with "…"
    private MirrorPreview PreviewMirror(Action action, MirrorState state, int index, int value)
    {
        (List<(string word, string heading)> words, string json) = SimulateMirror(action, state, index, value, 0);
        (List<(string word, string heading)> other, _)           = SimulateMirror(action, state, index, value, 1);

        string text      = string.Join(" ", words.Select(word => word.word));
        string otherText = string.Join(" ", other.Select(word => word.word));
        return new MirrorPreview(text == otherText ? text : MaskMirrorText(text, otherText), json,
            words.Select(word => word.word).ToArray(), words.Select(word => word.heading).ToArray());
    }

    // Title of the part of the screen (window/popup title, hub section title) where the options start to differ, so the
    // players know which page of the official app to look at. Empty when the title itself differs (it is then in the options).
    private static string MirrorHeading(List<MirrorPreview> previews)
    {
        if (previews.Count == 0 || previews.Any(preview => preview.Words.Length == 0)) return string.Empty;

        int minLength = previews.Min(preview => preview.Words.Length);
        int prefix    = 0;
        while (prefix < minLength && previews.All(preview => preview.Words[prefix] == previews[0].Words[prefix])) ++prefix;

        string heading = previews[0].Headings[Math.Min(prefix, previews[0].Words.Length - 1)];
        return previews.All(preview => preview.Headings[Math.Min(prefix, preview.Words.Length - 1)] == heading) ? heading : string.Empty;
    }

    private (List<(string word, string heading)> words, string json) SimulateMirror(Action action, MirrorState state, int index, int value, int otherDraws)
    {
        RestoreMirrorState(state);
        ScenarioVars.RandomArray[index] = value;
        MirrorSimulationValue                   = otherDraws;
        IsMirrorSimulating                      = true;

        try
        {
            action();
            ScenarioVars.RandomArray[index] = -1;
            return (MirrorScreenWords(), JsonConvert.SerializeObject(this));
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Mirror: preview of random slot {index} = {value} failed: {exception}");
            return ([($"#{value}", string.Empty)], string.Empty);
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

    // What Gameplay.razor would show, as plain words, each with the title of the part of the screen it is in
    private List<(string word, string heading)> MirrorScreenWords()
    {
        List<(string heading, string text)> parts = new();

        void Add(string heading, IEnumerable<string> texts)
        {
            foreach (string text in texts) parts.Add((heading, text));
        }

        IEnumerable<string> Elements(List<GameplayElement> elements) => elements.Select(element => element.Callback == null ? element.Content : $"[{element.Content}]");

        if (ActivePopup != null)
        {
            Add(ActivePopup.Title, [ActivePopup.Title, ActivePopup.Description]);
        }
        else if (ActiveInputPopup != null)
        {
            Add(string.Empty, [ActiveInputPopup.Message]);
        }
        else if (ActiveWindow != null)
        {
            Add(ActiveWindow.Title, [ActiveWindow.Title, ActiveWindow.Subtitle]);
            Add(ActiveWindow.Title, Elements(ActiveWindow.Elements));
        }
        else if (ActiveHub != null)
        {
            Add(ActiveHub.Title, [ActiveHub.Title, ActiveHub.Subtitle]);

            foreach (GameplayHubSection section in ActiveHub.Sections.Where(section => section.ShouldShow()))
            {
                string heading = section.Title != string.Empty ? section.Title : ActiveHub.Title;
                Add(heading, [section.Title]);
                Add(heading, Elements(section.Elements));
            }
        }

        return parts.SelectMany(part => Words(part.text).Select(word => (word, string.Join(" ", Words(part.heading))))).ToList();
    }

    private static string[] Words(string text)
    {
        return WebUtility.HtmlDecode(Regex.Replace(text, "<[^>]+>", " ")).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
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
