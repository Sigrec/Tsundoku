using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

namespace Tsundoku.Services;

/// <summary>
/// Applies or removes glassmorphism (acrylic blur) transparency on all application windows.
/// Adjusts window transparency and background color alpha for a frosted glass effect.
/// </summary>
public static class GlassmorphismService
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static bool _isEnabled;

    /// <summary>Alpha value for primary backgrounds when glass is enabled.</summary>
    private const byte GlassAlpha = 0x99; // ~60% opaque

    /// <summary>Resource keys whose background alpha is reduced when glass is enabled.</summary>
    private static readonly string[] GlassBackgroundKeys =
    [
        "TsundokuMenuBGColor",
        "TsundokuCollectionBGColor",
    ];

    /// <summary>Stores the original opaque colors so they can be restored when glass is disabled.</summary>
    private static readonly Dictionary<string, Color> _originalColors = [];

    /// <summary>Gets whether glassmorphism is currently enabled.</summary>
    public static bool IsEnabled => _isEnabled;

    /// <summary>
    /// Applies or removes glassmorphism on all open windows.
    /// </summary>
    public static void Apply(bool enabled)
    {
        _isEnabled = enabled;

        if (Application.Current?.Resources is not Avalonia.Controls.ResourceDictionary resources)
        {
            return;
        }

        if (enabled)
        {
            foreach (string key in GlassBackgroundKeys)
            {
                if (resources.TryGetResource(key, null, out object? value) && value is SolidColorBrush brush)
                {
                    if (!_originalColors.ContainsKey(key))
                    {
                        _originalColors[key] = brush.Color;
                    }

                    Color original = _originalColors[key];
                    resources[key] = new SolidColorBrush(Color.FromArgb(GlassAlpha, original.R, original.G, original.B));
                }
            }
        }
        else
        {
            foreach (string key in GlassBackgroundKeys)
            {
                if (_originalColors.TryGetValue(key, out Color original))
                {
                    resources[key] = new SolidColorBrush(original);
                }
            }
        }

        // Apply transparency to all windows and force-update their backgrounds
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (Window window in desktop.Windows)
            {
                ApplyToWindow(window, enabled);
            }
        }

        LOGGER.Info("Glassmorphism {State}", enabled ? "enabled" : "disabled");
    }

    /// <summary>
    /// Applies glassmorphism transparency settings to a single window.
    /// Also forces named controls to pick up the new resource values.
    /// </summary>
    public static void ApplyToWindow(Window window, bool enabled)
    {
        if (enabled)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur];
            window.Background = Brushes.Transparent;
        }
        else
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.None];
            window.Background = null;
        }

        // Force-update controls that use ControlTheme setters (TemplateBinding caches the initial value)
        ApplyBackgroundToNamedControl(window, "Navigation", "TsundokuMenuBGColor");
        ApplyBackgroundToNamedControl(window, "CollectionPane", "TsundokuCollectionBGColor");
    }

    private static void ApplyBackgroundToNamedControl(Window window, string controlName, string resourceKey)
    {
        Control? control = window.FindControl<Control>(controlName);
        if (control is null) return;

        if (Application.Current?.Resources.TryGetResource(resourceKey, null, out object? value) == true
            && value is SolidColorBrush brush)
        {
            switch (control)
            {
                case Border border:
                    border.Background = brush;
                    break;
                case ScrollViewer scrollViewer:
                    scrollViewer.Background = brush;
                    break;
                case Panel panel:
                    panel.Background = brush;
                    break;
            }
        }
    }
}
