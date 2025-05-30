﻿using Tsundoku.Models;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Tsundoku.Helpers;
using static Tsundoku.Models.TsundokuLanguageModel;
using Avalonia.Collections;

namespace Tsundoku.ViewModels
{
    public class AddNewSeriesViewModel : ViewModelBase
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private readonly BitmapHelper _bitmapHelper;
        private readonly MangaDex _mangaDex;
        private readonly AniList _aniList;
        [Reactive] public string TitleText { get; set; }
        [Reactive] public string PublisherText { get; set; }
        [Reactive] public string CoverImageUrl { get; set; }
        [Reactive] public string MaxVolumeCount { get; set; }
        [Reactive] public string CurVolumeCount { get; set; }
        [Reactive] public bool AllowDuplicate { get; set; }
        [Reactive] public string AdditionalLanguagesToolTipText { get; set; }
        [Reactive] public bool IsAddSeriesButtonEnabled { get; set; } = false;
        public AvaloniaList<ListBoxItem> SelectedAdditionalLanguages { get; set; } = [];
        private static readonly StringBuilder CurLanguages = new StringBuilder();

        public AddNewSeriesViewModel(IUserService userService, BitmapHelper bitmapHelper, MangaDex mangaDex, AniList aniList) : base(userService)
        {
            _bitmapHelper = bitmapHelper;
            _mangaDex = mangaDex;
            _aniList = aniList;

            SelectedAdditionalLanguages.CollectionChanged += AdditionalLanguagesCollectionChanged;
        }

        // TODO - Additional Languages selected should persist across app startups? I can traverse the list and and check what additional langs are used or store it in json
        // TODO - When user is looking for a additional lang but MangaDex doesn't have it show a popup that lets them enter the title in?
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
        public async Task<KeyValuePair<bool, string>> GetSeriesDataAsync(string title, Format bookType, ushort curVolCount, ushort maxVolCount, List<TsundokuLanguage> additionalLanguages, string customImageUrl = "", string publisher = "Unknown", Demographic demographic = Demographic.Unknown, uint volumesRead = 0, decimal rating = -1, decimal value = 0, bool allowDuplicate = false)
        {
            string returnMsg = string.Empty;
            Series? newSeries = await Series.CreateNewSeriesCardAsync(
                bitmapHelper: _bitmapHelper,
                mangaDex: _mangaDex,
                aniList: _aniList,
                title: title,
                bookType: bookType,
                maxVolCount: maxVolCount,
                minVolCount: curVolCount, // Assuming curVolCount maps to minVolCount
                additionalLanguages: additionalLanguages,
                publisher: publisher,
                demographic: demographic,
                volumesRead: volumesRead,
                rating: rating,
                value: value,
                customImageUrl: customImageUrl,
                allowDuplicate: allowDuplicate
            );

            bool successfulAdd = false;
            if (newSeries != null)
            {
                try
                {
                    LOGGER.Debug($"\nAdding New Series (Allow Dupe ?= {allowDuplicate}) -> \"{title}\" | \"{bookType}\" | {curVolCount} | {maxVolCount}\n{newSeries}");
                    successfulAdd = _userService.AddSeries(newSeries, allowDuplicate);
                    if (!successfulAdd)
                    {
                        returnMsg = "Duplicate Series";
                    }
                }
                catch (Exception ex)
                {
                    LOGGER.Error("Error adding new Series to Collection\n{msg}", ex.Message);
                    newSeries?.Dispose();
                    returnMsg = "Unknown Reason";
                }
            }
            return new KeyValuePair<bool, string>(successfulAdd, returnMsg);
        }

        public List<TsundokuLanguage> ConvertSelectedLangList()
        {
            return [.. SelectedAdditionalLanguages.Select(lang => lang.Content.ToString().GetEnumValueFromMemberValue<TsundokuLanguage>())];
        }
    }
}
