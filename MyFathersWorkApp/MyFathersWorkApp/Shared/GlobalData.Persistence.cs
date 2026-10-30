using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyFathersWorkApp;

// The game and its whole undo history are kept in the browser after every screen, so a reload, a closed tab or a
// language change reopens the same screen and Back still works.
//   "continue"   - base64 JSON of the newest undo point (the save code of the pause menu, loaded by Continue)
//   "continueTime" - when "continue" was last written (UTC, ISO 8601), shown in Saved Games
//   "undoMeta"   - "first|end": undo points first .. end-1 are stored
//   "undo/{i}"   - one undo point: screen method, round, JSON with RandomArray left out unless it changed since i-1
// When the browser storage is full the oldest undo points are dropped; the newest one is always kept.
// New Game and loading another game pack these keys into one saved game first (Shared/SavedGames.cs).
public partial class GlobalData
{
    private class StoredUndo
    {
        public string  M { get; set; } = string.Empty; // UndoData.Method
        public int     R { get; set; } = -1;           // UndoData.Round
        public string  J { get; set; } = string.Empty; // UndoData.JsonData, RandomArray as null when A is null
        public string? A { get; set; }                 // UndoData.RandomJson when it differs from the previous point
    }

    private const string _CONTINUE_KEY  = "continue";
    private const string _CONTINUE_TIME_KEY = "continueTime";
    private const string _UNDO_META_KEY = "undoMeta";
    private const string _RANDOM_PREFIX = "\"RandomArray\":";
    private const string _RANDOM_NULL   = "\"RandomArray\":null";

    // Ids of the undo points already in the browser, by stack position (bottom first); positions below _StoredFirst are not stored
    [JsonIgnore] private readonly List<long> _StoredIds = new();
    [JsonIgnore] private int  _StoredFirst;
    [JsonIgnore] private int  _StorageOffset; // storage index of stack position 0
    [JsonIgnore] private bool _Persisting;
    [JsonIgnore] private bool _PersistAgain;

    private static string UndoKey(int index) => $"undo/{index}";

    // Called after every render; writes only what changed since the last call
    public async Task PersistAsync(IJSRuntime js)
    {
        if (_Persisting)
        {
            _PersistAgain = true;
            return;
        }

        _Persisting = true;

        try
        {
            do
            {
                _PersistAgain = false;
                await PersistChangesAsync(js);
            } while (_PersistAgain);
        }
        catch (JSException e)
        {
            Console.WriteLine($"Saving failed: {e.Message}");
        }
        finally
        {
            _Persisting = false;
        }
    }

    private async Task PersistChangesAsync(IJSRuntime js)
    {
        UndoData[] points = UndoStack.Reverse().ToArray();
        if (points.Length == 0) return;

        int same = 0;
        while (same < points.Length && same < _StoredIds.Count && points[same].Id == _StoredIds[same]) same++;
        if (same == points.Length && same == _StoredIds.Count) return;

        int first = Math.Min(_StoredFirst, same);

        while (true)
        {
            try
            {
                await WriteUndoPointsAsync(js, points, Math.Max(same, first), first);
                break;
            }
            catch (JSException) when (first < points.Length - 1)
            {
                // Storage full: drop the older half and write the rest again
                int dropTo = first + Math.Max(1, (points.Length - first) / 2);
                dropTo = Math.Min(dropTo, points.Length - 1);
                for (int i = _StoredFirst; i < dropTo; i++) await js.InvokeVoidAsync("localStorage.removeItem", UndoKey(_StorageOffset + i));
                Console.WriteLine($"Browser storage full: kept the newest {points.Length - dropTo} undo points");
                first = _StoredFirst = dropTo;
                same  = first;
            }
        }
    }

    private async Task WriteUndoPointsAsync(IJSRuntime js, UndoData[] points, int from, int first)
    {
        for (int i = from; i < points.Length; i++)
        {
            UndoData point   = points[i];
            bool     keepRng = i == first || point.RandomJson != points[i - 1].RandomJson;
            StoredUndo stored = new StoredUndo
            {
                M = point.Method,
                R = point.Round,
                J = keepRng ? point.JsonData : point.JsonData.Replace(_RANDOM_PREFIX + point.RandomJson, _RANDOM_NULL),
                A = keepRng ? point.RandomJson : null
            };
            await js.InvokeVoidAsync("localStorage.setItem", UndoKey(_StorageOffset + i), JsonConvert.SerializeObject(stored));
        }

        for (int i = points.Length; i < _StoredIds.Count; i++) await js.InvokeVoidAsync("localStorage.removeItem", UndoKey(_StorageOffset + i));

        await js.InvokeVoidAsync("localStorage.setItem", _UNDO_META_KEY, $"{_StorageOffset + first}|{_StorageOffset + points.Length}");
        await js.InvokeVoidAsync("localStorage.setItem", _CONTINUE_KEY, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(WithCurrentLanguage(points[^1].JsonData))));
        await js.InvokeVoidAsync("localStorage.setItem", _CONTINUE_TIME_KEY, DateTime.UtcNow.ToString("o"));

        _StoredFirst = first;
        _StoredIds.Clear();
        _StoredIds.AddRange(points.Select(point => point.Id));
    }

    // An undo point can be older than the language picked in the pause menu; "continue" keeps the current one, as Undo does
    private string WithCurrentLanguage(string json)
    {
        JObject jo = JObject.Parse(json);
        if ((string?)jo[nameof(Language)] == Language && (string?)jo[nameof(ScenarioLanguage)] == ScenarioLanguage) return json;

        jo[nameof(Language)]         = Language;
        jo[nameof(ScenarioLanguage)] = ScenarioLanguage;
        return jo.ToString(Formatting.None);
    }

    // New game (after SavedGamesService.ArchiveCurrentAsync kept a copy): forget the game being played and its undo history
    public async Task ClearSavedGameAsync(IJSRuntime js)
    {
        UndoStack.Clear();
        RedoStack.Clear();
        await js.InvokeVoidAsync("localStorage.removeItem", _CONTINUE_KEY);
        await js.InvokeVoidAsync("localStorage.removeItem", _CONTINUE_TIME_KEY);
        await ClearStoredUndoAsync(js);
    }

    private async Task ClearStoredUndoAsync(IJSRuntime js)
    {
        (int first, int end) = await ReadUndoMetaAsync(js);
        for (int i = first; i < end; i++) await js.InvokeVoidAsync("localStorage.removeItem", UndoKey(i));
        await js.InvokeVoidAsync("localStorage.removeItem", _UNDO_META_KEY);

        _StoredIds.Clear();
        _StoredFirst   = 0;
        _StorageOffset = 0;
    }

    private static async Task<(int first, int end)> ReadUndoMetaAsync(IJSRuntime js)
    {
        string?   meta  = await js.InvokeAsync<string?>("localStorage.getItem", _UNDO_META_KEY);
        string[]? parts = meta?.Split('|');
        if (parts is not { Length: 2 } || !int.TryParse(parts[0], out int first) || !int.TryParse(parts[1], out int end)) return (0, 0);
        return (first, end);
    }

    // After LoadGameAsync: puts the stored undo history back when it belongs to the loaded save, otherwise starts a new one
    private async Task RestoreUndoHistoryAsync(IJSRuntime js, bool fromBrowser, string json, Action<GlobalData>? screen)
    {
        List<UndoData> history = fromBrowser ? await ReadStoredUndoAsync(js) : new List<UndoData>();

        if (history.Count == 0 || !IsSameUndoPoint(history[^1].JsonData))
        {
            await ClearStoredUndoAsync(js);
            UndoStack.Push(NewUndoData(screen, ResumeMethod, json, screen == null ? UndoRound : -1));
            return;
        }

        history[^1].JsonData = json; // "continue" may have a newer language (pause menu)
        foreach (UndoData point in history) UndoStack.Push(point);
        _StoredIds.Clear();
        _StoredIds.AddRange(history.Select(point => point.Id));
        _StoredFirst = 0;
    }

    private async Task<List<UndoData>> ReadStoredUndoAsync(IJSRuntime js)
    {
        List<UndoData> history = new();
        (int first, int end) = await ReadUndoMetaAsync(js);
        _StorageOffset = first;

        string randomJson = string.Empty;

        for (int i = first; i < end; i++)
        {
            string?     text   = await js.InvokeAsync<string?>("localStorage.getItem", UndoKey(i));
            StoredUndo? stored = text == null ? null : JsonConvert.DeserializeObject<StoredUndo>(text);

            // A gap or a point without its random values: keep only what comes after it
            if (stored == null || (stored.A == null && randomJson == string.Empty))
            {
                history.Clear();
                _StorageOffset = i + 1;
                randomJson     = string.Empty;
                continue;
            }

            if (stored.A != null) randomJson = stored.A;

            Action<GlobalData>? callback = ResolveScreen(stored.M);
            history.Add(new UndoData
            {
                Callback   = callback,
                Method     = callback == null ? string.Empty : stored.M,
                JsonData   = stored.A == null ? stored.J.Replace(_RANDOM_NULL, _RANDOM_PREFIX + randomJson) : stored.J,
                RandomJson = randomJson,
                Round      = stored.R,
                Id         = ++_NextUndoId
            });
        }

        return history;
    }

    // Whether a stored undo point is the state just loaded from "continue" (and not left over from another game or a crafted save)
    private bool IsSameUndoPoint(string json)
    {
        try
        {
            JObject jo   = JObject.Parse(json);
            long?   seed = jo.Descendants().OfType<JProperty>().FirstOrDefault(p => p.Name == nameof(ScenarioVars.FlavorSeed))?.Value.Value<long>();

            return (int?)jo[nameof(ScenarioId)] == (int)ScenarioId && (int?)jo[nameof(Generation)] == (int)Generation && (int?)jo[nameof(Years)] == (int)Years &&
                   ((string?)jo[nameof(ResumeMethod)] ?? string.Empty) == ResumeMethod && seed == ScenarioVars.FlavorSeed;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
