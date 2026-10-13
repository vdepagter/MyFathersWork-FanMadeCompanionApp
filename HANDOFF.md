# Handoff - active work

Only work in progress lives here. When a section is finished, move it to [`HANDOFF_ARCHIVE.md`](HANDOFF_ARCHIVE.md)
(newest first) and delete it from this file. Conventions are in [`CLAUDE.md`](CLAUDE.md).

## Fear of the Unknown - get it onto main (October 2026)

The port and the browser play test are done (see the archive). Everything is still **uncommitted** on branch
`claude/second-scenario-implementation-63bbb7` (worktree `.claude/worktrees/gen3-empty-page-icon-88c676`).
`launch.bat` in the main checkout runs `main`, so Fear of the Unknown is not visible there until this is merged -
that was the "blank screen after selecting the second scenario".

Next steps:
- Commit, open a PR, merge to `main`.
- Play a real game at the table (only crafted-save runs so far).
- Random sub-events not yet seen in the browser (they share code paths with tested ones): Liberal salts / Arise My
  Love / bribe / policy vote, Isolation smuggling trial outcomes, Foreigners Wall "Acceptance" + caravan visits,
  Creature cure accepted path, Gen I fates other than Creature, 2- and 4-player games.

Known small issues (not fixed):
- Letter dates are random flavor per round, so dates in one round are not in order and can contradict the text
  ("9 February" letter saying "it is August").
- Rankings show 1st/2nd/3rd for players with equal scores even when the family shares the win (shared code, same in
  Cost of Disease).
