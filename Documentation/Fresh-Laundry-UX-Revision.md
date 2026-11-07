# Fresh Laundry GUI revision — 7 November 2026

## What changed and why

- **Matching grid:** early levels now keep fixed, even row spacing on tall displays. Sparse logical slots are mapped to compact visual cells without changing puzzle generation, the saved puzzle, or its solution. Removed socks leave their cells in place, so remaining targets do not jump around during a level.
- **Completed pairs:** increased artwork from 7% to 16% of basket width. One pair is centred; additional pairs form a centred row. A masked copy of the existing basket front covers the lower edges of the socks, placing them visually inside the basket. No new basket texture is loaded.
- **Surface contrast:** replaced almost-white UI surfaces with honey panels and a muted sage play area. The level route uses a deeper teal surface. Headers and navigation have a dark teal foundation. Popups, including the win screen, dim the room behind them.
- **Level navigation:** one regular winding route replaces the exaggerated curves. Coral and a gold ring identify the current day, with “YOU ARE HERE” above it. Completed days are gold with earned stars; future days are muted and carry locks. The fixed primary action names the next day, and the heading explains upward scrolling.
- **Consistency:** the same colours apply to settings, pause, rewards, collection screens, helper dialogs and the browser loader. The room artwork and fabric socks are preserved.

## Screenshots and builds

Open [the screenshot gallery](Fresh-Laundry-GUI/index.html) for full-size runtime captures of the screens, dialogs, tutorials, basket states and representative device sizes. Captures are from the development desktop player; its native shop button is absent from the Web and YouTube releases.

Local build packages are in `Builds/FreshLaundry-UX`. Each ZIP contains `index.html` at its root. Web uses Unity Brotli compression with fallback; YouTube leaves compression to its host. Raw YouTube folder size is not the expected compressed network transfer size. Measured package sizes are in [build-sizes.json](Fresh-Laundry-GUI/build-sizes.json).

No external upload or publication was performed. This revision has not been manually exercised in a browser or in the YouTube host; native runtime checks and successful WebGL compilation do not substitute for that platform validation.

## Validation

- Runtime matching/level-complete flow and screen captures.
- All 100 levels checked at 360×640, 540×1170 and 768×1024 for artwork bounds, title separation, text sizing, helper padding and footer separation.
- Basket tested at five aspect ratios, including one centred pair and five occupied slots.
- Navigation checked across four viewport sizes, including transitions back from Album and Shop.
- Settings and pause toggles checked without reconstructing their popup.
- Lowest rendering quality remains active, with anti-aliasing and shadows disabled. The button depth is static UI vertex shading, not real-time 3D lighting.

The `ux-*.log` files in the project root contain this revision's results. Desktop timing samples are not a low-end phone performance guarantee.

## Art provenance

The previous Fresh Laundry pass used the built-in image-generation tool for the portrait room background, transparent fabric sock sheet and transparent woven basket. This revision adds no generated art.

- `Assets/Resources/Art/FreshLaundryBackground.png`: cosy cream laundry room, mint edge tiles, sunlight, three socks on a top clothesline, plants and towels around an empty centre; no text or controls.
- `Assets/Resources/Art/FabricSocks.png`: six-column, four-row transparent sock sheet; pink, blue, yellow, green, purple and coral; stripe, dot, star and zigzag patterns in soft knitted fabric, preserving the existing sprite layout.
- `Assets/Resources/Art/FreshBasket.png`: shallow mint wicker basket with cream rim, visible interior and soft toy-like lighting; transparent background, no socks or handles.

The raised controls are reusable procedural meshes; generated button images are not shipped. Previous source files from this revision are preserved in `Documentation/UX-Revision-Backup`.
