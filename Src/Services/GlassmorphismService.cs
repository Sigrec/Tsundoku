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

    /// <summary>Alpha value for menu backgrounds when glass is enabled.</summary>
    private const byte MenuGlassAlpha = 0x99; // ~60% opaque

    /// <summary>Alpha for toast/badge backgrounds when glass is enabled.</summary>
    private const byte ToastGlassAlpha = 0x80; // ~50% opaque

    /// <summary>Alpha for collection background when glass is enabled.</summary>
    private const byte CollectionGlassAlpha = 0x33; // ~20% opaque

    /// <summary>Resource keys and their glass alpha values.</summary>
    private static readonly Dictionary<string, byte> GlassBackgroundKeys = new()
    {
        { "TsundokuMenuBGColor", MenuGlassAlpha },
        { "TsundokuCollectionBGColor", CollectionGlassAlpha },
        { "TsundokuStatusAndBookTypeBGColor", ToastGlassAlpha },
    };

    /// <summary>Stores the original opaque colors so they can be restored when glass is disabled.</summary>
    private static readonly Dictionary<string, Color> _originalColors = [];

    /// <summary>Gets whether glassmorphism is currently enabled.</summary>
    public static bool IsEnabled => _isEnabled;

    /// <summary>
    /// Updates the stored original (opaque) colors from the current theme resources.
    /// Call this after a theme change and before re-applying glass alpha.
    /// </summary>
    /// <summary>Resource keys to track original colors for.</summary>
    private static readonly string[] AllTrackedColorKeys = [.. GlassBackgroundKeys.Keys];

    public static void UpdateOriginalColors()
    {
        if (Application.Current?.Resources is not Avalonia.Controls.ResourceDictionary resources) return;

        foreach (string key in AllTrackedColorKeys)
        {
            if (resources.TryGetResource(key, null, out object? value) && value is SolidColorBrush brush)
            {
                _originalColors[key] = brush.Color;
            }
        }
    }

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
            foreach ((string key, byte alpha) in GlassBackgroundKeys)
            {
                if (_originalColors.TryGetValue(key, out Color original))
                {
                    resources[key] = new SolidColorBrush(Color.FromArgb(alpha, original.R, original.G, original.B));
                }
                else if (resources.TryGetResource(key, null, out object? value) && value is SolidColorBrush brush)
                {
                    _originalColors[key] = brush.Color;
                    resources[key] = new SolidColorBrush(Color.FromArgb(alpha, brush.Color.R, brush.Color.G, brush.Color.B));
                }
            }
        }
        else
        {
            foreach (string key in GlassBackgroundKeys.Keys)
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
        Control? control;
        try
        {
            control = window.FindControl<Control>(controlName);
        }
        catch (InvalidOperationException)
        {
            return;
        }
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
