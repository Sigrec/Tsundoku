using System.Collections.Frozen;
using Avalonia.Media;

namespace Tsundoku.Models;

public static class ThemeResourceKeys
{
    public const string MenuBGColor = "TsundokuMenuBGColor";
    public const string UsernameColor = "TsundokuUsernameColor";
    public const string UserIconBorderColor = "TsundokuUserIconBorderColor";
    public const string MenuTextColor = "TsundokuMenuTextColor";
    public const string SearchBarBGColor = "TsundokuSearchBarBGColor";
    public const string SearchBarBorderColor = "TsundokuSearchBarBorderColor";
    public const string SearchBarTextColor = "TsundokuSearchBarTextColor";
    public const string DividerColor = "TsundokuDividerColor";
    public const string MenuButtonBGColor = "TsundokuMenuButtonBGColor";
    public const string MenuButtonBGHoverColor = "TsundokuMenuButtonBGHoverColor";
    public const string MenuButtonBorderColor = "TsundokuMenuButtonBorderColor";
    public const string MenuButtonBorderHoverColor = "TsundokuMenuButtonBorderHoverColor";
    public const string MenuButtonTextAndIconColor = "TsundokuMenuButtonTextAndIconColor";
    public const string MenuButtonTextAndIconHoverColor = "TsundokuMenuButtonTextAndIconHoverColor";
    public const string CollectionBGColor = "TsundokuCollectionBGColor";
    public const string StatusAndBookTypeBGColor = "TsundokuStatusAndBookTypeBGColor";
    public const string StatusAndBookTypeBGHoverColor = "TsundokuStatusAndBookTypeBGHoverColor";
    public const string StatusAndBookTypeTextColor = "TsundokuStatusAndBookTypeTextColor";
    public const string StatusAndBookTypeTextHoverColor = "TsundokuStatusAndBookTypeTextHoverColor";
    public const string SeriesCardBGColor = "TsundokuSeriesCardBGColor";
    public const string SeriesCardTitleColor = "TsundokuSeriesCardTitleColor";
    public const string SeriesCardPublisherColor = "TsundokuSeriesCardPublisherColor";
    public const string SeriesCardStaffColor = "TsundokuSeriesCardStaffColor";
    public const string SeriesCardDescColor = "TsundokuSeriesCardDescColor";
    public const string SeriesProgressBGColor = "TsundokuSeriesProgressBGColor";
    public const string SeriesProgressBarColor = "TsundokuSeriesProgressBarColor";
    public const string SeriesProgressBarBGColor = "TsundokuSeriesProgressBarBGColor";
    public const string SeriesProgressBarBorderColor = "TsundokuSeriesProgressBarBorderColor";
    public const string SeriesProgressTextColor = "TsundokuSeriesProgressTextColor";
    public const string SeriesProgressButtonsHoverColor = "TsundokuSeriesProgressButtonsHoverColor";
    public const string SeriesButtonIconColor = "TsundokuSeriesButtonIconColor";
    public const string SeriesButtonIconHoverColor = "TsundokuSeriesButtonIconHoverColor";
    public const string SeriesEditPaneBGColor = "TsundokuSeriesEditPaneBGColor";
    public const string SeriesNotesBGColor = "TsundokuSeriesNotesBGColor";
    public const string SeriesNotesBorderColor = "TsundokuSeriesNotesBorderColor";
    public const string SeriesNotesTextColor = "TsundokuSeriesNotesTextColor";
    public const string SeriesEditPaneButtonsBGColor = "TsundokuSeriesEditPaneButtonsBGColor";
    public const string SeriesEditPaneButtonsBGHoverColor = "TsundokuSeriesEditPaneButtonsBGHoverColor";
    public const string SeriesEditPaneButtonsBorderColor = "TsundokuSeriesEditPaneButtonsBorderColor";
    public const string SeriesEditPaneButtonsBorderHoverColor = "TsundokuSeriesEditPaneButtonsBorderHoverColor";
    public const string SeriesEditPaneButtonsIconColor = "TsundokuSeriesEditPaneButtonsIconColor";
    public const string SeriesEditPaneButtonsIconHoverColor = "TsundokuSeriesEditPaneButtonsIconHoverColor";

    public static readonly FrozenDictionary<string, Func<TsundokuTheme, SolidColorBrush>> PropertyMap =
        new Dictionary<string, Func<TsundokuTheme, SolidColorBrush>>
        {
            [MenuBGColor] = t => t.MenuBGColor,
            [UsernameColor] = t => t.UsernameColor,
            [UserIconBorderColor] = t => t.UserIconBorderColor,
            [MenuTextColor] = t => t.MenuTextColor,
            [SearchBarBGColor] = t => t.SearchBarBGColor,
            [SearchBarBorderColor] = t => t.SearchBarBorderColor,
            [SearchBarTextColor] = t => t.SearchBarTextColor,
            [DividerColor] = t => t.DividerColor,
            [MenuButtonBGColor] = t => t.MenuButtonBGColor,
            [MenuButtonBGHoverColor] = t => t.MenuButtonBGHoverColor,
            [MenuButtonBorderColor] = t => t.MenuButtonBorderColor,
            [MenuButtonBorderHoverColor] = t => t.MenuButtonBorderHoverColor,
            [MenuButtonTextAndIconColor] = t => t.MenuButtonTextAndIconColor,
            [MenuButtonTextAndIconHoverColor] = t => t.MenuButtonTextAndIconHoverColor,
            [CollectionBGColor] = t => t.CollectionBGColor,
            [StatusAndBookTypeBGColor] = t => t.StatusAndBookTypeBGColor,
            [StatusAndBookTypeBGHoverColor] = t => t.StatusAndBookTypeBGHoverColor,
            [StatusAndBookTypeTextColor] = t => t.StatusAndBookTypeTextColor,
            [StatusAndBookTypeTextHoverColor] = t => t.StatusAndBookTypeTextHoverColor,
            [SeriesCardBGColor] = t => t.SeriesCardBGColor,
            [SeriesCardTitleColor] = t => t.SeriesCardTitleColor,
            [SeriesCardPublisherColor] = t => t.SeriesCardPublisherColor,
            [SeriesCardStaffColor] = t => t.SeriesCardStaffColor,
            [SeriesCardDescColor] = t => t.SeriesCardDescColor,
            [SeriesProgressBGColor] = t => t.SeriesProgressBGColor,
            [SeriesProgressBarColor] = t => t.SeriesProgressBarColor,
            [SeriesProgressBarBGColor] = t => t.SeriesProgressBarBGColor,
            [SeriesProgressBarBorderColor] = t => t.SeriesProgressBarBorderColor,
            [SeriesProgressTextColor] = t => t.SeriesProgressTextColor,
            [SeriesProgressButtonsHoverColor] = t => t.SeriesProgressButtonsHoverColor,
            [SeriesButtonIconColor] = t => t.SeriesButtonIconColor,
            [SeriesButtonIconHoverColor] = t => t.SeriesButtonIconHoverColor,
            [SeriesEditPaneBGColor] = t => t.SeriesEditPaneBGColor,
            [SeriesNotesBGColor] = t => t.SeriesNotesBGColor,
            [SeriesNotesBorderColor] = t => t.SeriesNotesBorderColor,
            [SeriesNotesTextColor] = t => t.SeriesNotesTextColor,
            [SeriesEditPaneButtonsBGColor] = t => t.SeriesEditPaneButtonsBGColor,
            [SeriesEditPaneButtonsBGHoverColor] = t => t.SeriesEditPaneButtonsBGHoverColor,
            [SeriesEditPaneButtonsBorderColor] = t => t.SeriesEditPaneButtonsBorderColor,
            [SeriesEditPaneButtonsBorderHoverColor] = t => t.SeriesEditPaneButtonsBorderHoverColor,
            [SeriesEditPaneButtonsIconColor] = t => t.SeriesEditPaneButtonsIconColor,
            [SeriesEditPaneButtonsIconHoverColor] = t => t.SeriesEditPaneButtonsIconHoverColor,
        }.ToFrozenDictionary();
}
