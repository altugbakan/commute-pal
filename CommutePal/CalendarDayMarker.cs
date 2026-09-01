using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CommutePal;

/// <summary>
/// Turns a calendar cell's date into the dot colour / tooltip that shows whether that day is logged.
/// Office days (bike, car, public transport) use the accent colour; home days a muted one.
/// </summary>
public sealed class CalendarDayMarker : IValueConverter
{
    public CommuteLog? Log { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime || Log?.Get(DateOnly.FromDateTime(dateTime)) is not { } mode)
        {
            return targetType == typeof(Brush) ? Brushes.Transparent : null;
        }

        if (targetType == typeof(Brush))
        {
            var key = mode == CommuteMode.Home ? "TextSecondary" : "Accent";
            return Application.Current.FindResource(key);
        }

        return mode switch
        {
            CommuteMode.Bike => "Bike",
            CommuteMode.Car => "Car",
            CommuteMode.PublicTransport => "Public transport",
            CommuteMode.Home => "Home",
            _ => mode.ToString(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
