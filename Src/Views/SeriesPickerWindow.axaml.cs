using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using Tsundoku.Helpers;
using Tsundoku.Models;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Views;

public sealed partial class SeriesPickerWindow : ReactiveWindow<ViewModelBase>
{
    private readonly List<SeriesPickerItem> _allItems;
    private readonly HashSet<Guid> _selectedIds = [];
    private bool _isRepopulating;

    public List<Series>? SelectedSeries { get; private set; }

    public SeriesPickerWindow(IReadOnlyList<Series> series, TsundokuLanguage language)
    {
        _allItems = BuildItems(series, language);
        InitializeComponent();

        foreach (SeriesPickerItem item in _allItems)
        {
            _selectedIds.Add(item.Series.Id);
        }
        PopulateList(string.Empty);

        SearchBox.TextChanged += (_, _) => PopulateList(SearchBox.Text ?? string.Empty);
        SeriesList.SelectionChanged += OnSelectionChanged;
    }

    /// <summary>
    /// Builds one row per series. Titles shared by more than one series get the format appended
    /// so the rows can be told apart.
    /// </summary>
    private static List<SeriesPickerItem> BuildItems(IReadOnlyList<Series> series, TsundokuLanguage language)
    {
        Dictionary<string, int> titleCounts = [with(StringComparer.OrdinalIgnoreCase)];
        List<(Series Series, string Title)> titled = [with(series.Count)];
        foreach (Series s in series)
        {
            string title = s.Titles.TryGetValue(language, out string? t) ? t : s.Titles[TsundokuLanguage.Romaji];
            titled.Add((s, title));
            titleCounts[title] = titleCounts.GetValueOrDefault(title) + 1;
        }

        List<SeriesPickerItem> items = [with(titled.Count)];
        foreach ((Series s, string title) in titled)
        {
            string display = titleCounts[title] > 1 ? $"{title} ({s.Format})" : title;
            items.Add(new SeriesPickerItem(s, display));
        }
        return items;
    }

    private void PopulateList(string filter)
    {
        _isRepopulating = true;
        try
        {
            List<SeriesPickerItem> visible = [];
            foreach (SeriesPickerItem item in _allItems)
            {
                if (string.IsNullOrWhiteSpace(filter) || item.DisplayText.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    visible.Add(item);
                }
            }
            SeriesList.ItemsSource = visible;

            foreach (SeriesPickerItem item in visible)
            {
                if (_selectedIds.Contains(item.Series.Id))
                {
                    SeriesList.SelectedItems?.Add(item);
                }
            }
        }
        finally
        {
            _isRepopulating = false;
        }
        UpdateCount();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isRepopulating) return;

        foreach (object? added in e.AddedItems)
        {
            if (added is SeriesPickerItem item) _selectedIds.Add(item.Series.Id);
        }
        foreach (object? removed in e.RemovedItems)
        {
            if (removed is SeriesPickerItem item) _selectedIds.Remove(item.Series.Id);
        }
        UpdateCount();
    }

    private void UpdateCount() => CountText.Text = $"{_selectedIds.Count}/{_allItems.Count} selected";

    /// <summary>Selects every row the current search shows.</summary>
    private void OnSelectAll(object? sender, RoutedEventArgs e)
    {
        foreach (SeriesPickerItem item in VisibleItems())
        {
            _selectedIds.Add(item.Series.Id);
        }
        PopulateList(SearchBox.Text ?? string.Empty);
    }

    /// <summary>Deselects every row the current search shows.</summary>
    private void OnDeselectAll(object? sender, RoutedEventArgs e)
    {
        foreach (SeriesPickerItem item in VisibleItems())
        {
            _selectedIds.Remove(item.Series.Id);
        }
        _isRepopulating = true;
        try
        {
            SeriesList.UnselectAll();
        }
        finally
        {
            _isRepopulating = false;
        }
        UpdateCount();
    }

    private IEnumerable<SeriesPickerItem> VisibleItems() =>
        SeriesList.ItemsSource as IEnumerable<SeriesPickerItem> ?? [];

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        SelectedSeries = [];
        foreach (SeriesPickerItem item in _allItems)
        {
            if (_selectedIds.Contains(item.Series.Id))
            {
                SelectedSeries.Add(item.Series);
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
