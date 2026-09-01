using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CommutePal;

public partial class MainWindow : Window
{
    private static readonly Brush SelectedBrush = new SolidColorBrush(Color.FromRgb(0xDD, 0xEE, 0xFF));
    private static readonly Brush SelectedBorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
    private static readonly Brush DefaultBrush = Brushes.White;
    private static readonly Brush DefaultBorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0));

    private readonly CommuteLog _log;
    private readonly bool _compact;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Today);
    private bool _suppressStartupToggle;

    /// <param name="compact">Sign-in popup: four icons only, closes as soon as one is clicked.</param>
    public MainWindow(CommuteLog log, bool compact)
    {
        InitializeComponent();

        _log = log;
        _compact = compact;

        if (compact)
        {
            FullPanel.Visibility = Visibility.Collapsed;
            CompactPanel.Visibility = Visibility.Visible;

            // Borderless transparent window so the rounded card and its shadow are the whole UI.
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            Topmost = true; // make sure the sign-in prompt is actually seen
            ShowInTaskbar = false;
        }
        else
        {
            _suppressStartupToggle = true;
            StartupCheckBox.IsChecked = StartupRegistration.IsEnabled();
            _suppressStartupToggle = false;
        }

        Refresh();
    }

    private void ModeButton_Click(object sender, RoutedEventArgs e)
    {
        var mode = Enum.Parse<CommuteMode>((string)((Button)sender).Tag);

        try
        {
            _log.Set(_today, mode);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save:\n{ex.Message}\n\n{CommuteLog.Directory}",
                "CommutePal", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_compact)
        {
            Close();
            return;
        }

        Refresh();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void CompactPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // No title bar in the popup, so let the card itself be dragged.
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // The compact popup has no title bar, so Escape is the way to dismiss it without logging.
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void Refresh()
    {
        var todayMode = _log.Get(_today);
        var buttons = ModeGrid.Children.OfType<Button>().Concat(CompactGrid.Children.OfType<Button>());
        foreach (var button in buttons)
        {
            var isSelected = todayMode is not null && (string)button.Tag == todayMode.ToString();
            button.Background = isSelected ? SelectedBrush : DefaultBrush;
            button.BorderBrush = isSelected ? SelectedBorderBrush : DefaultBorderBrush;
        }

        if (_compact)
        {
            return;
        }

        var culture = CultureInfo.CurrentCulture;
        DateText.Text = _today.ToString("dddd, d MMMM", culture);

        var thisMonth = new DateTime(_today.Year, _today.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        ThisMonthHeader.Text = thisMonth.ToString("MMM yyyy", culture);
        LastMonthHeader.Text = lastMonth.ToString("MMM yyyy", culture);

        var current = _log.StatsFor(thisMonth.Year, thisMonth.Month);
        var previous = _log.StatsFor(lastMonth.Year, lastMonth.Month);

        BikeThis.Text = current.Bike.ToString();
        CarThis.Text = current.Car.ToString();
        PtThis.Text = current.PublicTransport.ToString();
        HomeThis.Text = current.Home.ToString();
        TotalThis.Text = current.Total.ToString();

        BikeLast.Text = previous.Bike.ToString();
        CarLast.Text = previous.Car.ToString();
        PtLast.Text = previous.PublicTransport.ToString();
        HomeLast.Text = previous.Home.ToString();
        TotalLast.Text = previous.Total.ToString();
    }

    private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_suppressStartupToggle)
        {
            return;
        }

        try
        {
            if (StartupCheckBox.IsChecked == true)
            {
                StartupRegistration.Enable();
            }
            else
            {
                StartupRegistration.Disable();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not update the sign-in setting:\n{ex.Message}",
                "CommutePal", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenLog_Click(object sender, RoutedEventArgs e)
    {
        System.IO.Directory.CreateDirectory(CommuteLog.Directory);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{CommuteLog.Directory}\"") { UseShellExecute = true });
    }
}
