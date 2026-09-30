# My Father's Work - Fan Made Companion App

Blazor WebAssembly (.NET) re-implementation of the official *My Father's Work* companion app (a Unity app whose
stories were written in Twine/Harlowe and compiled to C# by Cradle). Players make choices on a phone/tablet next to the
board game; the app tells them what to set up and narrates the story over 3 Generations x 3 rounds.

## Status (September 2026)

| Scenario | Web app | Notes |
|---|---|---|
| The Cost of Disease (`ScenarioId.CostOfDisease`) | **Complete** - Gen I, II, III, final scoring, all 8 endings | Only English text. |
| Fear of the Unknown (`FearOfUnknown`) | **Not started** | Selecting it bounces back to the main menu. |
| A Time of War (`TimeOfWar`) | **Not started** | Same. |

## Repository layout

- `MyFathersWorkWebApp/MyFathersWorkWebApp/` - the web app.
  - `Pages/` - menu/setup flow (`Home`, `PlayersSelect` ... `PlayersScenarioLanguage`) and `Gameplay.razor` (renders the active window / hub / popup).
  - `Shared/GlobalData.cs` - all game state (serialized to JSON for save/load/undo), localization lookup, undo stack, end-of-round popup.
  - `Shared/Scenarios/Gameplay*.cs` - UI model: `GameplayWindow`, `GameplayHub` + `GameplayHubSection`, `GameplayPopup`, `GameplayInputPopup`.
  - `Shared/Scenarios/TheCostOfDisease/` - one `static partial class TheCostOfDisease`, split per generation/branch (`00_Setup`, `01_Gen1_Fever`, ..., `08_Scoring`) + `TheCostOfDiseaseVars.cs` (scenario state). The `.txt` files next to Gen I/II are old copies, not compiled.
  - `wwwroot/localization/` - `UI_Localization.csv` (English + Polish), `TheCostOfDisease_Localization.csv` (story text) and `TheCostOfDisease_Gameplay_Localization.csv` (rules/setup text). `*-copy.csv` are old copies.
  - `wwwroot/images/setup/` (popup images, referenced by `Shared/Consts/PopUpIcon.cs`) and `wwwroot/images/gameplay/` (inline `<icon=...>`). S2_*/S3_* images for the other scenarios already exist. Images are **Git LFS** - a worktree without `git lfs pull` shows broken icons.
- `MyFathersWorkConsole/` - porting aid. `Processor/*.txt` are the decompiled Cradle story classes for all three scenarios (the only source for Fear of the Unknown and A Time of War). `TheStoryProcessor.Run()` converts a `.txt` into readable console C# under `Processor/Scenario/ScenarioPart_*.cs` (currently holds the Cost of Disease Gen III passages; they are fully ported now).
- `tools/` - `passage_dump.py` (read passages), `tagcheck.py` (verify localization tags), `test-save-helpers.js` (play-test from crafted saves).
- Original Unity project (outside the repo, owner's machine): `C:\Users\ValentijndePagter\Downloads\my-fathers-work-master-4-20260622T163118Z-3-001\my-fathers-work-master-4`. Useful parts: `Assets/Scripts/OldStory/TheCostofDisease*.cs` (Cradle output, Cost of Disease only), `Assets/Scripts/UI/View*.cs` (score entry, tie breaker, ranking, endings UI flow), `Assets/Resources/setupImages`.

## How a scenario is written (conventions - follow them exactly)

Every screen is a `private static void X(GlobalData globalData)` method in the scenario's partial class:

```csharp
private static void Changes(GlobalData globalData)
{
    globalData.SaveToUndo();                               // FIRST, before any state change
    globalData.ActiveWindow = new GameplayWindow(globalData);
    globalData.ActiveWindow.AddDefaultTitle();             // tag "Changes_Title"
    globalData.ActiveWindow.AddDefaultContent();           // tag "Changes_Content"
    globalData.ActiveWindow.AddClickHereToContinue(Changes_0);
}

private static void Changes_0(GlobalData globalData)       // setup popup
{
    globalData.SaveToUndo();
    globalData.ActivePopup = new GameplayPopup(globalData, PopUpTitle.SpecialSetup, PopUpIcon.S1_LetterBACK, PopUpButton.Confirm, Diseases1); // tag "Changes_0_Content"
}
```

- **Localization tags come from `[CallerMemberName]`**: `Method_Title`, `Method_SubTitle`, `Method_Content`, `Method_Content{n}` (`AddNextContent(n)` / `AddNextContentWithLinks(n, [...])`), popups `Method_Content`. `AddDefaultBaseTitle` uses the part before the first `_`. Hubs: `Hub_Title`, `Hub_{Section}_Title/_Content/_Content{n}/_ClickHere`. Calls inside lambdas still use the enclosing method name. Use `AddCustomContent(string)` for text built in code.
- **Undo** (`GlobalData.SaveToUndo`) finds the caller by name via reflection on `typeof(TheCostOfDisease)`: every method that calls it must be unique by name, `static`, and take exactly `(GlobalData)`. Call it before mutating state; methods that only route (no screen) must not call it. Undo restores the JSON snapshot, rebuilds the hub (`GetHubMethod`) and re-invokes the method.
- **Hubs** (`GameplayHub`) are the per-generation round screens. One hub method per branch (e.g. `GloomyGothic`), sections switched by `ReplaceShouldShow(() => globalData.Years ...)` plus state flags. Set `HubId` and register the hub in `GlobalData.GetHubMethod()` (used by load and undo). End the round with a section calling `globalData.ShowEndOfRoundPopUp(nextEvent)`; the last round uses `AddEndOfGenerationSection`. Rebuild the hub (call the hub method) at the end of every event chain instead of `_ => { }` when the hub content depends on state.
- **`ShowEndOfRoundPopUp` advances `Years` (and `Generation` after Late) *before* calling the next event** - events after the Early round run with `Years == Middle`. At the generation change `ActiveHub` becomes null, so a callback of `_ => { }` then renders a **blank page** (this was the old "blank after Gen II" bug).
- **Randomness** must use the pre-rolled `TheCostOfDiseaseVars.RandomArray` (`RandomElement`, `RandomBool`, `RandomInRange`) with a fixed index per decision so undo/load replay identically. Never read `RandomArray` directly. Indices in use for Cost of Disease: 0-58 (Gen I/II) and 59-203 (`_RND_*` constants in the Gen III files). For events that can repeat, use `base + min(counter, 9)`. Shuffles are drawn one position at a time from the remaining items (`HunterAt`, `HuntRewardAt`).
- **Mirror mode** ("Play along with the official app" on the scenario language screen, `GlobalData.MirrorMode`, `Shared/GlobalData.Mirror.cs`, `Shared/MirrorPopup.razor`): `RandomArray` starts at -1 (not drawn). The first draw of an unanswered slot throws `MirrorQuestionException`; `GlobalData.RunScenarioAction` restores the state, replays the action in a sandbox once per value, shows the players the text that differs, and reruns the action with their answer. Rules for scenario code:
  - Every entry into scenario code (links, popup buttons, undo, load, start) goes through `RunScenarioAction`.
  - **Draw a random value on the screen where it becomes visible**, not earlier. Otherwise mirror mode asks before the official app shows it ("not visible on the current screen" in the console, shown as a guess-or-roll question). Examples: `Gen2Building`, `HuntVp`, `DrawFeverReward`, `MasterworkCost`, `DrawAffiliations` (which faction is evil is drawn on first use), the Symposium target.
  - A random next screen is chosen inside the callback: `data => vars.RandomElement([A, B], i).Invoke(data)`, not `AddClickHereToContinue(RandomBool(i) ? A : B)`.
  - Hub sections are built even while hidden - don't draw in a hidden section's text (guard with the section's condition, see `SuspicionBuilding`).
  - Answers live in `RandomArray`, so undo past the screen that asked forgets them and asks again (that is how a wrong answer is corrected).
- **State** lives in the scenario Vars class (serialized). Use arrays, not `List<T>` (Newtonsoft `ObjectCreationHandling.Reuse` appends to lists on load/undo). Add every field to `Reset()`. Per-player dictionaries are keyed by player name and filled in `Reset()`. `GlobalData.TmpValues` is cleared whenever a hub is displayed - only use it inside one event chain.
- **Placeholders** in CSV text (`Shared/Extensions.cs`):
  - `{{0=...|Oliver|James|...}}` + `FormatWithReplacement(0, value)` (the text after `=` is only a preview),
  - `{{1=|option0|option1|...}}` + `FormatWithIndex(1, index)` (enums starting at `None = 0` use `{{1=| |First|Second}}`),
  - `{{2=?|if true|if false}}` + `FormatWithCondition(2, () => ...)`.
  - The regex is `\{\{(id=[^}]+)\}\}`: option text cannot contain `|` or `}`. Nested placeholders work only if the inner one is formatted first.
  - Player-count words: `{{0=|two|three|four}}` with `LocalizedPlayerNumberIndex()`.
  - Links: `<link=0>text</link>` with `AddNextContentWithLinks` (link texts in one string must be unique).
- **CSV rules**: `Tag;Text` per line, `;` is the separator, so source semicolons become ` - `. Lines starting with `//` are comments. Duplicate tags throw at load. Trailing spaces are significant when a link follows inline (`... they may ` + `[click here]`) - some editors strip them. HTML is allowed (`<b>`, `<i>`, `<br />`, `<icon=Name>`, `<sprite=Name>`). Story text goes to `*_Localization.csv`, rules/setup text to `*_Gameplay_Localization.csv`. Both are searched.
- UI strings shared by all scenarios are in `UI_Localization.csv` + `Shared/Consts/GlobalTags.cs` and need an English **and** Polish value.

## Porting workflow (what worked for Cost of Disease Gen III)

1. Produce readable passages: in `TheStoryProcessor.cs` point `path` at the scenario `.txt` (it is hardcoded to `TheCostofDisease.txt`) and run it, or read `.txt` directly. The console output loses link labels (`OPT("Name", MethodName)`); `tools/passage_dump.py` restores them from the `.txt` (set `MFW_SOURCE=FearOfTheUnknown.txt`).
2. Build the passage graph from the scenario start / generation intros (BFS over `Method*` references) to know what belongs to each generation.
3. Map Twine concepts: a passage with text + `<hook>` "Click to continue" + `setupStyleEvnt` block -> window + setup popup. `$PrintEndOfTheRoundText(tag, next)` -> hub end-of-round section. `_SetupImage` -> `PopUpIcon`. `either(...)`/`GetRandom` -> `RandomArray` index. `$nameA..E` / `allyA..E` ifs -> loops over `GetActivePlayers()` (the web app supports 2-4 players, drop 5-player branches). `(display:)` / abort chains -> direct method calls.
4. Write code, then the CSV rows, then run `python tools/tagcheck.py` (0 missing) and play-test.
5. Keep source bugs visible: several Twine passages have dead or always-true conditions. Decide deliberately and note it (see Cost of Disease notes below).

## Adding Fear of the Unknown / A Time of War - what must be generalized

The engine is currently hardwired to Cost of Disease in these places:

- `GlobalData.SaveToUndo` - `switch (ScenarioId)` with `typeof(TheCostOfDisease)`; add a case per scenario class.
- `GlobalData.GetHubMethod` - only Cost of Disease hub ids.
- `GlobalData.LoadScenarioLanguagesOptionsAsync` - CSV paths per scenario (unknown scenarios navigate home - this is why they currently bounce to the menu).
- `Pages/PlayersScenarioLanguage.razor` - `switch` calling `TheCostOfDisease.Init`; also calls `TheCostOfDiseaseVars.Reset`.
- `GlobalData.TheCostOfDiseaseVars` - add a Vars class per scenario (reset + save), or introduce a common base.
- `08_TheCostOfDisease_Scoring.cs` (score entry, tie breakers, rankings, endings) - the score/tie/ranking part is scenario-independent and should move to shared code; only the endings text is per scenario. Its state (`Scores`, `TiedPlayers`, ...) lives in `TheCostOfDiseaseVars`.
- `tools/tagcheck.py` - paths point at the Cost of Disease folder/CSVs.
- `Pages/Home.razor` `Test()` shortcut sets `ScenarioId.CostOfDisease`.
- `Shared/Enums/Generation.cs` has 3 values; after the last round `Generation` becomes 3 (harmless, the round tracker shows nothing as current).

Source size for reference: `FearOfTheUnknown.txt` 378 passages, `ATimeOfWar.txt` 297 passages (Cost of Disease: 361). Both are English. Fear of the Unknown has a battle mini-game (`BattleStart`) and A Time of War has time-travel/paradox mechanics - look at the Unity `View*` scripts (`ViewBiddingSystem`, etc.) for UI the Twine text relies on.

## Cost of Disease - known deviations / open questions

- Gen III building tiles: Gloomy Gothic uses `BuildingsExposeValue <= 1`, Prosperity `> 0`. Looks inconsistent, but `<= 1` appears identically in every story version in the Unity project (earlier English edit, latest English, Spanish), so it was kept as the developer shipped it. The owner decided to keep it, but suspects it may be flawed.
- "Still Wary" popup lists Wolves allies only (source always appended the first player due to an always-true condition).
- Ties follow the rulebook (End of the Game): most points, then completed Masterwork, then most Estate Upgrades, else the family shares the win. Tie-breakers pick the winner without changing scores (the original app added +1 / the entered number to the score).
- "A Return to Evil" keeps the end-of-generation prompt (source jumped straight to scoring).
- Gen II Hospital: when everybody visited the Hospital the source skips "A Steady Focus"; the web app shows it.
- Not ported: endings/achievements gallery, sharing, voice audio, 5-player games.

## Build, run, test

- `dotnet build MyFathersWorkWebApp/MyFathersWorkWebApp.sln`
- `dotnet run --project MyFathersWorkWebApp/MyFathersWorkWebApp/MyFathersWorkWebApp.csproj` (http://localhost:5166; use `--no-launch-profile --urls http://localhost:5199` if 5166 is taken).
- There are no automated tests. Play-test with `tools/test-save-helpers.js`: craft a save at a late-round hub, reload `/Gameplay`, and drive the flow with `run([...])`. Check the browser console for exceptions and run `python tools/tagcheck.py`. To test mirror mode, pass `mirror: true` to `mkSave`; random click-throughs that report `Mirror:` console lines find draws made too early.
