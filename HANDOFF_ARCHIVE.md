# Handoff archive

Finished work moved out of [`HANDOFF.md`](HANDOFF.md), newest first. Kept short: what was done and where it is documented.

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
