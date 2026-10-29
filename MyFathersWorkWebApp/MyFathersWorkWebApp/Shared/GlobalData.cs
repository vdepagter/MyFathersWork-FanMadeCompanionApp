using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace MyFathersWorkWebApp;

public partial class GlobalData
{
    private class UndoData
    {
        public Action<GlobalData>? Callback   { get; set; }
        public string              Method     { get; set; } = string.Empty; // name of Callback, kept in the browser (GlobalData.Persistence.cs)
        public string              JsonData   { get; set; } = string.Empty;
        public string              RandomJson { get; set; } = string.Empty; // RandomArray as JSON, stored only when it changes
        public int                 Round      { get; set; } = -1; // hub saves only: (Generation, Years) of the shown hub
        public long                Id         { get; set; }
    }

    private int UndoRound => (int)Generation * 3 + (int)Years;

    [JsonIgnore] public Dictionary<string, Dictionary<string, string>> UI_Localization                { get; }      = new();
    [JsonIgnore] public Dictionary<string, string>                     Scenario_Localization          { get; set; } = new();
    [JsonIgnore] public Dictionary<string, string>                     Scenario_Gameplay_Localization { get; set; } = new();
    [JsonIgnore] public List<string>                                   OtherPossibleScenarioLanguages { get; set; } = new();

    public string Language         { get; set; } = "English";
    public string ScenarioLanguage { get; set; } = "English_Source";

    public ScenarioId                 ScenarioId           { get; set; }
    public int                        PlayersNum           { get; set; }
    public string                     PlayerAName          { get; set; } = string.Empty;
    public string                     PlayerBName          { get; set; } = string.Empty;
    public string                     PlayerCName          { get; set; } = string.Empty;
    public string                     PlayerDName          { get; set; } = string.Empty;
    public string                     TownName             { get; set; } = string.Empty;
    public Years                      Years                { get; set; } = Years.Early;
    public Generation                 Generation           { get; set; } = Generation.First;
    public TheCostOfDiseaseVars       TheCostOfDiseaseVars { get; }      = new();
    public FearOfTheUnknownVars       FearOfTheUnknownVars { get; }      = new();
    public ATimeOfWarVars             ATimeOfWarVars       { get; }      = new();
    public Dictionary<string, object> TmpValues            { get; }      = new();

    // Screen this state is shown on (the method that called SaveToUndo), empty on a hub. Load reopens it.
    public string ResumeMethod { get; set; } = string.Empty;

    // The active scenario's random values and final scoring state
    [JsonIgnore] public ScenarioVars ScenarioVars => ScenarioId switch
    {
        ScenarioId.FearOfUnknown => FearOfTheUnknownVars,
        ScenarioId.TimeOfWar     => ATimeOfWarVars,
        _                        => TheCostOfDiseaseVars
    };

    // Only the active scenario's state is saved (keeps saves and undo snapshots small)
    public bool ShouldSerializeTheCostOfDiseaseVars() => ScenarioId is not (ScenarioId.FearOfUnknown or ScenarioId.TimeOfWar);
    public bool ShouldSerializeFearOfTheUnknownVars() => ScenarioId == ScenarioId.FearOfUnknown;
    public bool ShouldSerializeATimeOfWarVars()       => ScenarioId == ScenarioId.TimeOfWar;

    [JsonIgnore] public GameplayHub?        ActiveHub        { get; set; }
    [JsonIgnore] public GameplayWindow?     ActiveWindow     { get; set; }
    [JsonIgnore] public GameplayPopup?      ActivePopup      { get; set; }
    [JsonIgnore] public GameplayInputPopup? ActiveInputPopup { get; set; }

    [JsonIgnore] private Stack<UndoData> UndoStack { get; } = new();

    [JsonIgnore] public bool IsUndoDisabled => ActiveMirrorQuestion == null && UndoStack.Count <= 1;

    [JsonIgnore] private bool _UndoInProgress;
    [JsonIgnore] private long _NextUndoId;

    public GlobalData()
    {
        ResetScenarioVars();
    }

    public void ResetScenarioVars()
    {
        TheCostOfDiseaseVars.Reset(this);
        FearOfTheUnknownVars.Reset(this);
        ATimeOfWarVars.Reset(this);
    }

    // Static class holding the active scenario's screens (SaveToUndo looks methods up by name)
    private Type? ScenarioType => ScenarioId switch
    {
        ScenarioId.CostOfDisease => typeof(TheCostOfDisease),
        ScenarioId.FearOfUnknown => typeof(FearOfTheUnknown),
        ScenarioId.TimeOfWar     => typeof(ATimeOfWar),
        _                        => null
    };

    public string GetLocalizedUITag(string tag)
    {
        // A menu language that no longer exists (Polish was removed) falls back to English
        if (UI_Localization.TryGetValue(Language, out Dictionary<string, string>? value) || UI_Localization.TryGetValue("English", out value))
        {
            if (value.TryGetValue(tag, out string? translation)) return translation;
            return $"Missing Translation -> {Language}/{tag}";
        }

        return $"Missing Language -> {Language}";
    }

    public string GetScenarioLocalizedTag(string tag)
    {
        if (tag == string.Empty) return string.Empty;
        if (Scenario_Localization.TryGetValue(tag, out string? value)) return value;
        if (Scenario_Gameplay_Localization.TryGetValue(tag, out value)) return value;
        return $"Missing Scenario Translation -> {tag}";
    }

    public T GetTmpValue<T>(string key)
    {
        object? value = TmpValues.GetValueOrDefault(key);

        // After undo or load the values come from JSON (an int comes back as a long)
        if (value is not T && value is IConvertible) return (T)Convert.ChangeType(value, typeof(T));
        return (T)value!;
    }

    public void SaveToUndo([CallerMemberName] string methodName = "")
    {
        if (_UndoInProgress) return;

        if (ScenarioType == null) return;

        ResumeMethod = methodName;
        UndoStack.Push(NewUndoData(ResolveScreen(methodName)!, methodName, JsonConvert.SerializeObject(this), -1));
    }

    // A screen method by name, see SaveToUndo
    private Action<GlobalData>? ResolveScreen(string methodName)
    {
        if (ScenarioType == null || methodName == string.Empty) return null;

        const BindingFlags flags  = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo?        method = ScenarioType.GetMethod(methodName, flags) ?? typeof(FinalScoring).GetMethod(methodName, flags);
        return method == null ? null : (Action<GlobalData>)Delegate.CreateDelegate(typeof(Action<GlobalData>), method);
    }

    private UndoData NewUndoData(Action<GlobalData>? callback, string method, string json, int round)
    {
        return new UndoData
        {
            Callback   = callback,
            Method     = callback == null ? string.Empty : method,
            JsonData   = json,
            RandomJson = JsonConvert.SerializeObject(ScenarioVars.RandomArray),
            Round      = round,
            Id         = ++_NextUndoId
        };
    }

    public int LocalizedPlayerNumberIndex()
    {
        return PlayersNum - 2;
    }

    public string GetPlayerNameByIndex(int index)
    {
        return index switch
        {
            0 => PlayerAName,
            1 => PlayerBName,
            2 => PlayerCName,
            3 => PlayerDName,
            _ => string.Empty
        };
    }

    public void ShowEndOfRoundPopUp(Action<GlobalData> nextCallback)
    {
        ActivePopup = new GameplayPopup(this, PopUpTitle.EndOfRound, PopUpIcon.MFWlogo, PopUpButton.Confirm, _ =>
            {
                Generation prevGeneration = Generation;

                if (Years == Years.Late)
                {
                    Years      =  Years.Early;
                    Generation += 1;
                }
                else
                {
                    Years += 1;
                }

                if (prevGeneration != Generation) ActiveHub = null;
                else ActiveHub!.SetSubtitle(Years);

                nextCallback(this);
            }, GlobalTags.Gameplay_EndOfRound_Content,
            content => content.FormatWithIndex(3, (int)Years + 1)
                              .FormatWithCondition(2, () => Years is Years.Early or Years.Middle)
                              .FormatWithIndex(1, (int)Generation)
                              .FormatWithIndex(0, (int)Years));
    }

    public string[] GetActivePlayers()
    {
        string[] result = new string[PlayersNum];
        result[0] = PlayerAName;
        result[1] = PlayerBName;
        if (PlayersNum >= 3) result[2] = PlayerCName;
        if (PlayersNum >= 4) result[3] = PlayerDName;
        return result;
    }

    // Player, village and typed-in names: wrapped in translate="no" so the browser's own translation leaves them alone
    public IEnumerable<string> ProtectedNames()
    {
        string[] setupNames = [PlayerAName, PlayerBName, PlayerCName, PlayerDName, TownName];
        return setupNames.Concat(ScenarioVars.TypedNames())
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Select(name => name.Trim())
                         .Distinct();
    }

    // Value for <html lang>: the story language on /Gameplay, the menu language elsewhere. Lets the browser offer
    // "Translate this page" from the right language.
    public string PageLanguageCode(bool gameplay)
    {
        string language = (gameplay ? ScenarioLanguage : Language).Replace("_Source", "");
        return _LanguageCodes.GetValueOrDefault(language, "en");
    }

    private static readonly Dictionary<string, string> _LanguageCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["English"] = "en", ["Dutch"] = "nl", ["German"] = "de", ["French"] = "fr", ["Spanish"] = "es",
        ["Italian"] = "it", ["Portuguese"] = "pt", ["Czech"] = "cs", ["Russian"] = "ru", ["Ukrainian"] = "uk",
        ["Chinese"] = "zh", ["Japanese"] = "ja", ["Korean"] = "ko"
    };

    public async Task<bool> IsContinueAvailableAsync(IJSRuntime js)
    {
        string? base64 = await js.InvokeAsync<string?>("localStorage.getItem", "continue");
        return !string.IsNullOrEmpty(base64);
    }

    public void Undo()
    {
        if (ActiveMirrorQuestion != null)
        {
            // Back from a mirror question returns to the screen it was asked from
            ActiveMirrorQuestion = null;
            if (ActiveHub != null || ActiveWindow != null || ActivePopup != null || ActiveInputPopup != null) return;
        }

        if (UndoStack.Count <= 1) return;
        _UndoInProgress = true;
        UndoStack.Pop();
        UndoData prev = UndoStack.Peek();
        ActiveHub        = null;
        ActiveWindow     = null;
        ActivePopup      = null;
        ActiveInputPopup = null;

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Reuse,
            NullValueHandling      = NullValueHandling.Ignore
        };
        // The language picked in the pause menu stays, the snapshot may be older than that choice
        string language         = Language;
        string scenarioLanguage = ScenarioLanguage;
        JsonConvert.PopulateObject(prev.JsonData, this, settings);
        Language         = language;
        ScenarioLanguage = scenarioLanguage;
        _UndoInProgress  = false;

        RunScenarioAction(() =>
        {
            _UndoInProgress = true;

            try
            {
                GetHubMethod()?.Invoke(this);
                prev.Callback?.Invoke(this);
            }
            finally
            {
                _UndoInProgress = false;
            }
        });
    }

    // Makes the shown hub an undo point. Every undo point is written to the browser by PersistAsync (GlobalData.Persistence.cs).
    public void SaveGame()
    {
        // A hub shown again in the same round (no undoable screen in between) is not saved twice. A hub of the next round
        // reached straight from the end-of-round popup is a new undo point, otherwise undo would skip back a round.
        if (_UndoInProgress || (UndoStack.Count != 0 && UndoStack.Peek().Callback == null && UndoStack.Peek().Round == UndoRound)) return;
        ResumeMethod = string.Empty;
        UndoStack.Push(NewUndoData(null, string.Empty, JsonConvert.SerializeObject(this), UndoRound));
    }

    // data: a save code from the load popup; empty: the browser's save ("continue") and, when it belongs to it, the undo history
    public async Task LoadGameAsync(string data, NavigationManager nav, HttpClient http, IJSRuntime js)
    {
        UndoStack.Clear();

        bool    fromBrowser = string.IsNullOrEmpty(data);
        string? base64      = fromBrowser ? await js.InvokeAsync<string?>("localStorage.getItem", "continue") : data;

        if (string.IsNullOrEmpty(base64))
        {
            nav.NavigateTo("");
            return;
        }

        ResetScenarioVars();
        ActiveHub            = null;
        ActiveWindow         = null;
        ActivePopup          = null;
        ActiveInputPopup     = null;
        ActiveMirrorQuestion = null;
        ResumeMethod         = string.Empty; // saves from before ResumeMethod open on the hub

        string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Reuse,
            NullValueHandling      = NullValueHandling.Ignore
        };
        JsonConvert.PopulateObject(json, this, settings);

        Dictionary<string, Dictionary<string, string>> scenarioLocalizations         = new();
        Dictionary<string, Dictionary<string, string>> scenarioGameplayLocalizations = new();

        await LoadScenarioLanguagesOptionsAsync(nav, http, scenarioLocalizations, scenarioGameplayLocalizations);
        Scenario_Localization          = scenarioLocalizations[ScenarioLanguage];
        Scenario_Gameplay_Localization = scenarioGameplayLocalizations[scenarioLocalizations[ScenarioLanguage]["Gameplay_Lang"]];

        Action<GlobalData>? activeHub = GetHubMethod();
        Action<GlobalData>? screen    = ResolveScreen(ResumeMethod);

        if (activeHub == null && screen == null)
        {
            nav.NavigateTo("");
            return;
        }

        // The screen is rebuilt the way undo does it: from the state saved before its method ran
        RunScenarioAction(() =>
        {
            _UndoInProgress = true;

            try
            {
                activeHub?.Invoke(this);
                screen?.Invoke(this);
            }
            finally
            {
                _UndoInProgress = false;
            }
        });

        await RestoreUndoHistoryAsync(js, fromBrowser, json, screen);
    }

    public async Task LoadScenarioLanguagesOptionsAsync(NavigationManager nav, HttpClient http, Dictionary<string, Dictionary<string, string>> scenarioLocalizations,
                                                        Dictionary<string, Dictionary<string, string>> scenarioGameplayLocalizations)
    {
        await Narration.LoadAsync(http, ScenarioId);

        string scenarioLocalization = ScenarioId switch
        {
            ScenarioId.CostOfDisease => "localization/TheCostOfDisease_Localization.csv",
            ScenarioId.FearOfUnknown => "localization/FearOfTheUnknown_Localization.csv",
            ScenarioId.TimeOfWar     => "localization/ATimeOfWar_Localization.csv",
            _                        => string.Empty
        };

        string scenarioGameplayLocalization = ScenarioId switch
        {
            ScenarioId.CostOfDisease => "localization/TheCostOfDisease_Gameplay_Localization.csv",
            ScenarioId.FearOfUnknown => "localization/FearOfTheUnknown_Gameplay_Localization.csv",
            ScenarioId.TimeOfWar     => "localization/ATimeOfWar_Gameplay_Localization.csv",
            _                        => string.Empty
        };

        if (scenarioLocalization == string.Empty || scenarioGameplayLocalization == string.Empty)
        {
            nav.NavigateTo("");
            return;
        }

        (await http.GetStringAsync(scenarioLocalization)).ProcessLocalizationFile(scenarioLocalizations);
        (await http.GetStringAsync(scenarioGameplayLocalization)).ProcessLocalizationFile(scenarioGameplayLocalizations);

        OtherPossibleScenarioLanguages.Clear();
        OtherPossibleScenarioLanguages.AddRange(scenarioLocalizations.Keys);
    }

    // Starts the active scenario after the setup screens
    public bool StartScenario()
    {
        Action<GlobalData>? init = ScenarioId switch
        {
            ScenarioId.CostOfDisease => TheCostOfDisease.Init,
            ScenarioId.FearOfUnknown => FearOfTheUnknown.Init,
            ScenarioId.TimeOfWar     => ATimeOfWar.Init,
            _                        => null
        };

        if (init == null) return false;
        RunScenarioAction(() => init(this));
        return true;
    }

    public void ShowFinalEnding()
    {
        switch (ScenarioId)
        {
            case ScenarioId.CostOfDisease:
            {
                TheCostOfDisease.FinalEnding(this);
                break;
            }
            case ScenarioId.FearOfUnknown:
            {
                FearOfTheUnknown.FinalEnding(this);
                break;
            }
            case ScenarioId.TimeOfWar:
            {
                ATimeOfWar.FinalEnding(this);
                break;
            }
        }
    }

    private Action<GlobalData>? GetHubMethod()
    {
        if (ScenarioId == ScenarioId.FearOfUnknown)
        {
            return FearOfTheUnknownVars.HubId switch
            {
                FearOfTheUnknownHubId.WitchHunt  => FearOfTheUnknown.WitchHunt,
                FearOfTheUnknownHubId.Foreigners => FearOfTheUnknown.Foreigners,
                FearOfTheUnknownHubId.Creature   => FearOfTheUnknown.Creature,
                FearOfTheUnknownHubId.Isolation  => FearOfTheUnknown.Isolation,
                FearOfTheUnknownHubId.Tension    => FearOfTheUnknown.Tension,
                FearOfTheUnknownHubId.Privatized => FearOfTheUnknown.Privatized,
                FearOfTheUnknownHubId.Liberal    => FearOfTheUnknown.Liberal,
                _                                => null
            };
        }

        if (ScenarioId == ScenarioId.TimeOfWar)
        {
            return ATimeOfWarVars.HubId switch
            {
                ATimeOfWarHubId.TakingSides  => ATimeOfWar.TakingSides,
                ATimeOfWarHubId.Martial      => ATimeOfWar.Martial,
                ATimeOfWarHubId.TimeTravel   => ATimeOfWar.TimeTravel,
                ATimeOfWarHubId.MonarchReign => ATimeOfWar.MonarchReign,
                ATimeOfWarHubId.Peace        => ATimeOfWar.Peace,
                ATimeOfWarHubId.Warning      => ATimeOfWar.Warning,
                ATimeOfWarHubId.Paradox      => ATimeOfWar.Paradox,
                _                            => null
            };
        }

        if (ScenarioId == ScenarioId.CostOfDisease)
        {
            return TheCostOfDiseaseVars.HubId switch
            {
                CostOfDiseaseHubId.Fever        => TheCostOfDisease.Fever,
                CostOfDiseaseHubId.Devastation  => TheCostOfDisease.Devastation,
                CostOfDiseaseHubId.Hospital     => TheCostOfDisease.Hospital,
                CostOfDiseaseHubId.GloomyGothic => TheCostOfDisease.GloomyGothic,
                CostOfDiseaseHubId.Prosperity   => TheCostOfDisease.Prosperity,
                CostOfDiseaseHubId.NoUniversity => TheCostOfDisease.NoUniversity,
                CostOfDiseaseHubId.University   => TheCostOfDisease.University,
                _                               => null
            };
        }

        return null;
    }
}