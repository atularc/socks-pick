# Responsive UI pass ? 2026-11-06

Existing Canvas Scaler settings are preserved. Backgrounds cover the physical display; interactive controls retain notch/gesture clearance.

Map: HUD and navigation align to safe edges; same-frame return from Album and Shop tested.
Shop: HUD/awning use the upper edge, offer rows distribute extra height without stretching their internal art/text, Back to Map stays at bottom.
Album: compact grid with paging controls below it; header and collection move together toward the top on tall displays.
Gameplay: header and footer groups adapt independently; basket contents use basket-relative coordinates; effect layer permits animation into the extended footer.
Tutorial: instruction bubble follows the basket region. Dialogs stay centred; overlay dims the entire screen.
Win: full-display artwork, responsive banner and Next placement.

Validation: full screen/match/clear smoke flow at 540x1170 and 768x1024; four-size map/Shop transition checks; five-size basket placement checks. Actual notched device hardware has not been tested; Windows native captures do not reproduce OS cutouts. Main Unity scene was not replaced.

No new texture assets or UI packages. Windows preview updated; Android/iOS/Web exports require rebuilding to include this pass. No new platform build-size measurement was performed.
