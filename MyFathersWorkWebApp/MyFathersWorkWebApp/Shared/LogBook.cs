namespace MyFathersWorkWebApp;

public record LogBookEntry(string Title, string Location, string Summary);

// Log Book of The Cost of Disease, ported from the official app (ViewLogBook.cs): an event is logged the first time one of
// its screens is shown, and the book lists the events in that order. Data: wwwroot/localization/TheCostOfDisease_LogBook.csv.
// Pages/Gameplay.razor notes the screen after it is rendered, never scenario code (mirror sandbox, undo replays). The logged
// titles live in TheCostOfDiseaseVars.LogBook: undo forgets the screens it goes back over, load and New Game behave like
// the rest of the game state (Unity cleared the log book on New Game too).
public class LogBookService(HttpClient http)
{
    private const string _CSV_PATH = "localization/TheCostOfDisease_LogBook.csv";

    private readonly Dictionary<string, List<LogBookEntry>> _ByScreen = new();
    private readonly Dictionary<string, LogBookEntry>       _ByTitle  = new();

    private Task? _Loading;

    public static bool HasLogBook(GlobalData globalData) => globalData.ScenarioId == ScenarioId.CostOfDisease;

    public Task LoadAsync() => _Loading ??= LoadCoreAsync();

    private async Task LoadCoreAsync()
    {
        try
        {
            string csv = await http.GetStringAsync(_CSV_PATH);

            foreach (string rawLine in csv.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r').TrimStart('﻿');
                if (line.Length == 0 || line.StartsWith("//")) continue;

                // Screens;Title;Location;Summary (the header line is skipped)
                string[] parts = line.Split(';', 4);
                if (parts.Length < 4 || parts[0] == "Screens") continue;

                LogBookEntry entry = new(parts[1], parts[2], parts[3]);
                _ByTitle[entry.Title] = entry;
                foreach (string screen in parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!_ByScreen.TryGetValue(screen, out List<LogBookEntry>? entries)) _ByScreen[screen] = entries = [];
                    entries.Add(entry);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Log Book: {e.Message}");
        }
    }

    // The entry of the screen on display (a window or popup, not a hub): its screen method (ResumeMethod, the caller of
    // SaveToUndo). A screen with several entries (the first / second night of the hunt) logs the one named in the window title.
    public LogBookEntry? CurrentEntry(GlobalData globalData)
    {
        if (!HasLogBook(globalData) || (globalData.ActiveWindow == null && globalData.ActivePopup == null && globalData.ActiveInputPopup == null)) return null;
        if (!_ByScreen.TryGetValue(globalData.ResumeMethod, out List<LogBookEntry>? entries)) return null;
        if (entries.Count == 1) return entries[0];

        string title = globalData.ActiveWindow?.Title ?? string.Empty;
        return entries.FirstOrDefault(entry => title.Contains(entry.Title, StringComparison.OrdinalIgnoreCase)) ?? entries[0];
    }

    // Logs the screen on display; each event once, as in Unity
    public void Note(GlobalData globalData)
    {
        if (globalData.ActiveMirrorQuestion != null) return;

        LogBookEntry? entry = CurrentEntry(globalData);
        if (entry == null) return;

        TheCostOfDiseaseVars vars = globalData.TheCostOfDiseaseVars;
        if (!vars.LogBook.Contains(entry.Title)) vars.LogBook = [..vars.LogBook, entry.Title];
    }

    // The logged events, oldest first. Like Unity, the event on display is left out while it is the newest one.
    public List<LogBookEntry> Entries(GlobalData globalData)
    {
        if (!HasLogBook(globalData)) return [];

        List<LogBookEntry> entries = globalData.TheCostOfDiseaseVars.LogBook
                                               .Where(title => _ByTitle.ContainsKey(title))
                                               .Select(title => _ByTitle[title] with { Summary = _ByTitle[title].Summary.Replace("Xvillage", globalData.TownName) })
                                               .ToList();

        if (entries.Count > 0 && CurrentEntry(globalData)?.Title == entries[^1].Title) entries.RemoveAt(entries.Count - 1);
        return entries;
    }
}
