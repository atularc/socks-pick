> Superseded by the [Unity editor screen audit](../Editor-Screen-Audit/README.md), which fixes the editor rendering failure and includes current screenshots and browser checks.

# Reference B correction

[Open the reference/runtime comparison and all screens](Review/index.html). The comparison displays the approved artwork beside real game captures, cropped to their wooden frames at equal sizes.

This revision corrects the rejected wood, framing, stretched layouts, missing reward decorations and menu-button placement. `Assets/Resources/Art/ReferenceB.png` is an unchanged copy of the approved image. The renderer samples individual regions directly; it does not generate another interpretation of those pieces.

## What changed

- The wood and narrow carved frames use B's original artwork. Unused generated shell and logo files were moved out of Resources into `Retired-Art`.
- Each screen keeps its reference proportions within the safe area. Tall phones show more room outside the frame instead of stretching the frame and separating its controls.
- The level map has B's grouped first nodes, curved wooden route, original title plaque and original Play/Album button faces. The Play number stays live. Full Settings is accessible from Pause.
- Gameplay uses the measured header, instruction, tray and basket proportions. Header studs and instruction accent marks come from the reference. The sock grid adapts to the real level's number of pairs. Touch bounds follow the visible fabric instead of its transparent padding.
- The reward panel preserves the original emblem, accent marks, headline, stars, layered coin, album icon, studs and Next button artwork. The coin amount and collection progress remain live. The game has 24 designs; the reference's illustrative `3 / 12` is therefore replaced with the true count.
- The reward keeps a soft-focus image of the actual gameplay behind it, with a sharp outer frame. A small 192-pixel-wide render texture is blurred once when the reward opens, rather than running a blur effect every frame. Temporary render targets are restored and released.

The reference illustrates three screens and a six-pair first level. Other levels, fewer earned stars, replay rewards and additional menus must reflect the actual game state. Their functional differences are retained rather than hard-coding the example values from the artwork.

## Validation

The 100-level layout checks completed at 360×640, 540×1170 and 768×1024, including title clearance, tray-edge clearance using measured opaque sock bounds, text clipping and footer spacing. Controls, four safe-area layouts and five basket aspect ratios also completed their assertions. [Native evidence](Review/native-qa.json).

A short desktop idle sample on the RTX 4060 measured 16.67 ms average and worst frame, with zero managed allocation per sampled frame at Very Low quality. This is not a mobile benchmark or a measurement of the one-time reward capture.

The [browser report](Review/BrowserQA/report.json) records the actual pointer interactions tested on the packaged Web build. [Package sizes](Review/build-sizes.json) report the Web files and both ZIP archives. Files are in `Builds/Reference-B` and have not been uploaded.

The existing Windows preview shutdown issue remains: native assertions and captures complete, then UnityPlayer.dll exits with `0xC0000005`. The native report retains the nonzero process exit codes. Browser testing is separate; no YouTube host certification is claimed.

Implementation backups are in `Before`. The previous `Documentation/Cosy-B` review is retained as the prior version.

