using System.Windows;

namespace CommutePal;

/// <summary>Stepping between days and picking one from the month picker, so past days can be backfilled.</summary>
public partial class MainWindow
{
    private void InitializePicker()
    {
        Picker.MaxDate = _today;
        Picker.DayLookup = _log.Get;
    }

    private void PrevDay_Click(object sender, RoutedEventArgs e) => SelectDate(_selectedDate.AddDays(-1));

    private void NextDay_Click(object sender, RoutedEventArgs e) => SelectDate(_selectedDate.AddDays(1));

    private void Today_Click(object sender, RoutedEventArgs e) => SelectDate(_today);

    private void DateButton_Click(object sender, RoutedEventArgs e)
    {
        Picker.ShowDate(_selectedDate); // also redraws the markers for anything logged since it was last open
        DatePopup.IsOpen = true;
    }

    private void Picker_DateSelected(object? sender, DateOnly date)
    {
        DatePopup.IsOpen = false;
        SelectDate(date);
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
