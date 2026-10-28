using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace MyFathersWorkWebApp;

// Fan-made Oliver narration of all story text (voice setting Oliver; docs/AUDIO_TRANSITIONS_MAP.md 8.4). The clips are
// made by tools/narration/chunks.py: the story text of a tag is cut at "<b>CHOOSE" and split around its placeholders;
// every static chunk is one clip named after the FNV-1a 64 hash of its CSV text, and the text a placeholder was formatted
// with (an either/or variant, an NPC name) is looked up the same way ("value clip"; player names have none and are
// skipped). wwwroot/audio/vo/oliver/<Scenario>.json lists the clips: hash -> script hash ("" = silent chunk). Keep the
// regex, the cut and the hash identical to chunks.py.
public static class Narration
{
    private const string Cut = "<b>CHOOSE";

    private static readonly Regex Placeholder = new(@"\{\{\d+=(?:[^{}]|\{\{[^{}]*\}\})*\}\}", RegexOptions.Compiled);

    private sealed class Manifest
    {
        public Dictionary<string, string> Clips   { get; set; } = new();
        public List<string>               Private { get; set; } = new();
    }

    private static readonly Dictionary<ScenarioId, Manifest> Manifests = new();

    private static string Folder(ScenarioId scenario) => scenario switch
    {
        ScenarioId.CostOfDisease => "TheCostOfDisease",
        ScenarioId.FearOfUnknown => "FearOfTheUnknown",
        ScenarioId.TimeOfWar     => "ATimeOfWar",
        _                        => string.Empty
    };

    // Loads the scenario's clip list once (a missing list means: no Oliver narration yet)
    public static async Task LoadAsync(HttpClient http, ScenarioId scenario)
    {
        if (Manifests.ContainsKey(scenario) || Folder(scenario) == string.Empty) return;

        try
        {
            Manifests[scenario] = await http.GetFromJsonAsync<Manifest>($"audio/vo/oliver/{Folder(scenario)}.json") ?? new Manifest();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Oliver narration list not loaded: {exception.Message}");
            Manifests[scenario] = new Manifest();
        }
    }

    // The clip urls of a window in play order, or null when some speaking chunk of its story text has no clip (not
    // rendered yet, another text language, or text changed by code in a way the split cannot follow).
    public static List<string>? Clips(GlobalData globalData, GameplayWindow window)
    {
        if (window.StoryTexts.Count == 0 || !Manifests.TryGetValue(globalData.ScenarioId, out Manifest? manifest) || manifest.Clips.Count == 0) return null;

        string        folder = Folder(globalData.ScenarioId);
        List<string>  urls   = new();
        foreach ((string tag, string text) in window.StoryTexts)
        {
            if (!globalData.Scenario_Localization.TryGetValue(tag, out string? source) || !AddTag(manifest, folder, source, text, urls))
            {
                Console.WriteLine($"Oliver narration: no clips for {tag}");
                return null;
            }
        }

        return urls.Count > 0 ? urls : null;
    }

    // Private sections (one player reads alone): narrated, but only played when the player presses Play
    public static bool IsPrivate(GlobalData globalData, GameplayWindow window) =>
        Manifests.TryGetValue(globalData.ScenarioId, out Manifest? manifest) && window.StoryTexts.Any(story => manifest.Private.Contains(story.Tag));

    private static bool AddTag(Manifest manifest, string folder, string source, string rendered, List<string> urls)
    {
        source   = Before(source,   Cut);
        rendered = Before(rendered, Cut);

        int position     = 0;
        int placeholders = 0;
        int last         = 0;
        foreach (Match? match in Placeholder.Matches(source).Append(null))
        {
            string chunk = match == null ? source[last..] : source[last..match.Index];
            if (match != null) last = match.Index + match.Length;

            if (chunk != string.Empty)
            {
                int index = Find(manifest, rendered, chunk, position, placeholders);
                if (index < 0) return false;
                if (placeholders > 0) AddValue(manifest, folder, rendered[position..index], urls);
                if (!manifest.Clips.TryGetValue(Hash(chunk), out string? script)) return false;
                if (script != string.Empty) urls.Add(Url(folder, chunk, script));
                position     = index + chunk.Length;
                placeholders = 0;
            }

            if (match != null) ++placeholders;
        }

        if (placeholders > 0) AddValue(manifest, folder, rendered[position..], urls);
        return true;
    }

    // Where a static chunk follows in the rendered text. After one placeholder, a short chunk (" " between two either/or
    // sentences) can also occur inside the value: prefer the first position where the value before it has a clip, else
    // the first occurrence (player names have no clip).
    private static int Find(Manifest manifest, string rendered, string chunk, int position, int placeholders)
    {
        int first = rendered.IndexOf(chunk, position, StringComparison.Ordinal);
        if (placeholders != 1) return first;

        for (int index = first; index >= 0; index = rendered.IndexOf(chunk, index + 1, StringComparison.Ordinal))
        {
            if (manifest.Clips.ContainsKey(Hash(rendered[position..index]))) return index;
        }

        return first;
    }

    // A placeholder's value is narrated when it has a clip (a fixed variant or name); anything else (player names) is skipped
    private static void AddValue(Manifest manifest, string folder, string value, List<string> urls)
    {
        if (value != string.Empty && manifest.Clips.TryGetValue(Hash(value), out string? script) && script != string.Empty) urls.Add(Url(folder, value, script));
    }

    private static string Url(string folder, string text, string script) => $"audio/vo/oliver/{folder}/{Hash(text)}.mp3?v={script}";

    private static string Before(string text, string cut)
    {
        int index = text.IndexOf(cut, StringComparison.Ordinal);
        return index < 0 ? text : text[..index];
    }

    public static string Hash(string text)
    {
        ulong hash = 0xcbf29ce484222325;
        foreach (byte b in Encoding.UTF8.GetBytes(text))
        {
            hash ^= b;
            hash *= 0x100000001b3;
        }

        return hash.ToString("x16");
    }
}
