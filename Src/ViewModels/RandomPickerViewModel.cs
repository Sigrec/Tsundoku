using System.Collections.ObjectModel;
using Avalonia.Collections;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Tsundoku.Models;
using Tsundoku.Models.Enums;
using Tsundoku.Services;

namespace Tsundoku.ViewModels;

public sealed partial class RandomPickerViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly ISharedSeriesCollectionProvider _sharedSeriesProvider;
    private readonly Random _rng = new();
    private Guid _lastPickedId = Guid.Empty;

    [Reactive] public partial Series? PickedSeries { get; set; }
    [Reactive] public partial uint NextVolumeToBuy { get; set; }
    [Reactive] public partial int EligibleCount { get; set; }
    [Reactive] public partial string EmptyStateMessage { get; set; } = string.Empty;
    [Reactive] public partial bool HasPick { get; set; }
    [Reactive] public partial string MainTitle { get; set; } = string.Empty;
    public AvaloniaList<string> OtherTitles { get; } = [];

    public RandomPickerViewModel(IUserService userService, ISharedSeriesCollectionProvider sharedSeriesProvider) : base(userService)
    {
        _sharedSeriesProvider = sharedSeriesProvider ?? throw new ArgumentNullException(nameof(sharedSeriesProvider));
    }

    public void Roll()
    {
        ReadOnlyObservableCollection<Series> pool = _sharedSeriesProvider.DynamicUserCollection;
        List<Series> eligible = [.. pool.Where(s => s.CurVolumeCount < s.MaxVolumeCount)];
        EligibleCount = eligible.Count;

        if (eligible.Count == 0)
        {
            ReleaseCurrentCover();
            PickedSeries = null;
            NextVolumeToBuy = 0;
            HasPick = false;
            MainTitle = string.Empty;
            OtherTitles.Clear();
            EmptyStateMessage = pool.Count == 0
                ? "Your current view has no series — try clearing the filter or shelf."
                : "Every series in the current view is complete. Nothing to add.";
            LOGGER.Info("Random picker rolled with no eligible series (view size {ViewSize})", pool.Count);
            return;
        }

        Series next;
        if (eligible.Count == 1)
        {
            next = eligible[0];
        }
        else
        {
            do
            {
                next = eligible[_rng.Next(eligible.Count)];
            } while (next.Id == _lastPickedId);
        }

        ReleaseCurrentCover();
        _lastPickedId = next.Id;
        _userService.LoadSeriesCover(next.Id);

        PickedSeries = next;
        NextVolumeToBuy = next.CurVolumeCount + 1;
        HasPick = true;
        EmptyStateMessage = string.Empty;
        PopulateTitles(next);
        LOGGER.Info("Random picker rolled '{Title}' (Vol {Vol} of {Max})", MainTitle, NextVolumeToBuy, next.MaxVolumeCount);
    }

    private void PopulateTitles(Series series)
    {
        TsundokuLanguageModel.TsundokuLanguage lang = CurrentUser?.Language ?? TsundokuLanguageModel.TsundokuLanguage.Romaji;

        string primary = series.Titles.TryGetValue(lang, out string? langTitle) && !string.IsNullOrWhiteSpace(langTitle)
            ? langTitle
            : series.Titles.TryGetValue(TsundokuLanguageModel.TsundokuLanguage.Romaji, out string? romaji) && !string.IsNullOrWhiteSpace(romaji)
                ? romaji
                : series.Titles.Values.FirstOrDefault() ?? string.Empty;

        MainTitle = primary;

        OtherTitles.Clear();
        foreach (KeyValuePair<TsundokuLanguageModel.TsundokuLanguage, string> kv in series.Titles)
        {
            if (string.IsNullOrWhiteSpace(kv.Value)) continue;
            if (string.Equals(kv.Value, primary, StringComparison.Ordinal)) continue;
            OtherTitles.Add(kv.Value);
        }
    }

    public void Reset()
    {
        ReleaseCurrentCover();
        PickedSeries = null;
        NextVolumeToBuy = 0;
        HasPick = false;
        MainTitle = string.Empty;
        OtherTitles.Clear();
        EmptyStateMessage = string.Empty;
        _lastPickedId = Guid.Empty;
    }

    private void ReleaseCurrentCover()
    {
        if (_lastPickedId != Guid.Empty)
        {
            _userService.ReleaseSeriesCover(_lastPickedId);
        }
    }
}
