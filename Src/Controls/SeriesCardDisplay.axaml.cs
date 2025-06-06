using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Tsundoku.Models;
using Tsundoku.ViewModels;

namespace Tsundoku.Controls
{
    public partial class SeriesCardDisplay : UserControl
    {
        public static readonly StyledProperty<Series?> SeriesProperty =
                AvaloniaProperty.Register<SeriesCardDisplay, Series?>(nameof(Series));

        public Series? Series
        {
            get => GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        public static readonly StyledProperty<TsundokuTheme?> CardThemeProperty =
            AvaloniaProperty.Register<SeriesCardDisplay, TsundokuTheme?>(nameof(CardTheme));

        public TsundokuTheme? CardTheme
        {
            get => GetValue(CardThemeProperty);
            set => SetValue(CardThemeProperty, value);
        }

        // ‣ 3) (Optional) If you also need CurrentUser.Language, expose that here too:
        public static readonly StyledProperty<string?> LanguageProperty =
            AvaloniaProperty.Register<SeriesCardDisplay, string?>(nameof(Language));

        public string? Language
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
                return;

            await ViewModelBase.OpenSiteLink(Series.Link.ToString());
        }

        private async void CopySeriesTitleAsync(object? sender, PointerPressedEventArgs e)
        {
            if (sender is TextBlock tb)
                await TextCopy.ClipboardService.SetTextAsync(tb.Text);
        }

        private void SubtractVolume(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Series?.CurVolumeCount > 0)
            {
                Series.CurVolumeCount--;
            }
        }

        private void AddVolume(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Series != null && (Series.CurVolumeCount < Series.MaxVolumeCount))
            {
                Series.CurVolumeCount++;
            }
        }

        private async void OpenEditSeriesInfoWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_mainWindowViewModel == null || Series is null)
                return;

            Button seriesButton = (Button)sender;
            seriesButton.Foreground = CardTheme.SeriesButtonIconHoverColor;
            await _mainWindowViewModel.CreateEditSeriesDialog(Series);
            seriesButton.Foreground = CardTheme.SeriesButtonIconColor;
        }
    }
}