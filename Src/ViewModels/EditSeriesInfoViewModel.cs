using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.ViewModels
{
    public class EditSeriesInfoViewModel : ViewModelBase
    {
        public Series Series { get; set; }
        public Button Button { get; set; }
        [Reactive] public int DemographicIndex { get; set; }
        [Reactive] public string CoverImageUrl { get; set; }
        [Reactive] public string GenresToolTipText { get; set; }
        public static ObservableCollection<ListBoxItem> SelectedGenres { get; set; } = new ObservableCollection<ListBoxItem>();
        private static StringBuilder CurGenres = new StringBuilder();

        public EditSeriesInfoViewModel(Series Series, Button Button)
        {
            this.Series = Series;
            this.Button = Button;
            this.CurCurrency = MainUser.Currency;
            this.CurrentTheme = MainUser.SavedThemes.First(theme => theme.ThemeName.Equals(MainUser.MainTheme));

            this.WhenAnyValue(x => x.Series.Demographic).ObserveOn(RxApp.TaskpoolScheduler).Subscribe(x => DemographicIndex = Array.IndexOf(DEMOGRAPHICS, x));

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

        public static HashSet<Genre> GetCurrentGenresSelected()
        {
            HashSet<Genre> newGenres = [];
            foreach (ListBoxItem genreItem in SelectedGenres)
            {
                Genre genre = GenreExtensions.GetGenreFromString(genreItem.Content.ToString());
                if (genre != Genre.None)
                {
                    newGenres.Add(genre);
                }
            }
            return newGenres;
        }
    }
}
