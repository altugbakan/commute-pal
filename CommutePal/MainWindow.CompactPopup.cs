using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CommutePal;

/// <summary>Window chrome for the sign-in popup: borderless rounded card, draggable, with its own close button.</summary>
public partial class MainWindow
{
    private void ConfigureCompactPopup()
    {
        FullPanel.Visibility = Visibility.Collapsed;
        CompactPanel.Visibility = Visibility.Visible;

        // Transparent borderless window so the rounded card and its shadow are the whole UI.
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        Topmost = true; // make sure the sign-in prompt is actually seen
        ShowInTaskbar = false;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void CompactPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
