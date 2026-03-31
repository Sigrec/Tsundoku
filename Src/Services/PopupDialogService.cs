using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Projektanker.Icons.Avalonia;
using Tsundoku.ViewModels;
using Tsundoku.Views;

namespace Tsundoku.Services;

/// <summary>
/// Displays popup dialogs to the user using the <see cref="PopupDialog"/> window.
/// </summary>
public sealed class PopupDialogService(PopupDialogViewModel viewModel) : IPopupDialogService
{
    private readonly PopupDialogViewModel _viewModel = viewModel;

    /// <inheritdoc />
    public async Task ShowAsync(
        string title,
        string icon,
        string info,
        Window owner)
    {
        _viewModel.SetPopupInfo(title, icon, info);
        PopupDialog dialog = new PopupDialog
        {
            ViewModel = _viewModel
        };
        GlassmorphismService.ApplyToWindow(dialog, GlassmorphismService.IsEnabled);

        try
        {
            await dialog.ShowDialog(owner);
        }
        finally
        {
            dialog.Close();
        }
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(string title, string icon, string info, Window owner)
    {
        bool result = false;

        Window confirmDialog = new Window
        {
            Title = title,
            Width = 350,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            ShowInTaskbar = false,
            Topmost = true,
            Icon = owner.Icon
        };

        Border border = new Border
        {
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(0, 0, 8, 8),
            ClipToBounds = true,
            Background = (IBrush)Application.Current!.FindResource("TsundokuMenuBGColor")!,
            BorderBrush = (IBrush)Application.Current.FindResource("TsundokuDividerColor")!
        };

        StackPanel content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 12
        };

        Icon iconControl = new Icon
        {
            Value = icon,
            FontSize = 25,
            FontWeight = FontWeight.Bold,
            Foreground = (IBrush)Application.Current!.FindResource("TsundokuDividerColor")!
        };

        TextBlock messageText = new TextBlock
        {
            Text = info,
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (IBrush)Application.Current.FindResource("TsundokuMenuTextColor")!
        };

        StackPanel buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 10
        };

        ControlTheme? menuButtonTheme = Application.Current.FindResource("MenuButton") as ControlTheme;

        Button confirmBtn = new Button
        {
            Content = "Confirm",
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(20, 6),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Theme = menuButtonTheme
        };

        Button cancelBtn = new Button
        {
            Content = "Cancel",
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(20, 6),
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Theme = menuButtonTheme
        };

        confirmBtn.Click += (s, e) => { result = true; confirmDialog.Close(); };
        cancelBtn.Click += (s, e) => { result = false; confirmDialog.Close(); };

        buttonPanel.Children.Add(confirmBtn);
        buttonPanel.Children.Add(cancelBtn);

        content.Children.Add(iconControl);
        content.Children.Add(messageText);
        content.Children.Add(buttonPanel);
        border.Child = content;
        confirmDialog.Content = border;
        GlassmorphismService.ApplyToWindow(confirmDialog, GlassmorphismService.IsEnabled);

        await confirmDialog.ShowDialog(owner);
        return result;
    }
}