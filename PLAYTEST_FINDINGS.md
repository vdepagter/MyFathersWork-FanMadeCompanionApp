# Playtest findings

Findings from real play-tests, what caused them and where the same mistake can appear again. Use the checklist at the
end when porting Fear of the Unknown and A Time of War.

## Playtest 1 - The Cost of Disease, 4 players, mirror mode (October 2026)

Path: Gen I Library -> Gen II Devastation -> Gen III Prosperity (Fraternity of Hunters, good).

### 1. Middle Years Suspicion shows "Investigate" three times without a building name - FIXED

- **Cause:** hub text is formatted when the hub is *built*, not when it is shown. `Devastation()` is built once in the
  Early Years (from `PreDevastation`). `SuspicionBuilding` returns `None` outside Middle/Late (the mirror-mode guard), so
  the three `Devastation_Suspicion_ClickHere` links are formatted as "Investigate " + nothing. The event chain after the
  Early round (`Changes` ... `PreDevastation2`) ends with `_ => { }`, so the hub is never rebuilt with `Years == Middle`.
  The same applies to the Investigate/Expose/Conceal chains (`ExposeConfirmation`, `ConcealConfirmation`, ...).
- **Hidden by testing:** undo and load rebuild the hub (`GetHubMethod`), so a crafted save loaded at Middle Years looks
  correct. Only playing through the round transition shows it.
- **Elsewhere:** checked all Cost of Disease hubs. Only Devastation formats state-dependent text that changes after the
  hub is built. Hospital formats `Sci3` (chosen before the hub is built), GloomyGothic/Prosperity/University rebuild the
  hub at the end of every chain (their only `_ => { }` is the unused `default` of the end-of-round switch). Gen I and
  Hospital also have many `_ => { }` endings but their hub text is static.
- **Fix (both scenarios):** `GlobalData.RunScenarioAction` now rebuilds the shown hub (`RefreshShownHub`, via
  `GetHubMethod`) whenever a scenario action ends with no window or popup open. An event chain ending in `_ => { }`
  therefore always returns to an up-to-date hub. Hub methods must stay free of side effects (they are rebuilt after
  every action) - all current ones are: the only assignment is Fear of the Unknown's idempotent `vars.Path`.

### 2. III-1 "Strong Alliance" contains "END OF GENERATION SETUP ..." inline - FIXED

- **Cause:** in the official app the passage calls `ViewEndOfGeneration.S_OnEndOfGeneration(text, n)`, a Unity screen
  that shows the end-of-generation instructions on their own page. The port put that text into the story window
  (`DevastationFate1_Content`).
- **Elsewhere (Cost of Disease):** 7 passages call it.
  - `S5Fate1` / `S5Fate2` (end of Gen I): correct - shown as its own `PopUpTitle.EndOfGeneration` popup (`FeverHeart2_1`).
  - `DevastationFate1` (end of Gen II Devastation): **wrong**, inline.
  - `S5HospA1Yes/No`, `S5HospA2`, `S5HospA3` (end of Gen II Hospital): **wrong**, the text is inline at the top of
    `NoHospitalCons` ("A Steady Focus", `NoHospitalCons_Content`).
- **Other scenarios:** Fear of the Unknown 6 passages (`S4Witch1/2`, `S4Foreign1/3` - these also call
  `S_OnEndOfGeneration2` - `S4Jail1/2`), A Time of War 4 passages (`TowardsWar`, `TimeEnd1/2`, `TowardsDev`) plus 6 passages
  with `S_OnSetSpecialSetup` (`Martial1/2`, `ResistEnd`, `ResistEnd2`, `ResistEnd3`), which is the same idea for setup text.
  `python tools/sourcecheck.py <Scenario>` lists them.
- **Fix:** `DevastationEndOfGeneration` popup before "Strong Alliance"; `HospitalEndOfGeneration` popup after "A Steady
  Focus" and before the Gen III intro (where the source shows it; the random `S5HospA*` intro is drawn when it closes).
  The text was removed from both windows.
- **Fear of the Unknown:** already correct - `Gen1End`, `Gen2ForeignersEnd` (both `S4Foreign` variants) and
  `Gen2CreatureEnd` are separate End of Generation popups.

### 3. First / Second Night of the Hunt - "South" choice has no text - FIXED

- **Cause:** `HuntersChoice_Content1` ends with `<link=3>South</link>`; the source label is
  "South, back to the town. Both players gain 2 {reward4}." North/East/West are complete. Because the South reward is
  not shown, `HuntRewardAt(hunt * 4 + 3)` is only drawn after the click (in `HuntersChoiceDirection`), so mirror mode
  asks a "not visible on the current screen" question for it.
- **Elsewhere:** `tools/sourcecheck.py` compares every source link label with the CSVs. For Cost of Disease only the two
  South links (both hunts) are truncated. Other hits are intentional: title-screen player counts and debug start menu
  (UI pages), and `S5Fate2`, whose "Succeeded in / Failed to Build the Hospital" links became "Click here to continue"
  (harmless).
- **Fix:** South text with `{{3=|...}}`, formatted with `HuntRewardAt(globalData, hunt * 4 + 3)`, so mirror mode now
  asks for the South reward on this screen.
- **Fear of the Unknown:** no truncated labels. The only unmatched source links are the title screen, the debug start
  menu and unreachable note passages (`4Notes`, `Extracodes`, `LowestJournalEvent` - nothing links to them).

### 4. Mirror mode question lacks the screen heading - FIXED

- **Cause:** `MirrorPopup` shows at most 8 words before and after the changing part (`_MIRROR_CONTEXT_WORDS`). The
  window/popup title (e.g. "A Town Encounter") is part of the compared text but usually falls outside that window, so
  the players see a fragment without knowing which page of the official app to look at.
- **Elsewhere:** every mirror question (`MirrorScreenText`); worst on long story windows and hubs.
- **Fix (both scenarios):** `MirrorScreenWords` tags every word with the title of the part of the screen it is in
  (window title, popup title, or hub section title / hub title). `MirrorQuestion.Heading` is the title where the options
  start to differ and `MirrorPopup` shows it above the question. It is empty when the title itself differs (the options
  are different screens and each starts with its own title); "not visible" questions use the current screen's title.

### 5. "Stunning Success!" - "Gain a Knowledge of your choice" - NOT A BUG

`HuntSuccess_0` reads "Each player participating in the Hunt: Lose 1 <Creepiness icon>, return this servant to your
Quarters, and Gain {2VP | 3VP | a Knowledge of your choice}." This matches the source (`HuntSuccess1` setup) word for
word. The head icon is the Creepiness token (`Creepy_Icon`).

### 6. Gen III: official app went to "Overrun By Demons" -> "A Return to Evil - Late Years" - NOT REPRODUCIBLE

- Save at the branch (Late Years, Prosperity hub, Hunters good): `HuntCount = 2`, `Overrun = False`, so the web app
  showed "A Righteous/Wondrous Fate".
- `HuntSuccessCheck` matches the shipped story (`TheCostofDiseaseLatest.cs`): 2 successful hunts = success, 1 = coin
  flip, 0 = failure. Each "Yes." on "Did at least one player vote to contribute to the Hunt?" (`HuntSuccess1`) adds 1;
  `huntcount` is only reset in `ProsperityHunterIntro`. The round check (`round == 13` -> back to the hub, otherwise
  `HuntSuccessCheck`) matches `HuntIndex`.
- So the official app had at most 1 successful hunt: one of the two hunts was answered "No." there (or answered
  differently than in the web app), and with 1 success the official app flips a coin. The Unity Back button restores the
  previous passage's variables and replays it, which can also change counts. The web app cannot tell which hunt
  differed because it only stores the total.
- **Next time:** note the Yes/No answer of both hunt votes in both apps. If they really were both "Yes." in the official
  app, this needs another look.

## Checks against Fear of the Unknown (October 2026)

Run after the port was merged; no changes to Fear of the Unknown were needed.

- #1 hub text: its event chains already end by calling the hub method (the only `_ => { }` per file is the unused
  `default` of the end-of-round switch); the shared hub refresh now covers it anyway. Browser run in mirror mode through
  Gen II Foreigners Early -> Middle -> Late: no errors, hubs current.
- #2 end-of-generation screens: all three are separate popups (see #2).
- #3 link labels: nothing truncated (see #3).
- #4 mirror heading: shared code; checked on Foreigners questions ("Blame it on the Foreigners", "Town Strength",
  "Death and Resolution").

## Browser play test - A Time of War, full games from the menu (October 2026)

Four games from the main menu, no crafted saves: 3 players Martial -> Peace (ending 8), 2 players Martial -> Reign
(ending 6), 4 players Time Travel -> Paradox (ending 1), 2 players mirror mode Time Travel -> Warning (ending 4), plus a
mirror-mode Martial -> Reign run from a Gen I save.

### 7. Undo from the first screen of a round skips back a whole round - FIXED (engine, all scenarios)

- **Cause:** a hub is an undo point only through `GlobalData.SaveGame` (called when a hub is shown after a window or
  popup), which skips the push when the top undo entry is already a hub entry, so a hub re-shown without an undoable
  screen in between is not stored twice. The end-of-round popup has no `SaveToUndo`, so when the next round's hub
  follows the popup directly, the new hub looked like a re-shown hub and was not stored. Undo from the first screen
  of that round (Scoring after the Late round, a hub link) then restored the previous round's hub.
- **Where:** every round transition whose next event is the hub itself: A Time of War Reign (Middle -> Late without
  the statue), Paradox (Middle -> Late without a release), Fear of the Unknown Witch Hunt (Early -> Middle). The Cost of
  Disease always shows an event first, so it was not affected.
- **Fix:** hub undo entries remember their round (`UndoData.Round`); `SaveGame` only skips the push when the top hub
  entry is of the same round. Undo then goes Scoring -> Late hub -> Middle hub. Checked in all three scenarios.

## Checklist for porting / reviewing a scenario

1. **Hub text vs. state** - hub text is formatted when the hub is built. `RunScenarioAction` rebuilds the shown hub
   after every action, so this is automatic as long as hub methods have no side effects (no state changes, no
   `SaveToUndo`, no windows/popups) and only draw random values for sections that are shown. Still play-test round
   transitions themselves, not only crafted saves.
2. **Unity-only screens** - run `python tools/sourcecheck.py <Scenario> <CsvPrefix>`. Every `S_OnEndOfGeneration`,
   `S_OnEndOfGeneration2` and `S_OnSetSpecialSetup` text becomes its own popup (`PopUpTitle.EndOfGeneration` /
   `SpecialSetup`), never part of the story window.
3. **Link labels** - the same tool lists source link labels missing from the CSVs. Labels concatenated with a variable
   (`"South, ... gain 2 " + $reward4`) are the usual victims. Every value shown in a label must be drawn on that screen.
4. **Mirror mode** - click through with `mirror: true`; any `Mirror: ... not visible on the current screen` line is a draw
   made earlier or later than the official app shows it (often a missing piece of text, as in finding 3).
5. **Branch differences with the official app** - before treating a branch as a bug, compare the deciding variables in
   the save with the source condition (`tools/passage_dump.py`) and write down the player choices that feed them.
6. **Undo across round transitions** - after each end-of-round popup, press undo on the first screen of the new round
   (and on Scoring): it must return to that round's hub, then to the previous round's hub (finding 7). Crafted saves
   start with a single undo entry, so check this in games that reach the round by playing.
7. **Dynamic tags** - `python tools/tagcheck.py --scenario=<Scenario>` checks computed content indices that are a ternary
   of literals or listed in its `DYNAMIC_VALUES`; add an entry there for every new computed index it prints as DYNAMIC.
