> The sizes below describe the previous multi-platform export. Later UI fixes are in the Unity project and Windows preview; rebuild Web/Android/iOS to include them.

# Soft Candy implementation build report

Built 2026-11-06 in Unity 6000.3.17f1. All six export steps succeeded: Windows preview, Web, YouTube, APK, AAB, iOS Xcode project. Evidence: soft-candy-delivered.log and Builds/delivery-status.txt.

| Export | Bytes | MiB |
|---|---:|---:|
| Web | 9639212 | 9.19 |
| YouTube | 26215127 | 25.00 |
| apk | 25715776 | 24.52 |
| aab | 25627668 | 24.44 |

Web is the complete Brotli-compressed browser folder. YouTube is the raw upload folder; host transport compression is separate. Android numbers are the APK/AAB files, not installed sizes or device-specific store download estimates. Windows is a development preview. iOS is an Xcode project, not a signed IPA.

Texture audit identified uncompressed sock/detail imports despite selected platform formats. GPU-friendly atlas dimensions now produce compressed textures; runtime sprite geometry restores authored cell proportions. Small text/control art remains separate from compressed sprite sheets. No mipmaps or CPU-readable texture copies. Stale atlas references were cleared.

Validation: 9,927 core assertions, 100 unique levels, 2,100 solution simulations; Unity level validation before builds; native matching/win and screen smoke checks; Chrome actual pointer matching, tutorial, win, save/resume, scrolling and landscape checks; high-DPI mobile emulation; host bridge load/save, audio, pause, readiness and earned reward checks. Browser report is in Builds/BrowserQA/report.json. Final settings label correction was checked in native captures. Physical devices, Safari and store/host certification remain unverified.

Visual target: the selected B Soft Candy board, with square outer backgrounds. Actual screenshots: GUI-Preview/index.html. Visual fidelity is improved but pixel-exact reproduction has not been achieved; reconstructed artwork, typography and responsive spacing still differ from the flattened source board. These captures are not a claim of user approval.
