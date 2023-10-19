using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System.Collections.Specialized;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        [Reactive] public string TitleText { get; set; }
        [Reactive] public string MaxVolumeCount { get; set; }
        [Reactive] public string CurVolumeCount { get; set; }
        [Reactive] public string AdditionalLanguagesToolTipText { get; set; }
        [Reactive] public bool IsAddSeriesButtonEnabled { get; set; } = false;
        public ObservableCollection<ListBoxItem> SelectedAdditionalLanguages { get; set; } = new ObservableCollection<ListBoxItem>();
        private static readonly HttpClient AddCoverHttpClient = new HttpClient();
        private static readonly StringBuilder CurLanguages = new StringBuilder();
        public AddNewSeriesViewModel()
        {
            this.CurrentTheme = MainUser.SavedThemes.First(theme => theme.ThemeName.Equals(MainUser.MainTheme));
            SelectedAdditionalLanguages.CollectionChanged += AdditionalLanguagesCollectionChanged;
        }

        private void AdditionalLanguagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (SelectedAdditionalLanguages != null && SelectedAdditionalLanguages.Any())
                    {
                        for (int x = 0; x < SelectedAdditionalLanguages.Count - 1; x++)
                        {
                            CurLanguages.AppendLine(SelectedAdditionalLanguages[x].Content.ToString());
                        }
                        CurLanguages.Append(SelectedAdditionalLanguages.Last().Content.ToString());
                        AdditionalLanguagesToolTipText = CurLanguages.ToString();
                        CurLanguages.Clear();
                    }
                    else
                    {
                        CurLanguages.Clear();
                        AdditionalLanguagesToolTipText = CurLanguages.ToString();
                    }
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
        
        /// <summary>
        /// Recieves the call from the AddNewSeriesWindow Button "Add" press which sends the data to this method which calls for the method to get the series data and create a new series object. Then it checks to see if it's a valid series and whether the series already exists in the users collection if not then it adds.
        /// </summary>
        /// <param name="title">The titles of the new series to add</param>
        /// <param name="bookType">The book type or format of the series to add either a Manga(Comic) or Novel</param>
        /// <param name="curVolCount">The current # of volumes the user has collected for this series</param>
        /// <param name="maxVolCount">The max # of volumes this series currently has</param>
        /// <param name="additionalLanguages">Additional languages to get more info for from Mangadex</param>
        /// <returns>Whether the series can be added to the users collection or not</returns>
        public static async Task<bool> GetSeriesDataAsync(string title, Format bookType, ushort curVolCount, ushort maxVolCount, ObservableCollection<string> additionalLanguages)
        {
            Series? newSeries = await Series.CreateNewSeriesCardAsync(title, bookType, maxVolCount, curVolCount,  additionalLanguages);
            
            bool duplicateSeriesCheck = true;
            if (newSeries != null)
            {
                duplicateSeriesCheck = MainWindowViewModel.UserCollection.AsParallel().Any(series => series.Equals(newSeries));

                if (!duplicateSeriesCheck)
                {
                    LOGGER.Info($"\nAdding New Series -> \"{title}\" | {bookType} | {curVolCount} | {maxVolCount}\n{newSeries}");

                    int index = MainWindowViewModel.UserCollection.BinarySearch(newSeries, new SeriesComparer(MainUser.CurLanguage));
                    index = index < 0 ? ~index : index;
                    LOGGER.Debug(index);
                    if (MainWindowViewModel.UserCollection.Count == MainWindowViewModel.SearchedCollection.Count)
                    {
                        LOGGER.Debug("No Filter Insert");
                        MainWindowViewModel.SearchedCollection.Insert(index, newSeries);
                    }
                    else if (DetermineFilter(newSeries, ((Views.MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow).CollectionViewModel.CurFilter) || (MainWindowViewModel.SearchIsBusy && (newSeries.Titles.Values.AsParallel().Any(title => title.Contains(MainWindowViewModel.CurSearchText, StringComparison.OrdinalIgnoreCase)) || newSeries.Staff.Values.AsParallel().Any(staff => staff.Contains(MainWindowViewModel.CurSearchText, StringComparison.OrdinalIgnoreCase)))))
                    {
                        int searchedIndex = MainWindowViewModel.SearchedCollection.ToList().BinarySearch(newSeries, new SeriesComparer(MainUser.CurLanguage));
                        searchedIndex = searchedIndex < 0 ? ~searchedIndex : searchedIndex;
                        if (MainWindowViewModel.SearchedCollection.Count == 0 || searchedIndex == MainWindowViewModel.SearchedCollection.Count)
                        {
                            MainWindowViewModel.SearchedCollection.Add(newSeries);
                        }
                        else
                        {
                            MainWindowViewModel.SearchedCollection.Insert(searchedIndex, newSeries);
                        }
                    }
                    MainWindowViewModel.UserCollection.Insert(index, newSeries);
                }
                else
                {
                    LOGGER.Info($"{title} Already Exists Not Adding");
                }
            }
            return duplicateSeriesCheck;
        }

        public static async Task<Bitmap> SaveCoverAsync(string newPath, string coverLink)
        {
            //using (FileStream fs = new(newPath, FileMode.Create, FileAccess.Write));
            HttpResponseMessage response = await AddCoverHttpClient.GetAsync(new Uri(coverLink));
            using (FileStream fs = new(newPath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fs);
            }
			Bitmap newCover = new Bitmap(newPath).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
			newCover.Save(newPath, 100);
            return newCover;
        }

        public static ObservableCollection<string> ConvertSelectedLangList(ObservableCollection<ListBoxItem> SelectedLangs)
        {
            return new ObservableCollection<string>(SelectedLangs.Select(lang => lang.Content.ToString()).OfType<string>());
        }

        public static bool DetermineFilter(Series newSeries, string filter)
        {
            switch (filter)
            {
                case "Ongoing":
                    return newSeries.Status == Status.Ongoing;
                case "Finished":
                    return newSeries.Status == Status.Finished;
                case "Hiatus":
                    return newSeries.Status == Status.Hiatus;
                case "Cancelled":
                    return newSeries.Status == Status.Cancelled;
                case "Complete":
                    return newSeries.MaxVolumeCount == newSeries.CurVolumeCount;
                case "Incomplete":
                    return newSeries.MaxVolumeCount != newSeries.CurVolumeCount;;
                case "Manga":
                    return newSeries.Format != Format.Novel;
                case "Novel":
                    return newSeries.Format == Format.Novel;
                case "Shounen":
                    return newSeries.Demographic == Demographic.Shounen;
                case "Shoujo":
                    return newSeries.Demographic == Demographic.Shoujo;
                case "Seinen":
                    return newSeries.Demographic == Demographic.Seinen;
                case "Josei":
                    return newSeries.Demographic == Demographic.Josei;
                case "Read":
                case "Unread":
                case "Rating":
                case "Cost":
                case "Query":
                case "None":
                case "Favorites":
                default:
                    return false;
            }
        }
    }
}
