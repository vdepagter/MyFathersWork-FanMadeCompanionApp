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
- AI narration: see the section below.
- Not ported (deliberate): Unity bidding/reveal screen, clips the shipped app never played (map 2.3).
- Version: `0.66.6.<revision>`, revision = first-parent commits on main since 3d78856 minus 1 (deploy workflow).

## AI narration - ElevenLabs (October 2026)

Rules and commands: CLAUDE.md "AI narration", `docs/AUDIO_TRANSITIONS_MAP.md` 8.4, docstrings of `tools/narration/chunks.py`
and `elevenlabs.py`.

State (27 Oct 2026, branch `claude/oliver-v4-narration`, merged and pushed; deployed and live, first deploy with the
LFS cache step succeeded - cache ~158 MB):
- Voice setting "Oliver (AI, whole story)" next to Female / Male / Off. Male and Female play only the original recordings
  (the 7 v3 intro clips in `wwwroot/audio/vo/ai/` are gone). Oliver plays chunk clips per window, falls back to the
  original male recording where a window has none, and never autoplays private screens (Play only).
- Model `eleven_v4`, stability 0.8, similarity 0.8 (user's choice). Verified: v4 accepts but ignores style, speed and
  speaker boost (model metadata `can_use_style` / `can_use_speaker_boost` false; speed 0.7 gave the same clip length);
  it does not read audio tags aloud (speech-to-text check); no clipped endings.
- Fear of the Unknown: **fully rendered** - 509 clips, 124k characters sent, ~58k billed (v4 bills ~0.45 per character
  sent), 94 MB at 96 kbps mono (`wwwroot/audio/vo/oliver/`, LFS). Browser walk-throughs (Witch Hunt, Foreigners, Creature,
  all four Gen III branches, endings) found every window narrated except `skip.txt`; private screens offer Play only.
- Stand-ins for values nobody can render (user's choice, 27 Oct): player names -> "one of the cousins" / "a cousin" /
  "a cousin's" / "two of the cousins", village -> "the village" ("the town of <village>" -> "the town"), newspaper ->
  "the local newspaper", warrior / program names -> "the warrior" / "the program" (`chunks.STAND_INS`). Random dates
  are skipped. Headers: "A letter." etc. "$<amount>" -> "<amount> dollars" (amount is a value clip, 1-20).
- Quota after Fear of the Unknown: ~57.8k of 126.6k left (resets 16 Nov 2026).
- ElevenLabs Creator: 126,634 characters per month, resets 16 Nov 2026. API key in `ELEVENLABS_API_KEY`; the key has an
  IP allowlist (a 403 "ip_not_allowed" means the PC's public IP changed). Oliver: `python tools/narration/elevenlabs.py add
  oliver 503ef2e72fb506b3879737bc64638b2d48597374bf1247925feea5d30965f3bd jfIS2w2yJi0grJZPyEsk "Oliver Silk - Deep Gravel
  Narrative"` in a new checkout (git-ignored `voices.local.json`).
- Masters (192 kbps) are in the git-ignored `tools/narration/out/master/oliver/` of the main checkout - keep them.
  Older comparison renders: `.claude/worktrees/elevenlabs-voice-narration-3a3e64/tools/narration/out/`.

Play-test fixes (28 Oct 2026, branch `claude/playtest-feedback-fotu`): `chunks.py` drops "of" in "the village of <village>",
says "My dear cousin" for "My dear <name>", drops the generation numeral after a skipped name ("<name> II"), and lists every
`<Method>_Content<n>` when the index is computed (`1 + vars.ProgramType`); `CureDenied` is private; `Narration.cs` finds
a static chunk that also occurs inside the previous value (`PatientEntry`: " " between two either/or sentences).
Narration logic check (29 Oct): choices and rules lines are no longer narrated (`auto_cut`, `cuts.txt`), headers fixed
("Correspondence from <name>" -> "Correspondence.", "Anonymous Editorial", "Excerpt", "Case File"), "the 1800's" ->
"the eighteen hundreds", "<b><name></b> must suffer" -> "One of the cousins must suffer", "due to the <program>
Program" -> "due to the Program". Titles / headers per scenario (CLAUDE.md). Not changed: "Vlad III" is sent as written
(check by listening that v4 says "the Third"). **To render**: ~10k characters sent (~4.5k billed), `chunks.py render
FearOfTheUnknown` on the laptop, then commit the clips and the manifest.
For the next scenarios: review the `_Title` texts with placeholders (Cost of Disease has dates and names in titles,
e.g. "Upon a Secret Meeting - Letter - <date>") before rendering; Cost of Disease is now ~157k characters sent
(titles on, headers off), more than one month of quota.

Next steps:
1. The Cost of Disease (~126k characters sent, ~57k billed) and A Time of War (~106k, ~48k billed); private sections of
   each last. Cost of Disease fits roughly in the remaining quota; A Time of War after the reset. Both need a `values.txt`,
   `private.txt` and `skip.txt` like Fear of the Unknown (review `chunks.py plan <Scenario> --list` and the chunk starts).
2. Listen-through at the table: joins around skipped names, the stand-ins, the private Play button. Size: the three
   scenarios will be ~300 MB of clips; 64 kbps mono would cut that by a third if Pages/LFS limits bite.
3. Play-test the other scenarios with Oliver once rendered: console lines "Oliver narration: no clips for <Tag>" show
   windows whose text code changes beyond placeholders (not narrated); fix the split or accept.
4. User play-test of Fear of the Unknown with Oliver first; the other scenarios start in a new session.
