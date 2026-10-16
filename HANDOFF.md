# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## A Time of War port (October 2026)

Spec: [`docs/ATOW_PORT_MAP.md`](docs/ATOW_PORT_MAP.md) (one task per section 2.x). Status: **fully ported** - scaffold,
Gen I "Taking Sides", Gen II "Martial" and "Time Travel", Gen III "Heavy is the Crown", "Peace Through War", "Future
Consequences" and "You Cant Stop Progress", Scoring and all 8 endings (map 2.1-2.9), branch
`claude/port-a-time-of-war-132617`. No `// TODO ATOW` / `NotYetPorted` left; `GetHubMethod` maps all seven ATOW hubs. The status prefix was
removed from all menu descriptions (October 2026). Deviations: the "Ported" notes in map 2.1-2.8 and the ATOW
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
- Independent browser play test from the main menu (no crafted saves, every screen clicked, hub explanation links
  visited): 3 players Martial (Separatists intro) -> Peace, ending 8; 2 players Martial (Monarchists intro) -> Reign,
  ending 6; 4 players Time Travel (town "Paradox") -> Paradox, ending 1; 2 players **mirror mode** Time Travel ->
  Warning, ending 4 (plus a mirror Martial -> Reign run from a Gen I save). Score entry, tie-breakers, rankings and
  endings (family and single winner) in each; undo across round and Generation changes, reload on hubs. No console
  errors; the only `Mirror: ... not visible` line is the documented `Quarter` slot of `RumorD3Revealed`. Fixed: undo
  from the first screen of a round reached straight from the end-of-round popup jumped back a round (engine,
  `GlobalData.SaveGame`, PLAYTEST_FINDINGS #7 - also affected Fear of the Unknown Witch Hunt); text "Monarchists's"
  -> "Monarchists'" (sabotage intros), "a Irresistible Aphrodisiac" -> "an". `tagcheck.py` now also checks computed
  content indices (`DYNAMIC_VALUES`, ternaries) and the shared `Gen3_*` hub sections.

Next steps:
- Table play test.
- Owner questions (defaults applied, map section 7): B1 GunsAnnouncement text, B4 TowardsWar wording, B9 figure-token
  holder may not exist, B10 duel death odds, B14 first-visit text, B16 2-player CoW text, B17 Church costs "X/X/X",
  B19 random paradox only after a release (fixed), B21 University "pay $1" in Warning (fixed); the `SeedResolution` popup
  typo "gains gains"; endings 1/5 print "The family IV" on a shared win (as the source), ending 8 "the The family name"
  / "Dr. The family" and ending 6 "I, The family" likewise; the Drifters decree popup starts with a dangling "Then,"
  when the Crown does not move (source `{$crownL} Then, ...`).
- Not play-tested from the menu: the Martial duel / bribe-success / tea-yay sub-chains, Peace gas release, Warning
  stable timeline + Stasis, Paradox figure path with two Wooden Figures (covered by the per-branch crafted-save runs).
- Shared UI text (not ATOW): `PlayersVillageConfirm` "research.In fact" misses a space.
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
- Voice-over: 17 narrated passages in Cost of Disease / Fear of the Unknown (map section 8), Female/Male/Off (default Male as in Unity), "Narrated" pill on those windows.
  Uncertain: the female `GloomyWolvesIntro` clip lives in Unity's male folder (judged female by pitch). Recordings follow the
  English source text, so other text versions differ from the narration.
- A Time of War VO: 11 narrated passages wired (map 8.3, `ParadoxIntro` reuses the `WarningIntro` clip as in Unity).
- Not ported (deliberate): Unity bidding/reveal screen, clips the shipped app never played (map 2.3).
- Version: `0.66.6.<revision>`, revision = first-parent commits on main since 3d78856 minus 1 (deploy workflow).
