using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommutePal;

/// <summary>Stepping between days and picking a date from the calendar, so past days can be backfilled.</summary>
public partial class MainWindow
{
    private bool _suppressCalendarEvent;

    private void PrevDay_Click(object sender, RoutedEventArgs e) => SelectDate(_selectedDate.AddDays(-1));

    private void NextDay_Click(object sender, RoutedEventArgs e) => SelectDate(_selectedDate.AddDays(1));

    private void Today_Click(object sender, RoutedEventArgs e) => SelectDate(_today);

    private void DateButton_Click(object sender, RoutedEventArgs e)
    {
        _suppressCalendarEvent = true;
        DateCalendar.DisplayDate = _selectedDate.ToDateTime(TimeOnly.MinValue);
        DateCalendar.SelectedDate = DateCalendar.DisplayDate;
        _suppressCalendarEvent = false;

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
