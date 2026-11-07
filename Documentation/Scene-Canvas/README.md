# Saved Canvas screens and direct gameplay startup

Open `Assets/Scenes/Laundry.unity` in Unity **6000.3.17f1**.

## Editing screens

Expand **Canvas → Portrait safe area**:

- **Screens**: loading, level map, simple gameplay, basket gameplay, shop, and room themes.
- **Popups**: pause, settings, basket full, helpers, tutorials, album, sock detail, level complete, and campaign complete.
- **Overlays**: toast messages and the first-pair instruction.
- **Editable item templates**: sock item, tutorial glove, and tutorial glow.

Select a screen's root and click **Preview this screen in Canvas** in the Inspector. Then edit its child RectTransforms, fonts, colors, image references, and static text. Save the scene **outside Play mode** to retain edits. The game hides inactive screens at startup and activates the appropriate saved objects; it does not regenerate these screens when they open.

Keep `SceneView.id`, binding keys, and required linked objects intact. Object names and positions can be edited. The `SceneView` bindings connect the game to existing objects; they are not a layout generator. You can add persistent button events in the Inspector; runtime binding only replaces nonpersistent game listeners.

Counters, level numbers, collected/locked states, switch values, and progress bars update during play. Socks and short-lived visual effects remain dynamic gameplay content; socks and tutorial pointers use the saved editable templates. The twelve saved level-map rows are recycled while scrolling rather than creating 100 permanent rows.

The **LevelMapView** component on **Level map** exposes the seven-point route positions, node artwork overrides, and number colors. Keep seven entries in each route array. Empty artwork overrides use the existing node art.

Backgrounds and some decorative lettering remain part of the supplied artwork. Editing those pixels requires changing the corresponding image/atlas. All screen objects, their placement, and their bindings are saved in the Canvas. The original illustration has not been regenerated.

Normal builds and Play mode do **not** call the one-time authoring migration. Do not run `CanvasScreensEditor.Create` on an edited scene; it refuses to replace an existing catalog. Use **Sock Pairs → Authoring → Validate saved Canvas screens** to check required references.

## Startup

- New save: level 1 and its interactive tutorial.
- Valid unfinished round: restore level, moves, selected sock, basket, and elapsed time.
- No unfinished round: start the next unlocked level.
- An old saved layout uses the existing safe-restart migration while keeping campaign rewards.
- The map remains available from Pause → Home and the reward screen.

The platform readiness signal is sent after gameplay has initialized.

## Build size and performance

`build-sizes.json` records measured export sizes when packaging completes. Archive size is different from the uncompressed files uploaded to YouTube, runtime memory, or the Unity project's disk size.

Measured exports for this update:

| Target | ZIP | Change from previous ZIP | Exported files |
| --- | --- | --- | --- |
| Web | 7.588 MB | +56.8 KB / +0.75% | 7.670 MB, already Brotli-compressed assets |
| YouTube | 9.512 MB | +64.7 KB / +0.68% | 23.490 MB before host compression |

MB and KB here use decimal bytes. The browser check retained a 64 MiB initial WebAssembly heap. Desktop automation verifies functionality, not low-end mobile frame rate.

The build retains size-focused WebAssembly LTO, IL2CPP size optimization, managed stripping, compressed textures without mipmaps, shared image assets, a 64 MiB initial WebAssembly heap with a 256 MiB maximum, the lowest rendering preset, and disabled shadows/anti-aliasing. The regular Web build uses Brotli; the YouTube target leaves serving compression to its host.

Inactive screens do not render or process UI updates. Popup blur is captured once when a popup opens, and its small render texture is released when the popup closes. The saved screens add serialized object data and some resident UI objects; they do not duplicate the source bitmap assets. These are size-conscious settings, not proof that no further reduction is possible.

## Validation

- Core: `dotnet run --project Tests/SockPairs.Tests.csproj`.
- Canvas integrity: the editor menu above; release builds also reject missing screen bindings.
- Isolated editor integration: `CanvasScreensPlayCheck.Run` with `-sockCanvasQA`; uses a separate test save and checks all 100 layouts, startup/resume, screen reuse, Inspector layout retention, and serialized sprites.
- Browser progression and navigation: the scripts in `Tools/browser-tests`, with `SOCK_URL` pointing to the served Web build and `SOCK_QA_OUTPUT` selecting the report directory.

Earlier GUI hash-baseline scripts document earlier revisions; they are not the validator for this intentional Canvas architecture migration.
