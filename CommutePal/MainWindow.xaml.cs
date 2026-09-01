using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CommutePal;

/// <summary>The full app window: log or change any day, and see this month's and last month's totals.</summary>
public partial class MainWindow : Window
{
    private readonly CommuteLog _log;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>The day the four buttons act on. Steerable for backfilling.</summary>
    private DateOnly _selectedDate;

    public MainWindow(CommuteLog log)
    {
        InitializeComponent();

        _log = log;
        _selectedDate = _today;

        InitializeStartupCheckBox();
        InitializePicker();
        Refresh();
    }

    private void ModeButton_Click(object sender, RoutedEventArgs e)
    {
        var mode = Enum.Parse<CommuteMode>((string)((Button)sender).Tag);

        try
        {
            _log.Set(_selectedDate, mode);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save:\n{ex.Message}\n\n{CommuteLog.Directory}",
                "CommutePal", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Refresh();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DatePopup.IsOpen)
        {
            DatePopup.IsOpen = false;
        }
    }

    private void Refresh()
    {
        var selectedMode = _log.Get(_selectedDate);
        foreach (var button in ModeGrid.Children.OfType<Button>())
        {
            var isSelected = selectedMode is not null && (string)button.Tag == selectedMode.ToString();
            button.Background = (Brush)FindResource(isSelected ? "SelectedBg" : "CardBg");
            button.BorderBrush = (Brush)FindResource(isSelected ? "Accent" : "CardBorder");
        }

        var culture = CultureInfo.CurrentCulture;
        var isToday = _selectedDate == _today;

        DateText.Text = _selectedDate.ToString("dddd, d MMMM", culture);
        TodayButton.Visibility = isToday ? Visibility.Collapsed : Visibility.Visible;
        NextDayButton.IsEnabled = !isToday;

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
}
