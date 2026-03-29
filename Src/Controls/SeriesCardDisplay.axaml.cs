using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Controls;

public sealed partial class SeriesCardDisplay : UserControl
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public static readonly StyledProperty<Series> SeriesProperty =
            AvaloniaProperty.Register<SeriesCardDisplay, Series>(nameof(Series));

    public Series Series
    {
        get => GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    // ‣ 3) (Optional) If you also need CurrentUser.Language, expose that here too:
    public static readonly StyledProperty<TsundokuLanguage?> LanguageProperty =
        AvaloniaProperty.Register<SeriesCardDisplay, TsundokuLanguage?>(nameof(Language));

    public TsundokuLanguage? Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    private readonly MainWindowViewModel _mainWindowViewModel;
    public SeriesCardDisplay()
        : this(
            // Pull the singleton MainWindowViewModel from your IServiceProvider:
            App.ServiceProvider.GetRequiredService<MainWindowViewModel>()
            ?? throw new InvalidOperationException(
                   "MainWindowViewModel not registered in DI container.")
          )
    {
        // Note: InitializeComponent() is called in the chained ctor below.
    }

    // 2) The “real” DI‐based ctor:
    public SeriesCardDisplay(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel
            ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
        InitializeComponent();
    }

    private async void OpenSiteLink(object? sender, PointerPressedEventArgs e)
    {
        if (Series?.Link is null)
        {
            return;
        }

        await ViewModelBase.OpenSiteLink(Series.Link.ToString());
    }

    private async void CopySeriesTitleAsync(object? sender, PointerPressedEventArgs e)
    {
        if (Language is not null && Series?.Titles is not null)
        {
            string title = Series.Titles.TryGetValue(Language.Value, out string? langTitle)
                ? langTitle
                : Series.Titles[TsundokuLanguage.Romaji];
            LOGGER.Debug("Copying {title} to Clipboard", title);
            await TextCopy.ClipboardService.SetTextAsync(title);
        }
    }

    private void SubtractVolume(object? sender, RoutedEventArgs e)
    {
        Series?.DecrementCurVolumeCount();
    }

    private void AddVolume(object? sender, RoutedEventArgs e)
    {
        Series?.IncrementCurVolumeCount();
    }

    private async void OpenEditSeriesInfoWindow(object? sender, RoutedEventArgs e)
    {
        if (_mainWindowViewModel is null || Series is null || sender is not Button seriesButton)
        {
            return;
        }

        seriesButton.Foreground = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesButtonIconHoverColor)!;
        await _mainWindowViewModel.CreateEditSeriesDialog(Series);
        seriesButton.Foreground = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesButtonIconColor)!;
    }
}