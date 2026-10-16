# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

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
- AI narration (map 8.4, `tools/narration/`): ElevenLabs "Oliver Silk" narrates the windows right before and after each
  scenario intro (4 clips). To be judged in the next playthrough. Remaining un-narrated story text ~250k characters
  (~5.5 h); about half has placeholders (names, random dates) that need generic wording. Custom designed voices
  (`voices.json` male/female) need tweaking: a bit sharp, slightly too slow. Local open-model route (Qwen3-TTS,
  Chatterbox) in `tools/narration/local_tts.py`, tested on the laptop GPU only.
- Not ported (deliberate): Unity bidding/reveal screen, clips the shipped app never played (map 2.3), A Time of War VO.
- Version: `0.66.6.<revision>`, revision = first-parent commits on main since 3d78856 minus 1 (deploy workflow).
