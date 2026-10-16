# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## A Time of War port (October 2026)

Spec: [`docs/ATOW_PORT_MAP.md`](docs/ATOW_PORT_MAP.md) (one task per section 2.x). Status: **scaffold + Gen I "Taking
Sides" + Gen II "Martial" + Gen II "Time Travel" + Gen III "Heavy is the Crown" + Gen III "Peace Through War" done**
(map 1.3/1.4, 2.1-2.5), branch `claude/port-a-time-of-war-132617`.

- Engine: `Shared/Scenarios/ATimeOfWar/` (`ATimeOfWarVars` with every section-3 field, all `_RND_*` constants of 4.1 in
  `00_ATimeOfWar_Setup.cs`), `ATimeOfWarHubId`, `WarFaction`, `MartialBuilding`, `PopUpIcon` S3 constants, all `GlobalData`
  cases, `ATimeOfWar_Localization.csv` / `ATimeOfWar_Gameplay_Localization.csv` (+ shared scoring tags),
  `mkSaveATOW` in `tools/test-save-helpers.js`. Menu description "WORK IN PROGRESS".
- Gen I: setup, intro, Taking Sides hub (3 rounds), Town Hall rumors, Village Meeting, Barracks vote, end of Gen I
  (`TSBarracksPenalty`, `SeedGUNS`, `Warwarn`, both end-of-generation popups, `TowardsWar` / `TowardsDev`).
  Deviations: map 2.1 "Ported" notes.
- Browser play test: 3-player game from the menu through all Gen I rounds (all six rumors, meeting, both vote
  outcomes, both `$war` outcomes, town name "Paradox"), 2- and 4-player runs, undo, reload on the hub, mirror mode.

- Gen II Martial (files 02, 03): intros, `Martial` hub (3 rounds), Special Setup popup before the Early/Middle
  events, sabotage (village / cousin / Outpost), messengers, duels (science / pistols, denounce, decline, tie, death),
  quartering, bribe, afternoon tea (vote and 2-player bid), Barracks penalties, `ResistSides` -> `ResistEnd` / `2` / `3`
  -> End of Generation popup -> Gen III stubs `MonarchReignIntro` (file 05) / `PeaceIntro` (file 06). Deviations and
  source-bug decisions (B5-B7 fixed, B8/B9 as shipped, B10 death on 1 of 5): map 2.2 "Ported" notes.
- Browser play test of Martial: both factions from the end of Gen I through all rounds (2, 3 and 4 players), every
  sabotage/messenger/duel/tea/quartering/bribe branch, all three `ResistEnd*` into both Gen III stubs, undo on many
  screens, reload on the hub in every round, two mirror-mode runs (no `Mirror: ... not visible` lines, no exceptions).

- Gen II Time Travel (file 04): intro, Time Machine question/update card, Paradox tokens, `TimeTravel` hub (3 rounds:
  Paradox explanation, mending reward, Time Machine repairs, Military Demands), military token (followed / not), Barracks
  vote reversed, showcase resolution, Blame game (2-player rules, secret interrogation per player, $2 Flag offer,
  silent / blame), `BlameOut1/2/3`, Chaos Theory, Barracks penalty, one End of Generation popup -> `TimeEnd1` (dox) /
  `TimeEnd2` (repaired / zero / one / two) -> Gen III stubs `WarningIntro` (file 07) / `Paradoxical` (file 08).
  Deviations: map 2.3 "Ported" notes.
- Browser play test of Time Travel: from the end of Gen I into Time Travel and through all rounds with 2, 3 and 4
  players (both military token answers, repairs, mending, every Blame outcome incl. the 2-player rules, flags bought
  singular/plural, both Barracks states), `TimeEnd1` both links, `TimeEnd2` repaired and zero/one/two into the Gen III
  stubs, undo on many screens, reload on the hub in every round, one 4-player mirror-mode run (no `Mirror: ... not
  visible` lines, no exceptions).

- Gen III "Heavy is the Crown" (file 05, map 2.4): intro, Suspicious Barn / bribe checks, Crown token, `MonarchReign` hub
  (3 rounds), royal decrees (all 7, both choices, regal title input), Crown penalties, Wooden Figures, benevolence /
  iron fist vote (2-player bid), Commemorative Statue (trigger, bid, owner / other visits), Church, Weapon Cache, Royal
  Advisor bonus (sets `Ending` 5/6), Benevolence / Iron Fist bonus -> `Scoring`. Source bugs B14, B16, B18 fixed, B15 as
  proposed, B17 kept. Deviations: map 2.4 "Ported" notes.
- Gen III "Peace Through War" (file 06, map 2.5): intro, barn check, World Conquest / weapons setup, tax, weapon limit,
  `Peace` hub (3 rounds), war explanation, famine vote / 2-player bid with the new meat name, Wooden Figures, battles,
  gas vote, Final Battle (sets `Ending` 7/8) -> `Scoring`. Deviations: map 2.5 "Ported" notes.
- `Scoring` (file 09) is a `// TODO ATOW Scoring` stub (`NotYetPorted`) shared by both branches; the state the endings need
  is listed at the end of map 2.5.
- Browser play test of Reign and Peace: both entered from a Martial Late hub through the End of Generation popup (3p
  Reign, 4p Peace, 2p Peace, 2p and 3p Reign in mirror mode, 3p Peace in mirror mode), all three rounds to the scoring
  stub with every decree and both choices, title input, crown penalties, Wooden Figures 1 / >1, vote and 2-player bid
  variants of the CoW and famine events, statue triggered (fixed and random) and not, statue visits by owner / other,
  Church, Weapon Cache, benevolence tie and unique winner, meat / empty promises, gas / no gas, final battle both sides.
  Undo on decree, input, vote, end-of-Generation and scoring screens; reload / load on the hub in every round. No
  `Mirror: ... not visible` lines and no console errors.

Next steps:
- Gen III (2.6-2.7): replace the stubs `WarningIntro` (file 07) and `Paradoxical` (file 08) and add the rest, then
  Scoring + endings (2.8/2.9, replace the `Scoring` and `FinalEnding` stubs in file 09; remove `NotYetPorted`).
- Owner questions for Gen III Reign/Peace: B14 (first-visit text only once), B16 (2-player CoW left/right text fixed),
  B17 (Church costs "X/X/X"), B18 (decree tie rewards nobody) - defaults applied, see map section 7 and 2.4.
- Owner questions in map section 7 (B1 GunsAnnouncement text, B4 TowardsWar wording, B9 figure-token holder, B10 duel
  death odds, ...).
- `AudioService.ScenarioTheme` has no ATOW music yet (plays none).

## Fear of the Unknown - table play test (October 2026)

Port and browser play test are done and merged to `main` of the fork (`vdepagter/...`); see the archive. A PR to the
original repository (`Deusald/MyFathersWork-FanMadeCompanionApp`) can be opened from `main` or a branch at any time.

Next steps:
- Play a real game at the table (only crafted-save runs so far).
- Random sub-events not yet seen in the browser (they share code paths with tested ones): Liberal salts / Arise My
  Love / bribe / policy vote, Isolation smuggling trial outcomes, Foreigners Wall "Acceptance" + caravan visits,
  Creature cure accepted path, Gen I fates other than Creature, 2- and 4-player games.

## Music, sound effects, voice-over and screen transitions (October 2026)

Implemented and deployed; mapping from the Unity app in `docs/AUDIO_TRANSITIONS_MAP.md`.
`wwwroot/audio/*.mp3` (12 clips, ~9.3 MB, LFS), `wwwroot/js/audio.js` + `Shared/AudioService.cs`, `wwwroot/js/transitions.js`,
toggles in `Shared/AudioToggles.razor`. Sounds and transitions are derived in `Pages/Gameplay.razor` (`DetectScreenChange`);
scenario code only sets `GameplayWindow.SetCue` / `MarkFinalEnding` - never call audio from scenario code (mirror sandbox, undo, load).

Next steps:
- Test on a real iPhone/iPad (Safari autoplay unlock, MP3 playback) and at the table (volume of the effects vs music).
- Voice-over: 17 narrated passages (map section 8), Female/Male/Off (default Male as in Unity), "Narrated" pill on those windows.
  Uncertain: the female `GloomyWolvesIntro` clip lives in Unity's male folder (judged female by pitch). Recordings follow the
  English source text, so other text versions differ from the narration.
- Not ported (deliberate): Unity bidding/reveal screen, clips the shipped app never played (map 2.3), A Time of War VO.
- Version: `0.66.6.<revision>`, revision = first-parent commits on main since 3d78856 minus 1 (deploy workflow).
