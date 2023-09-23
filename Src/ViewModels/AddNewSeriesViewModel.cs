using System.Threading.Tasks;
using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.IO;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        public static readonly string[] AvailableLanguages = ["Arabic", "Azerbaijan", "Bengali", "Bulgarian", "Burmese", "Catalan", "Chinese", "Croatian", "Czech", "Danish", "Dutch", "Esperanto", "Estonian", "Filipino", "Finnish", "French", "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian", "Italian", "Kazakh", "Korean", "Latin", "Lithuanian", "Malay", "Mongolian", "Nepali", "Norwegian", "Persian", "Polish", "Portuguese", "Romanian", "Russian", "Serbian", "Slovak", "Spanish", "Swedish", "Tamil", "Thai", "Turkish", "Ukrainian", "Vietnamese"];

        [Reactive] public string TitleText { get; set; }
        [Reactive] public string MaxVolumeCount { get; set; }
        [Reactive] public string CurVolumeCount { get; set; }
        [Reactive] public string AdditionalLanguagesToolTipText { get; set; }
        [Reactive] public bool IsAddSeriesButtonEnabled { get; set; } = false;
        public ObservableCollection<ListBoxItem> SelectedAdditionalLanguages { get; set; } = new ObservableCollection<ListBoxItem>();
        private static readonly HttpClient AddCoverHttpClient = new HttpClient();
        public AddNewSeriesViewModel() => SelectedAdditionalLanguages.CollectionChanged += AdditionalLanguagesCollectionChanged;

        private void AdditionalLanguagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    if (!SelectedAdditionalLanguages.IsNullOrEmpty())
                    {
                        StringBuilder items = new StringBuilder();
                        for (int x = 0; x < SelectedAdditionalLanguages.Count - 1; x++)
                        {
                            items.AppendLine(SelectedAdditionalLanguages[x].Content.ToString());
                        }
                        items.Append(SelectedAdditionalLanguages.Last().Content.ToString());
                        AdditionalLanguagesToolTipText = items.ToString();
                    }
                    else
                    {
                        AdditionalLanguagesToolTipText = string.Empty;
                    }
                    break;
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
        public static async Task<bool> GetSeriesDataAsync(string title, string bookType, ushort curVolCount, ushort maxVolCount, ObservableCollection<string> additionalLanguages)
        {
            Series? newSeries = await Series.CreateNewSeriesCardAsync(title, bookType, maxVolCount, curVolCount,  additionalLanguages);
            
            bool duplicateSeriesCheck = true;
            if (newSeries != null)
            {
                duplicateSeriesCheck = false;
                foreach (Series series in MainWindowViewModel.UserCollection)
                {
                    if (!duplicateSeriesCheck && series.Link.Equals(newSeries.Link))
                    {
                        LOGGER.Debug($"{series.Link} | {newSeries.Link}");
                        duplicateSeriesCheck = true;
                    }
                }

                if (!duplicateSeriesCheck)
                {
                    // If the user is currently searching need to "refresh" the SearchedCollection so it can insert at correct index
                    if (MainWindowViewModel.SearchedCollection.Count != MainWindowViewModel.UserCollection.Count)
                    {
                        LOGGER.Info("Refreshing Searched Collection");
                        MainWindowViewModel.SortCollection();
                    }
                    LOGGER.Info($"\nAdding New Series -> {title} | {bookType} | {curVolCount} | {maxVolCount} |\n{newSeries.ToJsonString(options)}");
                    // File.WriteAllText(@"Series.json", newSeries.ToJsonString(options));

                    int index = MainWindowViewModel.UserCollection.BinarySearch(newSeries, new SeriesComparer(MainUser.CurLanguage));
                    index = index < 0 ? ~index : index;
                    newSeries.CoverBitMap = new Bitmap(newSeries.Cover).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);

                    MainWindowViewModel.UserCollection.Insert(index, newSeries);
                    MainWindowViewModel.SearchedCollection.Insert(index, newSeries);
                }
                else
                {
                    LOGGER.Info("Series Already Exists");
                }
            }
            else
            {
                LOGGER.Info($"{title} -> Does Not Exist");
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
            ObservableCollection<string> ConvertedList = [];
            foreach (ListBoxItem lang in SelectedLangs)
            {
                ConvertedList.Add(lang.Content.ToString());
            }
            return ConvertedList;
        }
    }
}
