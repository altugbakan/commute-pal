using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CommutePal.Controls;

/// <summary>
/// A month grid for picking a past day. Each logged day shows a dot under its number:
/// accent for office days (bike, car, public transport), muted for home days.
/// </summary>
public partial class MonthPicker : UserControl
{
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly _displayMonth;

    public MonthPicker()
    {
        InitializeComponent();
        _displayMonth = FirstOfMonth(_selectedDate);
        BuildWeekdayHeaders();
        Rebuild();
    }

    /// <summary>Days after this cannot be picked.</summary>
    public DateOnly MaxDate { get; set; } = DateOnly.MaxValue;

    /// <summary>Tells the picker what is logged on a day, so it can draw the marker.</summary>
    public Func<DateOnly, CommuteMode?>? DayLookup { get; set; }

    public event EventHandler<DateOnly>? DateSelected;

    /// <summary>Selects a day and shows its month.</summary>
    public void ShowDate(DateOnly date)
    {
        _selectedDate = date;
        _displayMonth = FirstOfMonth(date);
        Rebuild();
    }

    private void PrevMonth_Click(object sender, RoutedEventArgs e)
    {
        _displayMonth = _displayMonth.AddMonths(-1);
        Rebuild();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        _displayMonth = _displayMonth.AddMonths(1);
        Rebuild();
    }

    private void DayCell_Click(object sender, RoutedEventArgs e)
    {
        var date = (DateOnly)((Button)sender).Tag;
        _selectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    private void BuildWeekdayHeaders()
    {
        var format = CultureInfo.CurrentCulture.DateTimeFormat;
        var first = (int)format.FirstDayOfWeek;

        WeekdayRow.Children.Clear();
        for (var i = 0; i < 7; i++)
        {
            var name = format.AbbreviatedDayNames[(first + i) % 7];
            WeekdayRow.Children.Add(new TextBlock { Text = name, Style = (Style)FindResource("WeekdayHeader") });
        }
    }

    private void Rebuild()
    {
        var culture = CultureInfo.CurrentCulture;
        var today = DateOnly.FromDateTime(DateTime.Today);

        MonthText.Text = _displayMonth.ToString("MMMM yyyy", culture);
        NextMonthButton.IsEnabled = _displayMonth.AddMonths(1) <= FirstOfMonth(MaxDate);

        // Start the grid on the week that contains the 1st, always drawing 6 rows so the popup never resizes.
        var firstWeekday = (int)culture.DateTimeFormat.FirstDayOfWeek;
        var offset = ((int)_displayMonth.DayOfWeek - firstWeekday + 7) % 7;
        var cursor = _displayMonth.AddDays(-offset);

        DayGrid.Children.Clear();
        for (var i = 0; i < 42; i++, cursor = cursor.AddDays(1))
        {
            DayGrid.Children.Add(CreateCell(cursor, today));
        }
    }

    private Button CreateCell(DateOnly date, DateOnly today)
    {
        var isSelected = date == _selectedDate;
        var inMonth = date.Month == _displayMonth.Month;
        var mode = DayLookup?.Invoke(date);

        var number = new TextBlock
        {
            Text = date.Day.ToString(CultureInfo.CurrentCulture),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 5, 0, 0),
            FontWeight = date == today ? FontWeights.Bold : FontWeights.Normal,
            Opacity = inMonth ? 1 : 0.4,
        };

        var dot = new Ellipse
        {
            Width = 5,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 5),
            Fill = mode switch
            {
                null => Brushes.Transparent,
                CommuteMode.Home => (Brush)FindResource("TextSecondary"),
                _ => (Brush)FindResource("Accent"),
            },
        };

        var cell = new Button
        {
            Style = (Style)FindResource("DayCell"),
            Tag = date,
            IsEnabled = date <= MaxDate,
            Content = new Grid { Children = { number, dot } },
            ToolTip = mode?.DisplayName(),
        };

        if (isSelected)
        {
            cell.Background = (Brush)FindResource("SelectedBg");
            cell.BorderBrush = (Brush)FindResource("Accent");
        }

        cell.Click += DayCell_Click;
        return cell;
    }

    private static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);
}
