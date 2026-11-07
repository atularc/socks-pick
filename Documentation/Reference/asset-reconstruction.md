# Reference reconstruction assets

Generated with the built-in image generation tool using `Theme-Options/B-Soft-Candy.png` as the source reference. Assets are saved in `Assets/Resources/Art` and reused across screens.

- CandyAwning: reconstruct the exact seven-strip pink/ivory sloped awning and central raised purple Shop sign; isolate with alpha; preserve reference bevels/scallops.
- CandyDetails: 3 x 2 transparent atlas: shop coin stack, single star coin, pink heart, pink map button, lavender locked map button, gold selected map button. Preserve the reference's candy highlights and white rims.
- CandyRibbon: reconstruct the curved pink folded ribbon with exact text Level Complete!, isolated with alpha, preserve reference proportions.

No rounded outer screen frame. All generated bitmap inputs remain source assets; Unity sprite rectangles and platform import settings handle padding and runtime sizing. Do not label these reconstructions pixel-identical to the source board.

- CandyWinBackground: reconstruct the empty win background, pale lavender/cream centre and shaded pastel clouds in the bottom 15%; square full-bleed corners, no text or controls.

## Component refinement
- CandyBasket replacement prompt: reconstruct only the PLAY basket from the selected board; shallow lavender form, thinner segmented rim, five recessed slots, two woven front rows, about 3.5:1 aspect; transparent, no socks/text/decorations. Previous version retained outside Assets.
- CandyControls prompt: exact 2 x 2 transparent production sheet; empty pink/purple pill buttons above, pink/purple circular controls below; source-matching thin white highlight rim, lower bevel and lavender contact shadow; no text/symbols. Integrated with native live text and glyphs. Old separate pink/purple surface images were moved out of Resources.

Generated using the built-in image tool. The flattened reference is the reconstruction target; these are not source-layer extractions or a verified pixel-identical copy.
