# Portrait build configuration

All custom build commands apply portrait orientation settings. Android APK/AAB and iOS retain native portrait orientation. Windows builds open a fixed-size portrait window. Web and YouTube use a full-height portrait canvas, capped at 9:16 width on wider hosts; tall/narrow phone viewports fill their width. Side padding is a plain background, not horizontally stretched game art. Browser orientation is not locked.

Both templates and the existing Builds/Web/index.html and Builds/YouTube/index.html wrappers were updated. Existing compiled Web/YouTube binaries were not rebuilt by this wrapper change; rebuild from the Unity menus for the latest gameplay code. Configuration now preserves an existing Laundry scene and its CanvasScaler.

Browser check: actual Web build loaded and resized at 1920x1080,390x844,768x1024,844x390,360x640; portrait bounds/centering passed. Screenshots are under Builds/BrowserQA/portrait-*.png.

YouTube submission is a web-game ZIP with its entry point selected in the Developer Portal, not an APK/AAB. This layout change is not a certification result.
