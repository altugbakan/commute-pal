using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommutePal;

/// <summary>The sign-in prompt: four icons for today, closes as soon as one is clicked.</summary>
public partial class PopupWindow : Window
{
    private readonly CommuteLog _log;

    public PopupWindow(CommuteLog log)
    {
        InitializeComponent();
        _log = log;
    }

    private void ModeButton_Click(object sender, RoutedEventArgs e)
    {
        var mode = Enum.Parse<CommuteMode>((string)((Button)sender).Tag);

        try
        {
            _log.Set(DateOnly.FromDateTime(DateTime.Today), mode);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save:\n{ex.Message}\n\n{CommuteLog.Directory}",
                "CommutePal", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // No title bar, so the card itself is the drag handle.
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
