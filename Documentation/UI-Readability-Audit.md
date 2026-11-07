# UI readability audit ? 6 November 2026

Implemented larger, higher-contrast gameplay labels and helper buttons, live basket counts, unclipped waiting socks, expanded pile bounds for tall screens, corrected clothesline spacing, more room for tutorial text, separated pause icons/labels, and larger touch areas for icon buttons, toggles and HUD plus controls. Removed redundant timer/coin labels that collided with the basket; elapsed time still tracks internally. Album dots are noninteractive indicators; swipe and arrow navigation remain.

## Verification
- All 100 levels at 360x640, 390x844, 768x1024: text minimums, sprite-mask containment and header separation; one screenshot per level/resolution.
- Progression interactions on levels 11, 31, 61, 81 and 100: waiting-sock layer visibility, basket position and current counter. Level 11 can clear without accumulating two waiting socks.
- Screen smoke flow: map, shop, album, settings, play, pause, win, fail, helper offers, every chapter tutorial; no Text preferred-height overflow reported.
- Visual review of representative dense levels, small-phone controls, settings/pause, helper offers and tutorial explanations.
- Early audit caught title overlap on the small-phone and tablet layouts at levels 95?100; the corrected spacing passed subsequent 300-level runs.

Logs: release-audit-small.log, release-audit-tall.log, release-audit-tablet.log, text-screen-audit.log, ui-delivery-controls.log.
Screenshots: GUI-Preview/Verified-360x640, Verified-390x844, Verified-768x1024.

These are native Windows resolution simulations, not physical device tests. Main Unity source and Windows preview are updated; Android/iOS/Web exports must be rebuilt. Existing textures reused; build size was not remeasured for platform exports.

## Footer correction
Removed the redundant completed-pairs drawer from advanced levels; the header remains the progress counter. Matched-pair animation now travels to that counter. Basket/counter/buttons have separate vertical space. Each helper uses two independent centred text fields at +/-24 units with matching fonts and size, inside a 190-unit-high button. Tests inspect rendered glyph vertices, enforce an internal inset, and check gaps between basket, counter and buttons. These checks run on all advanced levels in the 100-level audit at all three resolutions. Logs: button-group-qa.log, footer-small-final.log, footer-tablet-final.log.

Footer optical alignment follow-up: helper title and price group raised 16 units to align with the visible face rather than the shadow/bevel bounds. Your pick is a centred empty-slot placeholder and hides on selection. Native captures verified empty/occupied states; 100-level tall-phone audit rerun. Log: footer-optical-qa.log.
