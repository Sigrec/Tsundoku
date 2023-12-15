using System.ComponentModel;
using ReactiveUI.Fody.Helpers;

namespace Tsundoku.Models
{
    public class TsundokuTheme : ICloneable, IComparable, IDisposable
    {
        [JsonIgnore] private bool disposedValue;
        public string ThemeName { get; set; }
        public string MenuBGColor { get; set; }
        public string UsernameColor { get; set; }
        public string MenuTextColor { get; set; }
        public string SearchBarBGColor { get; set; }
        public string SearchBarBorderColor { get; set; }
        public string SearchBarTextColor { get; set; }
        public string DividerColor { get; set; }
        public string MenuButtonBGColor { get; set; } 
        public string MenuButtonBGHoverColor { get; set; }
        public string MenuButtonBorderColor { get; set; }
        public string MenuButtonBorderHoverColor { get; set; }
        public string MenuButtonTextAndIconColor { get; set; }
        public string MenuButtonTextAndIconHoverColor { get; set; }
        public string CollectionBGColor { get; set; }
        public string StatusAndBookTypeBGColor { get; set; }
        public string StatusAndBookTypeBGHoverColor { get; set; }
        public string StatusAndBookTypeTextColor { get; set; }
        public string StatusAndBookTypeTextHoverColor { get; set; }
        public string SeriesCardBGColor { get; set; }
        public string SeriesCardTitleColor { get; set; }
        public string SeriesCardStaffColor { get; set; }
        public string SeriesCardDescColor { get; set; }
        public string SeriesProgressBGColor { get; set; }
        public string SeriesProgressBarColor { get; set; }
        public string SeriesProgressBarBGColor { get; set; }
        public string SeriesProgressBarBorderColor { get; set; }
        public string SeriesProgressTextColor { get; set; }
        public string SeriesProgressButtonsHoverColor { get; set; }
        public string SeriesButtonBGColor { get; set; }
        public string SeriesButtonBGHoverColor { get; set; }
        public string SeriesButtonIconColor { get; set; }
        public string SeriesButtonIconHoverColor { get; set; }
        public string SeriesEditPaneBGColor { get; set; }
        public string SeriesNotesBGColor  { get; set; }
        public string SeriesNotesBorderColor { get; set; }
        public string SeriesNotesTextColor { get; set; }
        public string SeriesEditPaneButtonsBGColor { get; set; }
        public string SeriesEditPaneButtonsBGHoverColor { get; set; }
        public string SeriesEditPaneButtonsBorderColor { get; set; }
        public string SeriesEditPaneButtonsBorderHoverColor { get; set; }
        public string SeriesEditPaneButtonsIconColor { get; set; }
        public string SeriesEditPaneButtonsIconHoverColor { get; set; }

        public static readonly TsundokuTheme DEFAULT_THEME = new TsundokuTheme(
            "Default", //ThemeName
            "#ff20232d",
            "#ffb4bddb",
            "#ffb4bddb",
            "#ff626460",
            "#ffdfd59e",
            "#ffb4bddb",
            "#ffdfd59e",
            "#ff626460",
            "#ff2c2d42",
            "#ffdfd59e",
            "#ffdfd59e",
            "#ffb4bddb",
            "#ff626460",
            "#ff2c2d42",
            "#ff626460",
            "#ffdfd59e",
            "#ffececec",
            "#ff626460",
            "#ff20232d",
            "#ffdfd59e",
            "#ffb4bddb",
            "#ffececec",
            "#ff626460",
            "#ffdfd59e",
            "#ff20232d",
            "#ffececec",
            "#ffececec",
            "#ff20232d",
            "#ff626460",
            "#ff626460",
            "#ffececec",
            "#ffdfd59e",
            "#ff20232d",
            "#ff626460",
            "#ffdfd59e",
            "#ffb4bddb",
            "#ff2c2d42",
            "#ff626460",
            "#ffdfd59e",
            "#ffdfd59e",
            "#ff626460",
            "#ffb4bddb"
        );

        public TsundokuTheme()
        {

        }

        public TsundokuTheme(string themeName)
        {
            ThemeName = themeName;
        }

        [JsonConstructor]
        public TsundokuTheme(string themeName, string menuBGColor, string usernameColor, string menuTextColor, string searchBarBGColor, string searchBarBorderColor, string searchBarTextColor, string dividerColor, string menuButtonBGColor, string menuButtonBGHoverColor, string menuButtonBorderColor, string menuButtonBorderHoverColor, string menuButtonTextAndIconColor, string menuButtonTextAndIconHoverColor, string collectionBGColor, string statusAndBookTypeBGColor, string statusAndBookTypeBGHoverColor, string statusAndBookTypeTextColor, string statusAndBookTypeTextHoverColor, string seriesCardBGColor, string seriesCardTitleColor, string seriesCardStaffColor, string seriesCardDescColor, string seriesProgressBGColor, string seriesProgressBarColor, string seriesProgressBarBGColor, string seriesProgressBarBorderColor, string seriesProgressTextColor, string seriesProgressButtonsHoverColor, string seriesButtonBGColor, string seriesButtonBGHoverColor, string seriesButtonIconColor, string seriesButtonIconHoverColor, string seriesEditPaneBGColor, string seriesNotesBGColor, string seriesNotesBorderColor, string seriesNotesTextColor, string seriesEditPaneButtonsBGColor, string seriesEditPaneButtonsBGHoverColor, string seriesEditPaneButtonsBorderColor, string seriesEditPaneButtonsBorderHoverColor, string seriesEditPaneButtonsIconColor, string seriesEditPaneButtonsIconHoverColor) : this(themeName)
        {
            MenuBGColor = menuBGColor;
            UsernameColor = usernameColor;
            MenuTextColor = menuTextColor;
            SearchBarBGColor = searchBarBGColor;
            SearchBarBorderColor = searchBarBorderColor;
            SearchBarTextColor = searchBarTextColor;
            DividerColor = dividerColor;
            MenuButtonBGColor = menuButtonBGColor;
            MenuButtonBGHoverColor = menuButtonBGHoverColor;
            MenuButtonBorderColor = menuButtonBorderColor;
            MenuButtonBorderHoverColor = menuButtonBorderHoverColor;
            MenuButtonTextAndIconColor = menuButtonTextAndIconColor;
            MenuButtonTextAndIconHoverColor = menuButtonTextAndIconHoverColor;
            CollectionBGColor = collectionBGColor;
            StatusAndBookTypeBGColor = statusAndBookTypeBGColor;
            StatusAndBookTypeBGHoverColor = statusAndBookTypeBGHoverColor;
            StatusAndBookTypeTextColor = statusAndBookTypeTextColor;
            StatusAndBookTypeTextHoverColor = statusAndBookTypeTextHoverColor;
            SeriesCardBGColor = seriesCardBGColor;
            SeriesCardTitleColor = seriesCardTitleColor;
            SeriesCardStaffColor = seriesCardStaffColor;
            SeriesCardDescColor = seriesCardDescColor;
            SeriesProgressBGColor = seriesProgressBGColor;
            SeriesProgressBarColor = seriesProgressBarColor;
            SeriesProgressBarBGColor = seriesProgressBarBGColor;
            SeriesProgressBarBorderColor = seriesProgressBarBorderColor;
            SeriesProgressTextColor = seriesProgressTextColor;
            SeriesProgressButtonsHoverColor = seriesProgressButtonsHoverColor;
            SeriesButtonBGColor = seriesButtonBGColor;
            SeriesButtonBGHoverColor = seriesButtonBGHoverColor;
            SeriesButtonIconColor = seriesButtonIconColor;
            SeriesButtonIconHoverColor = seriesButtonIconHoverColor;
            SeriesEditPaneBGColor = seriesEditPaneBGColor;
            SeriesNotesBGColor = seriesNotesBGColor;
            SeriesNotesBorderColor = seriesNotesBorderColor;
            SeriesNotesTextColor = seriesNotesTextColor;
            SeriesEditPaneButtonsBGColor = seriesEditPaneButtonsBGColor;
            SeriesEditPaneButtonsBGHoverColor = seriesEditPaneButtonsBGHoverColor;
            SeriesEditPaneButtonsBorderColor = seriesEditPaneButtonsBorderColor;
            SeriesEditPaneButtonsBorderHoverColor = seriesEditPaneButtonsBorderHoverColor;
            SeriesEditPaneButtonsIconColor = seriesEditPaneButtonsIconColor;
            SeriesEditPaneButtonsIconHoverColor = seriesEditPaneButtonsIconHoverColor;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(ThemeName);
            hash.Add(MenuBGColor);
            hash.Add(UsernameColor);
            hash.Add(MenuTextColor);
            hash.Add(SearchBarBGColor);
            hash.Add(SearchBarBorderColor);
            hash.Add(SearchBarTextColor);
            hash.Add(MenuButtonBGColor);
            hash.Add(MenuButtonBGHoverColor);
            hash.Add(MenuButtonBorderColor);
            hash.Add(MenuButtonBorderHoverColor);
            hash.Add(MenuButtonTextAndIconColor);
            hash.Add(MenuButtonTextAndIconHoverColor);
            hash.Add(DividerColor);
            hash.Add(CollectionBGColor);
            hash.Add(StatusAndBookTypeBGColor);
            hash.Add(StatusAndBookTypeBGHoverColor);
            hash.Add(StatusAndBookTypeTextColor);
            hash.Add(StatusAndBookTypeTextHoverColor);
            hash.Add(SeriesCardBGColor);
            hash.Add(SeriesCardTitleColor);
            hash.Add(SeriesCardStaffColor);
            hash.Add(SeriesCardDescColor);
            hash.Add(SeriesProgressBGColor);
            hash.Add(SeriesProgressBarColor);
            hash.Add(SeriesProgressBarBGColor);
            hash.Add(SeriesProgressBarBorderColor);
            hash.Add(SeriesProgressTextColor);
            hash.Add(SeriesProgressButtonsHoverColor);
            hash.Add(SeriesButtonBGColor);
            hash.Add(SeriesButtonBGHoverColor);
            hash.Add(SeriesButtonIconColor);
            hash.Add(SeriesButtonIconHoverColor);
            hash.Add(SeriesEditPaneBGColor);
            hash.Add(SeriesNotesBGColor);
            hash.Add(SeriesNotesBorderColor);
            hash.Add(SeriesNotesTextColor);
            hash.Add(SeriesEditPaneButtonsBGColor);
            hash.Add(SeriesEditPaneButtonsBGHoverColor);
            hash.Add(SeriesEditPaneButtonsBorderColor);
            hash.Add(SeriesEditPaneButtonsBorderHoverColor);
            hash.Add(SeriesEditPaneButtonsIconColor);
            hash.Add(SeriesEditPaneButtonsIconHoverColor);
            return hash.ToHashCode();
        }

        public int CompareTo(object? obj)
        {
            return this.ThemeName.CompareTo((obj as TsundokuTheme).ThemeName);
        }

        public virtual TsundokuTheme Cloning()
        {
            return new TsundokuTheme(ThemeName, MenuBGColor, UsernameColor, MenuTextColor,SearchBarBGColor,SearchBarBorderColor,SearchBarTextColor, DividerColor, MenuButtonBGColor, MenuButtonBGHoverColor, MenuButtonBorderColor, MenuButtonBorderHoverColor, MenuButtonTextAndIconColor, MenuButtonTextAndIconHoverColor, CollectionBGColor,StatusAndBookTypeBGColor,StatusAndBookTypeBGHoverColor,StatusAndBookTypeTextColor,StatusAndBookTypeTextHoverColor,SeriesCardBGColor,SeriesCardTitleColor,SeriesCardStaffColor,SeriesCardDescColor,SeriesProgressBGColor,SeriesProgressBarColor,SeriesProgressBarBGColor,SeriesProgressBarBorderColor,SeriesProgressTextColor,SeriesProgressButtonsHoverColor,SeriesButtonBGColor,SeriesButtonBGHoverColor,SeriesButtonIconColor,SeriesButtonIconHoverColor,SeriesEditPaneBGColor,SeriesNotesBGColor,SeriesNotesBorderColor,SeriesNotesTextColor,SeriesEditPaneButtonsBGColor,SeriesEditPaneButtonsBGHoverColor,SeriesEditPaneButtonsBorderColor,SeriesEditPaneButtonsBorderHoverColor,SeriesEditPaneButtonsIconColor,SeriesEditPaneButtonsIconHoverColor);
        }

        object ICloneable.Clone()
        {
            return Cloning();
        }

        protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
					
				}

				// free unmanaged resources (unmanaged objects) and override finalizer
				// set large fields to null
				disposedValue = true;
			}
		}

		// Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~Series()
		// {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
    }
}