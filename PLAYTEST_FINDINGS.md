# Playtest findings

Findings from real play-tests, what caused them and where the same mistake can appear again. Use the checklist at the
end when porting Fear of the Unknown and A Time of War.

## Playtest 1 - The Cost of Disease, 4 players, mirror mode (October 2026)

Path: Gen I Library -> Gen II Devastation -> Gen III Prosperity (Fraternity of Hunters, good).

### 1. Middle Years Suspicion shows "Investigate" three times without a building name - OPEN (bug)

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
- **Fix:** end the Devastation event chains with `Devastation` instead of `_ => { }` (at least `PreDevastation2`,
  `Diseases2A_0`, the creepy/concealed-building popups and the Building Signin confirmations), or let the end of a chain
  rebuild the active hub in one place (e.g. in `Gameplay.razor` when a popup/window closes onto a hub).

### 2. III-1 "Strong Alliance" contains "END OF GENERATION SETUP ..." inline - OPEN (bug)

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
- **Fix:** show the text as an `EndOfGeneration` popup (like `FeverHeart2_1`) before the story window, and drop it from
  the window content.

### 3. First / Second Night of the Hunt - "South" choice has no text - OPEN (bug)

- **Cause:** `HuntersChoice_Content1` ends with `<link=3>South</link>`; the source label is
  "South, back to the town. Both players gain 2 {reward4}." North/East/West are complete. Because the South reward is
  not shown, `HuntRewardAt(hunt * 4 + 3)` is only drawn after the click (in `HuntersChoiceDirection`), so mirror mode
  asks a "not visible on the current screen" question for it.
- **Elsewhere:** `tools/sourcecheck.py` compares every source link label with the CSVs. For Cost of Disease only the two
  South links (both hunts) are truncated. Other hits are intentional: title-screen player counts and debug start menu
  (UI pages), and `S5Fate2`, whose "Succeeded in / Failed to Build the Hospital" links became "Click here to continue"
  (harmless).
- **Fix:** add the South text with `{{3=|...}}` and format index 3 with `HuntRewardAt(globalData, hunt * 4 + 3)`.

### 4. Mirror mode question lacks the screen heading - OPEN (improvement)

- **Cause:** `MirrorPopup` shows at most 8 words before and after the changing part (`_MIRROR_CONTEXT_WORDS`). The
  window/popup title (e.g. "A Town Encounter") is part of the compared text but usually falls outside that window, so
  the players see a fragment without knowing which page of the official app to look at.
- **Elsewhere:** every mirror question (`MirrorScreenText`); worst on long story windows and hubs.
- **Fix:** keep the title of the screen that is being asked about (window title, popup title, or the hub section title
  that contains the change) and show it as a heading in `MirrorPopup`. When the title itself is what differs, it is
  already in the option text.

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

## Checklist for porting / reviewing a scenario

1. **Hub text vs. state** - any hub text built with a formatter (`AddSpecialClickHere(..., content => ...)`,
   `AddDefaultContent(..., content => ...)`, `SetDefaultTitle(content => ...)`) must be rebuilt when that state changes.
   End every event chain that returns to a state-dependent hub with the hub method, not `_ => { }`. Play-test the round
   transitions themselves, not only crafted saves (load and undo rebuild the hub and hide this).
2. **Unity-only screens** - run `python tools/sourcecheck.py <Scenario> <CsvPrefix>`. Every `S_OnEndOfGeneration`,
   `S_OnEndOfGeneration2` and `S_OnSetSpecialSetup` text becomes its own popup (`PopUpTitle.EndOfGeneration` /
   `SpecialSetup`), never part of the story window.
3. **Link labels** - the same tool lists source link labels missing from the CSVs. Labels concatenated with a variable
   (`"South, ... gain 2 " + $reward4`) are the usual victims. Every value shown in a label must be drawn on that screen.
4. **Mirror mode** - click through with `mirror: true`; any `Mirror: ... not visible on the current screen` line is a draw
   made earlier or later than the official app shows it (often a missing piece of text, as in finding 3).
5. **Branch differences with the official app** - before treating a branch as a bug, compare the deciding variables in
   the save with the source condition (`tools/passage_dump.py`) and write down the player choices that feed them.
