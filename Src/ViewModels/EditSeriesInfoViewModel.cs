using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;
using Tsundoku.Models.Enums;
using static Tsundoku.Models.Enums.SeriesDemographicEnum;
using static Tsundoku.Models.Enums.SeriesGenreEnum;

namespace Tsundoku.ViewModels;

public sealed class EditSeriesInfoViewModel : ViewModelBase
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    public Series Series { get; set; }
    [Reactive] public int DemographicIndex { get; set; }
    [Reactive] public string CoverImageUrl { get; set; }
    [Reactive] public string GenresToolTipText { get; set; }
    public AvaloniaList<ListBoxItem> SelectedGenres { get; set; } = [];
    private static StringBuilder CurGenres = new StringBuilder();

    public EditSeriesInfoViewModel(Series Series, IUserService userService) : base(userService)
    {
        this.Series = Series;
        this.WhenAnyValue(x => x.Series.Demographic)
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Subscribe(x => DemographicIndex = Array.IndexOf(SERIES_DEMOGRAPHICS, x));

        SelectedGenres.CollectionChanged += SeriesGenresChanged;
    }

    private void SeriesGenresChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                if (SelectedGenres != null && SelectedGenres.Any())
                {
                    foreach (ListBoxItem genre in SelectedGenres.OrderBy(genre => genre.Content.ToString()))
                    {
                        CurGenres.AppendLine(genre.Content.ToString());
                    }
                    GenresToolTipText = CurGenres.ToString().Trim();
                }
                else
                {
                    GenresToolTipText = string.Empty;
                }
                CurGenres.Clear();
                return;
            default:
                LOGGER.Error($"\"{e.Action}\" Failed for Genre Change");
                throw new ArgumentOutOfRangeException();
        }
    }

    public HashSet<SeriesGenre> GetCurrentGenresSelected()
    {
        HashSet<SeriesGenre> newGenres = [];
        foreach (ListBoxItem genreItem in SelectedGenres)
        {
            if (SeriesGenreEnum.TryParse(genreItem.Content.ToString(), out SeriesGenre genre))
            {
                newGenres.Add(genre);
            }
        }
        return newGenres;
    }
}
