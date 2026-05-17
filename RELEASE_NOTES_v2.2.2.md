# Tsundoku v2.2.2

## Changes

- Card Border color in the theme picker now updates live as you change it — previously the new color only appeared after restarting or switching themes
- Pasting a long cover image URL into Edit Series no longer stretches the window — the textbox stays at its normal width and scrolls horizontally
- Color picker swatches now have a small inset between the border ring and the color itself for a cleaner look
- Price Analysis no longer crashes when the website selection is bulk-cleared or reordered
- User icon picker decodes at the target display size instead of loading the full source image — uses far less memory for high-resolution icons
- Theme picker live preview is smoother — fewer allocations while dragging color values
- Upgraded Avalonia UI framework to 12.0.3 for upstream rendering fixes and stability improvements
