# Opening progression

This update changes gameplay data and save handling. GUI artwork, layouts, panels, buttons, scenes, and browser templates are unchanged. Existing tutorial panels show new text at the new learning steps.

| Levels | Purpose |
| --- | --- |
| 1–2 | Three, then four pairs; simple matching without basket pressure. |
| 3 | Match a top pair to uncover another pair. Existing tutorial explains layers. |
| 4–5 | Introduce holding a sock while uncovering its partner, with five basket spaces. |
| 6 | Short relief round. |
| 7–8 | Small dependency puzzles that make selection order matter. |
| 9 | Short relief round. |
| 10 | Combine the opening lessons. |
| 11–20 | Gradually increase from seven to fourteen pairs, with easier rounds between larger puzzles. |
| 21–100 | Original campaign unchanged. |

The first ten levels all have a solution requiring no more than one occupied holding space. Levels 4, 5, 7, 8, and 10 require holding at least one sock: their challenge is not merely a larger sock count. Existing hints can complete all ten.

Old saved campaigns retain coins, stars, unlocked levels, and collected socks. An in-progress round using an older version of a changed level restarts that round safely. Unchanged levels and current-version rounds resume normally. Restored rounds are saved immediately.

## Validation

- `dotnet run --project Tests/SockPairs.Tests.csproj`: all 100 levels, 20 additional seeds per level, reshuffle recovery, independently searched opening puzzles, tutorial placement, and hint completion.
- `python Tools/verify_opening_scope.py`: verifies 96 GUI-related files against the pre-change hashes and confirms levels 21–100 are unchanged.
- `node Tools/browser-tests/opening-progression.test.cjs`: actual browser pointer completion of levels 1–10, rewards, tutorial flow, basket retrieval, and saved-game compatibility.
- `node Tools/browser-tests/reference-b.test.cjs`: existing navigation, settings, progression, resume, and viewport smoke checks.

Reports and browser captures are in this directory. These checks establish functionality, not improved retention.

## First-time-player check

Observe 5–8 people who have never played, without explaining the rules. Record whether they understand the reveal at level 3, how they recover a waiting sock at level 4, where they ask for help, and where they voluntarily stop. After level 5, ask whether they want another round; do not prompt them to continue. Compare this with the original opening before drawing retention conclusions. Keep the artwork identical during the comparison.
