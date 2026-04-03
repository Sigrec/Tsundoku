using Avalonia;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Tsundoku.Models;

namespace Tsundoku.Services;

public interface IThemeResourceService
{
    void ApplyTheme(TsundokuTheme theme);
    IDisposable ObserveAndApply(TsundokuTheme theme);
}

public sealed class ThemeResourceService : IThemeResourceService
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    // Reverse lookup: C# property name → (resource key, getter)
    private static readonly FrozenDictionary<string, (string Key, Func<TsundokuTheme, SolidColorBrush> Getter)> _propertyToResource =
        ThemeResourceKeys.PropertyMap
            .Select(kvp => new KeyValuePair<string, (string, Func<TsundokuTheme, SolidColorBrush>)>(
                kvp.Key.Replace("Tsundoku", string.Empty, StringComparison.Ordinal),
                (kvp.Key, kvp.Value)))
            .ToFrozenDictionary();

    public void ApplyTheme(TsundokuTheme theme)
    {
        if (Application.Current?.Resources is not Avalonia.Controls.ResourceDictionary resources) return;

        // Update each theme resource individually, skipping null values
        foreach (KeyValuePair<string, Func<TsundokuTheme, SolidColorBrush>> kvp in ThemeResourceKeys.PropertyMap)
        {
            SolidColorBrush? brush = kvp.Value(theme);
            if (brush is not null)
            {
                resources[kvp.Key] = brush;
            }
        }

        // Re-apply glassmorphism alpha adjustments if enabled, since theme apply overwrites them
        if (GlassmorphismService.IsEnabled)
        {
            GlassmorphismService.UpdateOriginalColors();
            GlassmorphismService.Apply(true);
        }
    }

    public IDisposable ObserveAndApply(TsundokuTheme theme)
    {
        CompositeDisposable disposable = new();

        void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            string? propName = args.PropertyName;
            if (propName is not null && _propertyToResource.TryGetValue(propName, out _))
            {
                ApplyTheme(theme);
            }
        }

        theme.PropertyChanged += OnPropertyChanged;

        disposable.Add(Disposable.Create(() => theme.PropertyChanged -= OnPropertyChanged));

        return disposable;
    }
}
