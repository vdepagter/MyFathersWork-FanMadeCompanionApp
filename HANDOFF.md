# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## A Time of War port (October 2026)

Spec: [`docs/ATOW_PORT_MAP.md`](docs/ATOW_PORT_MAP.md) (one task per section 2.x). Status: **fully ported** - scaffold,
Gen I "Taking Sides", Gen II "Martial" and "Time Travel", Gen III "Heavy is the Crown", "Peace Through War", "Future
Consequences" and "You Cant Stop Progress", Scoring and all 8 endings (map 2.1-2.9), branch
`claude/port-a-time-of-war-132617`. No `// TODO ATOW` / `NotYetPorted` left; `GetHubMethod` maps all seven ATOW hubs. Menu
description still "WORK IN PROGRESS" (until a full play test). Deviations: the "Ported" notes in map 2.1-2.8 and the ATOW
section in `CLAUDE.md`.

- Engine: `Shared/Scenarios/ATimeOfWar/` (files 00-09, `ATimeOfWarVars`), `ATimeOfWarHubId`, `WarFaction`,
  `MartialBuilding`, `PopUpIcon` S3 constants, the `GlobalData` cases, both ATOW CSVs, `mkSaveATOW` in
  `tools/test-save-helpers.js`.
- Browser-tested per branch (crafted saves at the end of the previous Generation, then every round transition): Gen I
  (2/3/4 players), Martial (both factions, every event chain), Time Travel (every Blame outcome), Reign and Peace (all
  decrees, votes / bids, statue, battles), Warning (all six world pages, time shifts to the stable timeline, old man's
  letter, Stasis), Paradox (both figure paths, the Time Machine vote with 0 / 1 / 2 releases, all eight random paradoxes,
  giant check), Scoring with score entry, tie-breakers (Masterwork, Estate Upgrades, family win) and rankings into all
  8 endings. 2, 3 and 4 players, undo on many screens incl. score entry, reload on the hub in every round, mirror-mode
  runs per branch without `Mirror: ... not visible` lines, no console errors. `tagcheck.py` (all three scenarios) 0
  missing, `sourcecheck.py ATimeOfWar ATimeOfWar` only lists justified items (unreachable passages, `;` / Harlowe `''`
  in labels, the Unity-only screens handled as popups).

Next steps:
- One full browser play-through from the menu to an ending (so far each branch was entered from crafted saves at the
  previous Generation's Late hub).
- Table play test; afterwards drop "WORK IN PROGRESS" from the menu description (English + Polish).
- Owner questions (defaults applied, map section 7): B1 GunsAnnouncement text, B4 TowardsWar wording, B9 figure-token
  holder may not exist, B10 duel death odds, B14 first-visit text, B16 2-player CoW text, B17 Church costs "X/X/X",
  B19 random paradox only after a release (fixed), B21 University "pay $1" in Warning (fixed); the `SeedResolution` popup
  typo "gains gains"; endings 1/5 print "The family IV" on a shared win (as the source).
- `AudioService.ScenarioTheme` has no ATOW music yet (plays none); ATOW voice-over not ported.

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
