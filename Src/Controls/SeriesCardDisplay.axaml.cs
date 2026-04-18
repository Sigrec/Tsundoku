using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.Services;
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

    public static readonly StyledProperty<bool> GlassEnabledProperty =
        AvaloniaProperty.Register<SeriesCardDisplay, bool>(nameof(GlassEnabled));

    public bool GlassEnabled
    {
        get => GetValue(GlassEnabledProperty);
        set => SetValue(GlassEnabledProperty, value);
    }

    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly IUserService _userService;
    private Guid _loadedCoverSeriesId;
    private bool _isAttached;

    public SeriesCardDisplay()
        : this(
            App.ServiceProvider.GetRequiredService<MainWindowViewModel>()
                ?? throw new InvalidOperationException("MainWindowViewModel not registered in DI container."),
            App.ServiceProvider.GetRequiredService<IUserService>()
                ?? throw new InvalidOperationException("IUserService not registered in DI container.")
          )
    {
    }

    public SeriesCardDisplay(MainWindowViewModel mainWindowViewModel, IUserService userService)
    {
        _mainWindowViewModel = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        InitializeComponent();
    }

    static SeriesCardDisplay()
    {
        SeriesProperty.Changed.AddClassHandler<SeriesCardDisplay>((card, e) => card.OnSeriesChanged(e));
    }

    private static readonly Animation _fadeInAnimation = new()
    {
        Duration = TimeSpan.FromMilliseconds(650),
        Easing = new QuadraticEaseOut(),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame { Cue = new Cue(0.0), Setters = { new Setter(OpacityProperty, 0.0) } },
            new KeyFrame { Cue = new Cue(0.3), Setters = { new Setter(OpacityProperty, 0.0) } },
            new KeyFrame { Cue = new Cue(1.0), Setters = { new Setter(OpacityProperty, 1.0) } }
        }
    };

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        if (Series is not null && _loadedCoverSeriesId != Series.Id)
        {
            _userService.LoadSeriesCover(Series.Id);
            _loadedCoverSeriesId = Series.Id;
        }

        _ = _fadeInAnimation.RunAsync(this);
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_loadedCoverSeriesId != Guid.Empty)
        {
            _userService.ReleaseSeriesCover(_loadedCoverSeriesId);
            _loadedCoverSeriesId = Guid.Empty;
        }
        _isAttached = false;
        base.OnDetachedFromVisualTree(e);
    }

    private void OnSeriesChanged(Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (!_isAttached) return;

        if (_loadedCoverSeriesId != Guid.Empty)
        {
            _userService.ReleaseSeriesCover(_loadedCoverSeriesId);
            _loadedCoverSeriesId = Guid.Empty;
        }

        if (e.NewValue is Series newSeries)
        {
            _userService.LoadSeriesCover(newSeries.Id);
            _loadedCoverSeriesId = newSeries.Id;
            _ = _fadeInAnimation.RunAsync(this);
        }
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
            await ClipboardHelper.CopyToClipboardAsync(title);
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
        seriesButton.Background = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesCardButtonBGHoverColor)!;
        seriesButton.BorderBrush = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesCardButtonBorderHoverColor)!;
        await _mainWindowViewModel.CreateEditSeriesDialog(Series);
        seriesButton.Foreground = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesButtonIconColor)!;
        seriesButton.Background = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesCardButtonBGColor)!;
        seriesButton.BorderBrush = (SolidColorBrush)this.FindResource(ThemeResourceKeys.SeriesCardButtonBorderColor)!;
    }
}