using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
namespace ARM9Editor.Services;

public static class DialogService
{
    public static async Task ShowErrorAsync(Window? owner, string message)
    {
        await ShowMessageAsync(owner, "Error", message);
    }

    public static async Task ShowMessageAsync(Window? owner, string title, string message)
    {
        if (owner == null)
        {
            return;
        }
        Window dialog = new()
        {
            Title = title,
            Width = 400,
            Height = 150,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.Height
        };
        StackPanel stack = new()
        {
            Margin = new Thickness(20)
        };
        stack.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });
        Button button = new()
        {
            Content = "OK",
            Width = 80,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        button.Click += (_, _) => dialog.Close(false);
        stack.Children.Add(button);
        dialog.Content = stack;
        await dialog.ShowDialog(owner);
    }

    public static async Task<bool?> ShowConfirmationAsync(Window? owner, string title, string message)
    {
        if (owner == null)
        {
            return null;
        }
        Window dialog = new()
        {
            Title = title,
            Width = 400,
            Height = 150,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.Height
        };
        StackPanel stack = new()
        {
            Margin = new Thickness(20)
        };
        stack.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        });
        StackPanel buttonPanel = new()
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Orientation = Orientation.Horizontal
        };
        Button okButton = new()
        {
            Content = "OK",
            Width = 80,
            Margin = new Thickness(0, 0, 10, 0)
        };
        okButton.Click += (_, _) => dialog.Close(true);
        Button cancelButton = new()
        {
            Content = "Cancel",
            Width = 80
        };
        cancelButton.Click += (_, _) => dialog.Close(false);
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stack.Children.Add(buttonPanel);
        dialog.Content = stack;
        return await dialog.ShowDialog<bool?>(owner);
    }
}