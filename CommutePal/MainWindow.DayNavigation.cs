using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommutePal;

/// <summary>Stepping between days and picking a date from the calendar, so past days can be backfilled.</summary>
public partial class MainWindow
{
    private bool _suppressCalendarEvent;

    private void InitializeCalendar()
    {
        DateCalendar.DisplayDateEnd = _today.ToDateTime(TimeOnly.MinValue);
        ((CalendarDayMarker)Resources["DayMarker"]).Log = _log;
    }

    private void PrevDay_Click(object sender, RoutedEventArgs e) => SelectDate(_selectedDate.AddDays(-1));

    private void NextDay_Click(object sender, RoutedEventArgs e) => SelectDate(_selectedDate.AddDays(1));

    private void Today_Click(object sender, RoutedEventArgs e) => SelectDate(_today);

    private void DateButton_Click(object sender, RoutedEventArgs e)
    {
        _suppressCalendarEvent = true;
        DateCalendar.DisplayDate = _selectedDate.ToDateTime(TimeOnly.MinValue);
        DateCalendar.SelectedDate = DateCalendar.DisplayDate;
        _suppressCalendarEvent = false;

        // Re-apply the cell style so the logged-day dots reflect anything saved since the popup last opened.
        var style = DateCalendar.CalendarDayButtonStyle;
        DateCalendar.CalendarDayButtonStyle = null;
        DateCalendar.CalendarDayButtonStyle = style;

        DatePopup.IsOpen = true;
    }

    private void DateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressCalendarEvent || DateCalendar.SelectedDate is not { } picked)
        {
            return;
        }

        DatePopup.IsOpen = false;
        Mouse.Capture(null); // WPF's Calendar keeps mouse capture after a click, which would eat the next click
        SelectDate(DateOnly.FromDateTime(picked));
    }

    private void SelectDate(DateOnly date)
    {
        if (date > _today)
        {
            return;
        }

        _selectedDate = date;
        Refresh();
    }
}
