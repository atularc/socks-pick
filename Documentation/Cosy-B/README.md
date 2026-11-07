# Approved concept B — Cosy Laundry Room

The fixed art reference is [Approved-B.png](Approved-B.png). The implementation uses warm carved wood, deep teal felt, peach enamel buttons, brown live text, a descending level route, and an arched peach reward panel. Screens not illustrated in the concept extend the same materials and controls. The concept is artwork rather than a pixel-level layout specification; real labels and progress remain driven by game state and safe-area layouts adapt to the device.

Open [the runtime screenshot gallery](Review/index.html) to compare the finished game with the reference. The level-three capture matches the progression state shown in B. Screenshots are produced by the actual Unity player, not by image generation.

## Individual assets

Fifteen assets were generated individually with the built-in image-generation tool, using B as the reference. Final files are in `Assets/Resources/Art/`. [asset-manifest.json](asset-manifest.json) records their original generated paths and alpha bounds.

| Asset | Prompt specification / role |
| --- | --- |
| CosyBoard | Empty wooden rounded frame with uninterrupted dark teal felt, matching B's gameplay inset |
| CosyShell | Empty carved maple outer frame and wooden interior |
| CosyButton | Blank peach primary pill, extracted from B's Play control |
| CosySecondary | Blank lighter apricot Album/header control |
| CosyTeal | Blank petrol teal coin-reward pill |
| CosyPopup | Empty arched peach reward panel, wood rim and four brass studs; exterior transparency verified |
| CosyBasket | Empty natural wicker basket, cream cloth liner and teal stripe |
| CosyNode | Blank peach locked-level token |
| CosyGold | Blank golden completed-level token |
| CosyCurrent | Blank coral current-level token / circular control |
| CosyStar | Rounded inflated gold reward star |
| CosyRoom | Portrait warm wooden laundry room, edge plants, towels and windows, no UI or text |
| CosyLogo | Terracotta wooden title plaque with ivory sock emblem; title remains live |
| CosyVine | Isolated trailing leaves for the map edge |
| CosyTowels | Isolated teal, ivory, peach and terracotta towel stack |

Shared prompt requirements: preserve B's material, colour and upper-left lighting; straight-on production sprite; no extra text or unrelated objects; transparent exterior except the room. The popup received a separate background-removal pass. Alpha bounds were inspected without altering the image pixels. The existing fabric sock sheet was reused.

## Runtime and size decisions

- Small tokens and decorations import at 256 pixels, larger controls at 512, and the room at 1024. No mipmaps or readable CPU texture copies.
- Controls share compressed sprite atlas pages. Borders reuse textures; button faces use simple quads to preserve their generated rounded silhouette without nine-slice distortion. Frames retain scalable borders.
- The curved wooden trail samples the shell's wood texture. Stars and basket-front occlusion reuse existing sprites. The hundred-level map recycles twelve rows.
- Text and counters are live. Existing press/release and reward animations respect reduced motion. Depth is baked into sprites; no 3D lights, postprocessing, blur shader or new third-party package.
- The instruction sits inside the early-game teal tray. Reward album progress uses a live fill bar and count. Album headers, basket counts and helper prices have contrasting backing or text colours; popup backdrops are dimmed.
- Old Fresh Laundry room/basket files were moved out of Resources into `Retired-Art`, preventing their inclusion in the new builds. Previous code is preserved in `Before-B`.

Build packages are in `Builds/Cosy-B`. Web uses Brotli with Unity fallback; YouTube leaves compression to the host. Raw YouTube folder size is not its compressed transfer size. Exact measured sizes appear in [Review/build-sizes.json](Review/build-sizes.json).

## Validation and remaining limits

- All 100 levels completed the layout and text checks at 360×640, 540×1170 and 768×1024. The checks include the wooden tray inset, title clearance, sock clipping, helper text padding and basket/footer separation.
- Basket captures cover five aspect ratios and five occupied positions. Controls and safe-area checks cover settings, pause, navigation and tall screens. The gallery contains 33 native runtime captures, including every popup and chapter tutorial.
- The local Web build was exercised with actual pointer input: matching, win/reward, next level, pause/home, reload/resume, scrolling and landscape resize. See the [browser report](Review/BrowserQA/report.json) and its screenshots. Production builds ignore the editor-only unlock-all setting.
- On this Windows PC (RTX 4060), a 180-frame idle advanced-level sample measured 16.82 ms average, 27.42 ms worst frame and zero managed allocation per sampled frame, with Very Low quality. This is a short desktop sample, not a mobile benchmark.
- **Native preview limitation:** the final capture runs completed their assertions and wrote their screenshots, then exited with a UnityPlayer.dll access violation during shutdown (`0xC0000005`). This remains unresolved. The earlier Direct3D 12 capture run also crashed; switching to Direct3D 11 did not eliminate the shutdown issue. The [native report](Review/native-qa.json) preserves the nonzero exit codes instead of treating the runs as clean process passes. Browser interactions completed separately without browser errors.

The local browser builds are not uploaded. These checks do not constitute YouTube host certification or a low-end mobile performance guarantee.
