# Sock Pairs

Latest update: [editable Canvas screens, direct startup, and build measurements](Documentation/Scene-Canvas/README.md). The game starts level 1 or resumes the player's round directly. All 29 screens/overlays are saved in the Canvas; select one and use **Preview this screen in Canvas** to edit it. Layers begin at level 3 and the basket at level 4. Open with Unity 6000.3.17f1. Build packages, caches, generated screenshots, and historical backup folders are excluded from Git. Later design notes describe earlier iterations.

A portrait Unity puzzle game based on the supplied **Color Sort Game Ideas** document.

## Open and play

1. Add this folder in Unity Hub using **Unity 6000.3.17f1**.
2. Open `Assets/Scenes/Laundry.unity` and press Play, or choose **Sock Pairs > Play the game**.
3. Gameplay opens directly. Use **Pause > Home** to reach the scrolling level map.

The project uses a small dependency set: uGUI, built-in Unity modules, C#, compact JSON data and sprite atlases. No paid plugins, accounts or backend are needed to play.

## Implemented

- 100 distinct seeded levels using the supplied pair counts and chapter progression.
- Solvability witness for every level; tests exercise 2,100 complete generated solutions, plus randomized play and reshuffle recovery.
- Matching by design and size; selectable front socks; five-slot basket; selected sock; free retries.
- Odd socks, washer gates, moving clothesline, knots, wet socks and size variants.
- Vertically scrolling map with twelve recycled rows, level locks, stars and current-level positioning.
- Reference-style map, play area, pause, win, loss, helper, shop and collection screens.
- Original six sock drawings/balls from the document, with additional designs for later chapters.
- Live font rendering and scalable GUI shapes; no flattened screenshots used as interface panels.
- Curved match flights, folding/rolling transition, drawer landings, sparkles, confetti, button motion, entrance animation and basket warning.
- Generated sound effects and a short generated music loop: no downloaded audio files.
- Save/resume, coins, stars, collection, earned helper packs and room themes.
- Settings for sound, music, vibration and reduced motion.
- YouTube bridge for readiness, save/load, audio, pause/resume and opt-in rewarded ads.
- Separate browser and YouTube templates; Android APK/AAB, iOS Xcode export and Windows preview build commands.

## Build

Install Android, iOS and WebGL modules for the specified editor. The menu **Sock Pairs > Build** provides all build targets.

With Unity closed, PowerShell commands from the project folder:

```powershell
.\Tools\build.ps1 Web
.\Tools\build.ps1 YouTube
.\Tools\build.ps1 AndroidApk
.\Tools\build.ps1 Android
.\Tools\build.ps1 IOS
```

`Web` uses Brotli with a fallback decompressor and works with a simple local HTTP server. `YouTube` uses the Playables template and uncompressed upload files so YouTube can apply its own serving compression. Never open the web HTML directly with `file://`.

```powershell
python -m http.server 8765 --bind 127.0.0.1 --directory Builds/Web
```

Open `http://127.0.0.1:8765`.

Android uses ARM64, IL2CPP and code stripping. Configure your release signing key before Play Store submission; generated local builds use development signing. iOS output is an **Xcode project**, not a signed IPA: compile/sign on a Mac or a macOS build service. Bundle IDs currently use `com.saturagames.sockpairs` and can be changed before release.

## Size and performance

The project source, Unity Library cache, Windows debugging build and iOS Xcode export are **not** download-size measurements for the mobile/web game. See `Documentation/BUILD-REPORT.md` for measured deliverables.

Web builds use size-focused WebAssembly LTO, managed code stripping, a 64 MiB initial / 256 MiB maximum WebAssembly heap, and a capped device pixel ratio. The GUI uses reusable small shapes and one 28 KB font. Sock graphics share atlases. Existing sock views update in place after taps. The map keeps only twelve rows alive. Effects are bounded and can be reduced in Settings.

## Tests and level editing

```powershell
dotnet run --project Tests/SockPairs.Tests.csproj
```

In Unity, **Sock Pairs > Validate all 100 levels** runs the witnesses using Unity's runtime. **Sock Pairs > Level workbench** edits seeds/counts/mechanics, previews layouts and validates before saving. Source parameters are retained in `Tools/source-levels.json`. Runtime data is `Assets/Resources/Data/levels.json`.

`SmokeTest` is development-only. Launch a Windows development build with `-sockQA` to run the match/win flow and write screenshots under its `QA` folder. QA uses a separate save key. Browser test tooling is under `Tools/browser-tests`; its dependencies are not included in player builds.

## Release work still requiring external validation/configuration

- Human difficulty and retention playtesting: a mathematical solution does not establish that a level is enjoyable or matches a target human win rate. The supplied bot target percentages are retained as design targets, not claimed measured outcomes.
- Low-end Android, iPhone/iPad, Safari and YouTube app device testing; no connected phone has been certified by the automated desktop tests.
- YouTube partner/developer access, portal settings and SDK certification. Rewarded ads require that environment; preview builds do not simulate successful ad viewing.
- Store accounts, release signing, store metadata, privacy policy and billing/ad network configuration.
- Cash purchase cards reproduce the reference layout and example prices, but **billing is not connected**. Tapping them explains this. Coins from levels and the 800-coin helper pack are functional. There are no fake payments or mandatory ads.
- Platform analytics/leaderboards and a server-backed daily shared-score mode are not connected. The 100-level campaign works locally.

The latest visual direction is original artwork within the colourful laundry theme, following your request to redesign the GUI with depth. Gameplay has a recessed tray, raised controls, shaded socks, bounded entrance effects and separate header/basket areas. Matching, rolling, sparkle, confetti, star reveal and button animations remain active; device review is still required.

## Asset provenance

`Documentation/Reference` contains the extracted user-provided reference images. The active Soft Candy sock/icon sheets, basket and glove were generated from the user-selected concept. `Tools/prepare_assets.py` supplies rolled-pair sprites and compact reusable bevel surfaces. No low-resolution document crops are loaded by the game. The utility generator requires Pillow and NumPy. Lilita One is licensed under the SIL Open Font License; see `Documentation/FONT-LICENSE.txt`. Reference files, scripts, tests, licenses and screenshots outside `Assets` are not included in Unity player builds.

## Unlock all levels for testing

Select **SockPairs** in the Laundry scene hierarchy. Under **Testing**, **Unlock All Levels For Testing** is enabled. Set `SockGame.unlockAllLevelsForTesting` to false (or uncheck it in the Inspector) to restore normal map locks; restart Play mode after changing it.

This exposes all 100 level buttons without setting saved `unlocked` to 100. Completing a test level beyond normal progression does not advance campaign unlocks or leave a locked resume point. Coins, stars and collection rewards still save normally. Normal unlocked-level completion still progresses the campaign. Disable the boolean before creating release builds. Existing exported builds do not change until rebuilt.

## Selected theme: B ? Soft Candy

The active theme follows `Documentation/Theme-Options/B-Soft-Candy.png`: pale lavender backgrounds, berry-pink primary controls, lavender secondary controls/basket, rounded colourful socks, rounded gold stars, winding map path, and a white glove tutorial pointer. The tutorial waits for real taps and points to a matching sock after the player's first selection.

Production art is in `Assets/Resources/Art/CandySocks.png`, `CandyIcons.png`, `CandyHand.png` and `CandyBasket.png`. Sock/icon sheets share textures and are sliced into sprites in memory once, without copying their pixels. Small reusable nine-slice panels/buttons and live text construct the layouts. The reference concept board and retired art stay outside Assets and are not shipped. `Tools/prepare_assets.py` maintains reusable shape and rolled-pair assets; it does not regenerate the approved sock sheet.

The portrait layout adapts the reference to actual game controls, screen proportions, dynamic basket capacity and the 100-level rules. The concept's decorative lives HUD is not a new energy mechanic. Shop cash offers are still disabled previews. The testing unlock boolean remains enabled. See the current screenshots and measured build report in Documentation.
