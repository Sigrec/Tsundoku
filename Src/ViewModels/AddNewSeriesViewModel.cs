using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System.Collections.Specialized;
using ReactiveUI;
using System.Reactive.Linq;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        [Reactive] public string TitleText { get; set; }
        [Reactive] public string PublisherText { get; set; }
        [Reactive] public string CoverImageUrl { get; set; }
        [Reactive] public string MaxVolumeCount { get; set; }
        [Reactive] public string CurVolumeCount { get; set; }
        [Reactive] public bool AllowDuplicate { get; set; } = false;
        [Reactive] public string AdditionalLanguagesToolTipText { get; set; }
        [Reactive] public bool IsAddSeriesButtonEnabled { get; set; } = false;
        public static ObservableCollection<ListBoxItem> SelectedAdditionalLanguages { get; set; } = new ObservableCollection<ListBoxItem>();
        private static readonly StringBuilder CurLanguages = new StringBuilder();
        public AddNewSeriesViewModel()
        {   
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
                        foreach (ListBoxItem lang in SelectedAdditionalLanguages.OrderBy(lang => lang.Content.ToString()))
                        {
                            CurLanguages.AppendLine(lang.Content.ToString());
                        }
                        AdditionalLanguagesToolTipText = CurLanguages.ToString().Trim();
                    }
                    else
                    {
                        AdditionalLanguagesToolTipText = string.Empty;
                    }
                    CurLanguages.Clear();
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
        public static async Task<KeyValuePair<bool, string>> GetSeriesDataAsync(string title, Format bookType, ushort curVolCount, ushort maxVolCount, ObservableCollection<string> additionalLanguages, string customImageUrl = "", string publisher = "Unknown", Demographic demographic = Demographic.Unknown, uint volumesRead = 0, decimal rating = -1, decimal value = 0, bool alllowDuplicate = false)
        {
            string returnMsg = string.Empty;
            Series? newSeries = await Series.CreateNewSeriesCardAsync(title, bookType, maxVolCount, curVolCount, additionalLanguages, publisher, demographic, volumesRead, rating, value, customImageUrl);
            bool duplicateSeriesCheck = true;
            if (newSeries != null)
            {
                duplicateSeriesCheck = !alllowDuplicate && MainWindowViewModel.UserCollection.AsParallel().Any(series => series.Equals(newSeries));

                if (alllowDuplicate || !duplicateSeriesCheck)
                {
                    try
                    {
                        LOGGER.Info($"\nAdding New Series (Is Dupe ?= {duplicateSeriesCheck}) -> \"{title}\" | \"{bookType}\" | {curVolCount} | {maxVolCount}\n{newSeries}");
                        int index = MainWindowViewModel.UserCollection.BinarySearch(newSeries, new SeriesComparer(MainUser.CurLanguage));
                        index = index < 0 ? ~index : index;
                        if (MainWindowViewModel.UserCollection.Count == MainWindowViewModel.SearchedCollection.Count)
                        {
                            LOGGER.Debug("No Filter Insert");
                            MainWindowViewModel.SearchedCollection.Insert(index, newSeries);
                        }
                        else if (
                            DetermineFilter(newSeries, (((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow as Views.MainWindow).ViewModel.CurFilter)
                            ||
                            (MainWindowViewModel.SearchIsBusy && (newSeries.Titles.Values.AsParallel().Any(title => title.Contains(MainWindowViewModel.CurSearchText, StringComparison.OrdinalIgnoreCase)) || newSeries.Staff.Values.AsParallel().Any(staff => staff.Contains(MainWindowViewModel.CurSearchText, StringComparison.OrdinalIgnoreCase))))
                        )
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
                        LOGGER.Debug("{} | {}", newSeries.VolumesRead, newSeries.Value);
                        MainWindowViewModel.UserCollection.Insert(index, newSeries);
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Error("Error adding new Series to Collection\n{msg}", ex.Message);
                        newSeries?.Dispose();
                    }
                }
                else
                {
                    returnMsg = "Duplicate Series";
                    LOGGER.Info("{} Already Exists Not Adding", newSeries.Titles["Romaji"]);
                }
            }

            if (duplicateSeriesCheck)
            {
                LOGGER.Debug("Disposing {series}", newSeries?.Titles["Romaji"]);
                newSeries?.Dispose();
            }
            return new KeyValuePair<bool, string>(duplicateSeriesCheck, returnMsg);
        }

        public static ObservableCollection<string> ConvertSelectedLangList(ObservableCollection<ListBoxItem> SelectedLangs)
        {
            return new ObservableCollection<string>(SelectedLangs.Select(lang => lang.Content.ToString()).OfType<string>());
        }

        public static bool DetermineFilter(Series newSeries, TsundokuFilter filter)
        {
            return filter switch
            {
                TsundokuFilter.Ongoing => newSeries.Status == Status.Ongoing,
                TsundokuFilter.Finished => newSeries.Status == Status.Finished,
                TsundokuFilter.Hiatus => newSeries.Status == Status.Hiatus,
                TsundokuFilter.Cancelled => newSeries.Status == Status.Cancelled,
                TsundokuFilter.Complete => newSeries.MaxVolumeCount == newSeries.CurVolumeCount,
                TsundokuFilter.Incomplete => newSeries.MaxVolumeCount != newSeries.CurVolumeCount,
                TsundokuFilter.Manga => newSeries.Format != Format.Novel,
                TsundokuFilter.Novel => newSeries.Format == Format.Novel,
                TsundokuFilter.Shounen => newSeries.Demographic == Demographic.Shounen,
                TsundokuFilter.Shoujo => newSeries.Demographic == Demographic.Shoujo,
                TsundokuFilter.Seinen => newSeries.Demographic == Demographic.Seinen,
                TsundokuFilter.Josei => newSeries.Demographic == Demographic.Josei,
                _ => false,
            };
        }
    }
}
