## Scene canvas and navigation padding

Laundry.unity now serializes one Canvas, its safe-area root, Screen/Modal/Toast layers, EventSystem and lightweight loading view. SockGame reuses those references at startup; it does not create a duplicate canvas. Individual gameplay screens continue to populate on demand. Scene setup preserves this arrangement when builds regenerate the bootstrap scene.

The navigation bar and its three buttons share vertical centre -825. A 270-unit bar around 216-unit buttons leaves 27 units above and below, plus symmetrical transparent art margins. Runtime checks verify equal padding, a single canvas, in-place toggles and the matching/win flow.

## Latest control and basket corrections

Settings/Pause switches now update only their track and knob. The popup is preserved and its opening animation does not replay. All six switches were tested twice, checking popup identity, hierarchy and scale.

A shared four-sprite control sheet supplies pink/purple pills and circular buttons. Live text/icon groups are centred together with a shared gap. Bottom navigation uses common icon/label coordinates and label sizing. Map links now curve between recycled nodes.

The replacement basket has a thinner rim. Pair positions and landing destinations derive from its rendered image rectangle. Introductory levels display the latest five completed pairs; total completion is tracked separately. Later levels use five reusable holding places for unmatched socks; matching frees space. The distinction has not been changed in game logic. Basket resize checks passed at 540x960, 390x844, 768x1024, 960x540 and 360x640, plus five occupied slots.

This revision is integrated in the Unity project and Windows preview. Other platform exports have not been regenerated for these latest UI changes. Physical devices are not covered by the viewport checks.

# Soft Candy reference implementation

Visual target: `Theme-Options/B-Soft-Candy.png`. User exception: square outer background corners, full rectangular viewport, no rounded outer frame. Cards and buttons retain their rounded reference shapes.

Current shared assets: 24 socks in one 6 x 4 sheet, shallow lavender basket, tutorial glove, gold stars/shop/book/shuffle sheet, coin/heart/map button sheet, sloped shop awning, folded win ribbon, illustrated map and win backgrounds. Raster components are reconstructed from the reference using the built-in image tool. They are not verified pixel-identical extractions. Typography uses two subset Fredoka weights; original OFL licences are in Fonts.

Pink/purple buttons and cream popups use shared sliced surfaces. Small white cards have a thin lavender rim and bottom shadow. The map recycles 12 node hierarchies for all 100 levels. Testing unlock remains enabled: locked campaign nodes remain visually locked but are clickable in testing mode without changing saved campaign progression.

Large sock/icon/detail sheets use high-quality platform texture compression; text, buttons and smaller UI art retain crisp import settings. Mipmaps and CPU-readable texture copies are disabled. Full reference boards and original generation files stay outside Assets. Export sizes are measured in BUILD-REPORT.md, not inferred from source PNG sizes.

See GUI-Preview/index.html for actual native captures of all eight reference screen types. Capture files are implementation evidence, not the mockup. Matching/win flow is automated; physical device performance and store/host certification remain unverified.
