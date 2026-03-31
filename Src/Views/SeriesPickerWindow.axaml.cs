using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Views;

public sealed partial class SeriesPickerWindow : ReactiveWindow<ViewModelBase>
{
    private readonly List<Series> _allSeries;
    private readonly TsundokuLanguage _language;

    public List<Series>? SelectedSeries { get; private set; }

    public SeriesPickerWindow(IReadOnlyList<Series> series, TsundokuLanguage language)
    {
        _language = language;
        _allSeries = [.. series];
        InitializeComponent();

        PopulateList(string.Empty);
        SelectAll();

        SearchBox.TextChanged += (_, _) => PopulateList(SearchBox.Text ?? string.Empty);
        SeriesList.SelectionChanged += (_, _) => UpdateCount();
    }

    private void PopulateList(string filter)
    {
        List<string> titles = [];
        foreach (Series s in _allSeries)
        {
            string title = s.Titles.TryGetValue(_language, out string? t) ? t : s.Titles[TsundokuLanguage.Romaji];
            if (string.IsNullOrWhiteSpace(filter) || title.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                titles.Add(title);
            }
        }
        SeriesList.ItemsSource = titles;
    }

    private void SelectAll()
    {
        SeriesList.SelectAll();
        UpdateCount();
    }

    private void UpdateCount()
    {
        int selected = SeriesList.SelectedItems?.Count ?? 0;
        int total = SeriesList.ItemCount;
        CountText.Text = $"{selected}/{total} selected";
    }

    private void OnSelectAll(object? sender, RoutedEventArgs e)
    {
        SelectAll();
    }

    private void OnDeselectAll(object? sender, RoutedEventArgs e)
    {
        SeriesList.UnselectAll();
        UpdateCount();
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        HashSet<string> selectedTitles = [];
        if (SeriesList.SelectedItems is not null)
        {
            foreach (object? item in SeriesList.SelectedItems)
            {
                if (item is string title)
                {
                    selectedTitles.Add(title);
                }
            }
        }

        SelectedSeries = [];
        foreach (Series s in _allSeries)
        {
            string title = s.Titles.TryGetValue(_language, out string? t) ? t : s.Titles[TsundokuLanguage.Romaji];
            if (selectedTitles.Contains(title))
            {
                SelectedSeries.Add(s);
            }
        }

        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        SelectedSeries = null;
        Close();
    }
}
