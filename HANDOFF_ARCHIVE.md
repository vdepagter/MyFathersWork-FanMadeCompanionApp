# Handoff archive

Finished work moved out of [`HANDOFF.md`](HANDOFF.md), newest first. Kept short: what was done and where it is documented.

## Endings Gallery (October 2026)

- Remembered players, win tally, endings trees; notes in `CLAUDE.md` "Endings Gallery". Design mockups:
  https://claude.ai/artifact/UHhtp9HGMdMVJmnYeSZo7L.
- Built clean (written without a .NET SDK). Fixed: the Endings tab crashed on the "Generation I/II/III" label
  (`FormatWithIndex` got depth + 1); a final ending with `Ending == 0` (crafted saves only) is no longer recorded.
- Browser-tested: empty gallery, Reveal / Show all / ticks (persist after reload, light the branch path); New Game hub
  records players and branch, "same table as last time" and name chips (taken names left out); crafted saves into a
  final ending for all three scenarios (single winner, family win, repeat ending = "Ending recorded", village name in
  the title); undo to score entry with a new winner replaces the record; export, text and file import after clearing,
  saved-game text rejected; forget player; Polish UI. tagcheck 0 missing. Re-tested on top of the persistent undo
  history: reloading on the ending keeps one record, undo to score entry with a new winner still replaces it.

## Cost of Disease playtest findings (October 2026)

- Findings, causes and a recurrence checklist in [`PLAYTEST_FINDINGS.md`](PLAYTEST_FINDINGS.md); new `tools/sourcecheck.py`.
- Fixed: hub text formatted with stale state (shared hub refresh in `RunScenarioAction`), Gen II end-of-generation
  text now in separate popups, truncated "South" hunt choice, mirror-mode questions show the screen heading.
- Same checks run against Fear of the Unknown: nothing to fix.

## Merge + small fixes (October 2026)

- Fear of the Unknown merged to the fork's `main` (`e18a535`).
- Letter dates move forward within a round; "Conducting Business" dated August to match its text.
- Rankings: equal players share a place unless a tie-breaker separated them (both scenarios).

## Fear of the Unknown - browser play test (October 2026)

- Played with crafted saves (`mkSaveFotU` in `tools/test-save-helpers.js`, auto-drivers `auto`/`autoFirst`): menu
  start, Gen I, Gen II Creature + Foreigners, Gen III Privatized (hunt, Junk Palace, Payment or Death, battle),
  Liberal (asylum assessment, program, taxes), Isolation (verification, smuggling, gorilla rampage), Tension
  (special event, crossroads), scoring with tie-breakers, endings, undo, reload, mirror mode Gen I -> Gen II.
  Cost of Disease regression: end of game + scoring OK. No console errors.
- Fixed: one-time events could be claimed repeatedly (Bloodsmith reward, crossroads, verification) - now once per
  player and the hub link disappears; letter dates after the last round used Gen III Early years; trailing blank
  lines in assembled popups (`TrimTrailingBreaks`); "The Family" as shared winner name; files normalized to CRLF.
- Scenario descriptions in the menu: Cost of Disease "PLAY TESTING", Fear of the Unknown "WORK IN PROGRESS".

## Fear of the Unknown - port (October 2026)

- Engine generalized for several scenarios: `ScenarioVars` base class, shared `FinalScoring`, scenario-aware undo,
  hub lookup, CSV loading, save (`ShouldSerialize*`) and mirror mode. See CLAUDE.md "Adding a scenario".
- All Fear of the Unknown passages ported (Gen I Witch Hunt, Gen II Foreigners/Creature/Obsessions, Gen III
  Isolation/Tension/Privatized/Liberal, battle, asylum assessment, endings) with both CSV files.
- New tools: `tools/cradle_dump.py`; `tools/tagcheck.py --scenario=`.
- Deliberate deviations: CLAUDE.md "Fear of the Unknown - notes and deliberate deviations".

## The Cost of Disease (September 2026)

- Complete port incl. Gen III, final scoring and 8 endings; mirror mode; phone layout. Deviations: CLAUDE.md
  "Cost of Disease - known deviations".
