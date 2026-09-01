using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CommutePal;

public partial class MainWindow : Window
{
    private readonly CommuteLog _log;
    private readonly bool _compact;
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>The day the four buttons act on. Always today in the popup; steerable in the full view for backfilling.</summary>
    private DateOnly _selectedDate;

    /// <param name="compact">Sign-in popup: four icons only, closes as soon as one is clicked.</param>
    public MainWindow(CommuteLog log, bool compact)
    {
        InitializeComponent();

        _log = log;
        _compact = compact;
        _selectedDate = _today;

        if (compact)
        {
            ConfigureCompactPopup();
        }
        else
        {
            InitializeStartupCheckBox();
            DateCalendar.DisplayDateEnd = _today.ToDateTime(TimeOnly.MinValue);
        }

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

        if (_compact)
        {
            Close();
            return;
        }

        Refresh();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        if (DatePopup.IsOpen)
        {
            DatePopup.IsOpen = false;
        }
        else if (_compact)
        {
            Close();
        }
    }

    private void Refresh()
    {
        var selectedMode = _log.Get(_selectedDate);
        var buttons = ModeGrid.Children.OfType<Button>().Concat(CompactGrid.Children.OfType<Button>());
        foreach (var button in buttons)
        {
            var isSelected = selectedMode is not null && (string)button.Tag == selectedMode.ToString();
            button.Background = (Brush)FindResource(isSelected ? "SelectedBg" : "CardBg");
            button.BorderBrush = (Brush)FindResource(isSelected ? "Accent" : "CardBorder");
        }

        if (_compact)
        {
            return;
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
