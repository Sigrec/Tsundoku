using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Tsundoku.Helpers;

/// <summary>
/// Shows a blocking dialog describing an unexpected error, so the user sees what went wrong
/// without opening the log file. Built in code so it has no dependency on services that may
/// not have been created yet.
/// </summary>
public static class ErrorDialog
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public static async Task ShowAsync(Window owner, string title, string summary, Exception exception)
    {
        try
        {
            string logsFolder = AppFileHelper.GetFolderPath("Logs");
            string details = $"Tsundoku v{ViewModels.ViewModelBase.CUR_TSUNDOKU_VERSION}{Environment.NewLine}{summary}{Environment.NewLine}{Environment.NewLine}{exception}";

            Window dialog = new()
            {
                Title = title,
                Width = 640,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Topmost = true,
                Icon = owner.Icon
            };

            StackPanel content = new()
            {
                Margin = new Thickness(20),
                Spacing = 12
            };

            content.Children.Add(new TextBlock
            {
                Text = summary,
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = FindBrush("TsundokuUsernameColor", Brushes.White)
            });

            content.Children.Add(new TextBlock
            {
                Text = $"{exception.GetType().Name}: {exception.Message}",
                FontSize = 13,
                FontWeight = FontWeight.Bold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = FindBrush("TsundokuMenuTextColor", Brushes.White)
            });

            content.Children.Add(new ScrollViewer
            {
                MaxHeight = 220,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = new SelectableTextBlock
                {
                    Text = exception.ToString(),
                    FontSize = 11,
                    FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                    Foreground = FindBrush("TsundokuMenuTextColor", Brushes.White)
                }
            });

            content.Children.Add(new TextBlock
            {
                Text = $"Full logs: {logsFolder}",
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Foreground = FindBrush("TsundokuMenuTextColor", Brushes.White)
            });

            ControlTheme? buttonTheme = null;
            if (Application.Current is not null &&
                Application.Current.Resources.TryGetResource("MenuButton", null, out object? themeObj))
            {
                buttonTheme = themeObj as ControlTheme;
            }

            Button copyButton = CreateButton("Copy Details", buttonTheme);
            Button logsButton = CreateButton("Open Logs Folder", buttonTheme);
            Button closeButton = CreateButton("Close", buttonTheme);

            copyButton.Click += async (s, e) => await ClipboardHelper.CopyToClipboardAsync(details);
            logsButton.Click += (s, e) => OpenFolder(logsFolder);
            closeButton.Click += (s, e) => dialog.Close();

            content.Children.Add(new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Children = { copyButton, logsButton, closeButton }
            });

            dialog.Content = new Border
            {
                BorderThickness = new Thickness(2),
                Background = FindBrush("TsundokuMenuBGColor", Brushes.Black),
                BorderBrush = FindBrush("TsundokuDividerColor", Brushes.Gray),
                Child = content
            };

            await dialog.ShowDialog(owner);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to show the error dialog");
        }
    }

    private static IBrush FindBrush(string key, IBrush fallback) =>
        Application.Current?.FindResource(key) as IBrush ?? fallback;

    private static Button CreateButton(string text, ControlTheme? theme) => new()
    {
        Content = text,
        FontSize = 13,
        FontWeight = FontWeight.Bold,
        Padding = new Thickness(16, 6),
        Cursor = new Cursor(StandardCursorType.Hand),
        Theme = theme
    };

    private static void OpenFolder(string folder)
    {
        try
        {
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LOGGER.Warn(ex, "Failed to open folder {Folder}", folder);
        }
    }
}
