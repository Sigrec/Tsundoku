﻿using System.Threading.Tasks;
using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using Avalonia.Media.Imaging;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using ReactiveUI;
using System;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        public static readonly string[] AvailableLanguages = new string[] { "Arabic", "Azerbaijan", "Bengali", "Bulgarian", "Burmese", "Catalan", "Chinese", "Croatian", "Czech", "Danish", "Dutch", "Esperanto", "Estonian", "Filipino", "Finnish", "French", "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian", "Italian", "Kazakh", "Korean", "Latin", "Lithuanian", "Malay", "Mongolian", "Nepali", "Norwegian", "Persian", "Polish", "Portuguese", "Romanian", "Russian", "Serbian", "Slovak", "Spanish", "Swedish", "Tamil", "Thai", "Turkish", "Ukrainian", "Vietnamese" };

        [Reactive] public string TitleText { get; set; }
        [Reactive] public string MaxVolumeCount { get; set; }
        [Reactive] public string CurVolumeCount { get; set; }
        [Reactive] public bool IsAddSeriesButtonEnabled { get; set; } = false;

        public ObservableCollection<ListBoxItem> SelectedAdditionalLanguages { get; } = new ObservableCollection<ListBoxItem>();

        static readonly Helpers.AniListQuery ALQuery = new();
        static readonly Helpers.MangadexQuery MDQuery = new();

        public AddNewSeriesViewModel()
        {

        }
        
        /**
        * Last Modified: 08 December 2022
        *  by: Sean Njenga
        * Desc: Recieves the call from the AddNewSeriesWindow Button "Add" press which sends the data to this method which calls for
        *       the method to get the series data and create a new series object. Then it checks to see if it's a valid series and
        *       whether the series already exists in the users collection if not then it adds.
        * Params:
        *      title    | string      | The titles of the new series to add
        *      bookType | string      | The book type or format of the series to add either a Manga(Comic) or Novel
        *      ushort   | curVolCount | The current # of volumes the user has collected for this series
        *      ushort   | maxVolCount | The max # of volumes this series currently has
        */
        public async Task<bool> GetSeriesData(string title, string bookType, ushort curVolCount, ushort maxVolCount, ObservableCollection<string> additionalLanguages)
        {
            Series? newSeries = await Series.CreateNewSeriesCard(title, bookType, maxVolCount, curVolCount, ALQuery, MDQuery, additionalLanguages);
            
            bool duplicateSeriesCheck = true;
            if (newSeries != null)
            {
                duplicateSeriesCheck = false;
                foreach (Series series in MainWindowViewModel.Collection)
                {
                    if (!duplicateSeriesCheck && series.Link.Equals(newSeries.Link))
                    {
                        Constants.Logger.Debug($"{series.Link} | {newSeries.Link}");
                        duplicateSeriesCheck = true;
                    }
                }

                if (!duplicateSeriesCheck)
                {
                    // If the user is currently searching need to "refresh" the SearchedCollection so it can insert at correct index
                    if (MainWindowViewModel.SearchedCollection.Count != MainWindowViewModel.Collection.Count)
                    {
                        Constants.Logger.Info("Refreshing Searched Collection");
                        MainWindowViewModel.SortCollection();
                    }
                    Constants.Logger.Info($"\nAdding New Series -> {title} | {bookType} | {curVolCount} | {maxVolCount} |\n{newSeries.ToJsonString(options)}");
                    // File.WriteAllText(@"Series.json", newSeries.ToJsonString(options));

                    int index = MainWindowViewModel.Collection.BinarySearch(newSeries, new SeriesComparer(MainUser.CurLanguage));
                    index = index < 0 ? ~index : index;
                    newSeries.CoverBitMap = new Bitmap(newSeries.Cover).CreateScaledBitmap(new Avalonia.PixelSize(Constants.LEFT_SIDE_CARD_WIDTH, Constants.IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);

                    MainWindowViewModel.Collection.Insert(index, newSeries);
                    MainWindowViewModel.SearchedCollection.Insert(index, newSeries);
                }
                else
                {
                    Constants.Logger.Info("Series Already Exists");
                }
            }
            else
            {
                Constants.Logger.Info($"{title} -> Does Not Exist");
            }
            return duplicateSeriesCheck;
        }

        public static ObservableCollection<string> ConvertSelectedLangList(ObservableCollection<ListBoxItem> SelectedLangs)
        {
            ObservableCollection<string> ConvertedList = new();
            foreach (ListBoxItem lang in SelectedLangs)
            {
                ConvertedList.Add(lang.Content.ToString());
            }
            return ConvertedList;
        }
    }
}
