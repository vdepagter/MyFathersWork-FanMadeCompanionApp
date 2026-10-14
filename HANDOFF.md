# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## A Time of War port (October 2026)

Spec: [`docs/ATOW_PORT_MAP.md`](docs/ATOW_PORT_MAP.md) (one task per section 2.x). Status: **scaffold + Gen I "Taking
Sides" done** (map 1.3/1.4 and 2.1), branch `claude/port-a-time-of-war-132617`.

- Engine: `Shared/Scenarios/ATimeOfWar/` (`ATimeOfWarVars` with every section-3 field, all `_RND_*` constants of 4.1 in
  `00_ATimeOfWar_Setup.cs`), `ATimeOfWarHubId`, `WarFaction`, `MartialBuilding`, `PopUpIcon` S3 constants, all `GlobalData`
  cases, `ATimeOfWar_Localization.csv` / `ATimeOfWar_Gameplay_Localization.csv` (+ shared scoring tags),
  `mkSaveATOW` in `tools/test-save-helpers.js`. Menu description "WORK IN PROGRESS".
- Gen I: setup, intro, Taking Sides hub (3 rounds), Town Hall rumors, Village Meeting, Barracks vote, end of Gen I
  (`TSBarracksPenalty`, `SeedGUNS`, `Warwarn`, both end-of-generation popups, `TowardsWar` / `TowardsDev`).
  Deviations: map 2.1 "Ported" notes.
- Browser play test: 3-player game from the menu through all Gen I rounds (all six rumors, meeting, both vote
  outcomes, both `$war` outcomes, town name "Paradox"), 2- and 4-player runs, undo, reload on the hub, mirror mode.

Next steps:
- Gen II Martial (map 2.2) and Time Travel (2.3): replace the `// TODO ATOW Gen II` stubs `IntroMartialMonarchists`,
  `IntroMartialSeparatists` (file 02) and `TimeTravelIntro` (file 04); add their hubs to `GlobalData.GetHubMethod`.
- Gen III (2.4-2.7), then Scoring + endings (2.8, replaces the `FinalEnding` stub in file 09; remove `NotYetPorted`).
- Owner questions in map section 7 (B1 GunsAnnouncement text, B4 TowardsWar wording, ...).
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
