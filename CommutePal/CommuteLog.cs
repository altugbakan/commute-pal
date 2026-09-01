using System.IO;
using System.Text.RegularExpressions;

namespace CommutePal;

public enum CommuteMode
{
    Bike,
    Car,
    PublicTransport,
    Home,
}

public sealed record MonthStats(int Bike, int Car, int PublicTransport, int Home)
{
    public int Total => Bike + Car + PublicTransport + Home;
}

/// <summary>
/// One entry per day, persisted as one CSV per month (e.g. 2026-08.csv, rows "date,mode")
/// so a month can be handed over or opened in Excel on its own.
/// </summary>
public sealed partial class CommuteLog
{
    public static readonly string Directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CommutePal");

    private const string Header = "date,mode";
    private const string DateFormat = "yyyy-MM-dd";

    [GeneratedRegex(@"^\d{4}-\d{2}\.csv$", RegexOptions.IgnoreCase)]
    private static partial Regex MonthFileName();

    private readonly SortedDictionary<DateOnly, CommuteMode> _entries = new();

    public static bool HasAnyData =>
        System.IO.Directory.Exists(Directory) && MonthFiles().Any();

    public static string FileFor(int year, int month) =>
        Path.Combine(Directory, $"{year:0000}-{month:00}.csv");

    public static CommuteLog Load()
    {
        var log = new CommuteLog();
        if (!System.IO.Directory.Exists(Directory))
        {
            return log;
        }

        foreach (var file in MonthFiles())
        {
            foreach (var line in File.ReadLines(file))
            {
                var parts = line.Split(',');
                if (parts.Length != 2)
                {
                    continue;
                }

                if (DateOnly.TryParseExact(parts[0].Trim(), DateFormat, out var date)
                    && Enum.TryParse<CommuteMode>(parts[1].Trim(), ignoreCase: true, out var mode))
                {
                    log._entries[date] = mode;
                }
            }
        }

        return log;
    }

    public CommuteMode? Get(DateOnly date) =>
        _entries.TryGetValue(date, out var mode) ? mode : null;

    public void Set(DateOnly date, CommuteMode mode)
    {
        _entries[date] = mode;
        SaveMonth(date.Year, date.Month);
    }

    public MonthStats StatsFor(int year, int month)
    {
        var inMonth = EntriesIn(year, month).Select(e => e.Value).ToList();

        return new MonthStats(
            Bike: inMonth.Count(m => m == CommuteMode.Bike),
            Car: inMonth.Count(m => m == CommuteMode.Car),
            PublicTransport: inMonth.Count(m => m == CommuteMode.PublicTransport),
            Home: inMonth.Count(m => m == CommuteMode.Home));
    }

    private IEnumerable<KeyValuePair<DateOnly, CommuteMode>> EntriesIn(int year, int month) =>
        _entries.Where(e => e.Key.Year == year && e.Key.Month == month);

    private static IEnumerable<string> MonthFiles() =>
        System.IO.Directory.EnumerateFiles(Directory, "*.csv")
            .Where(f => MonthFileName().IsMatch(Path.GetFileName(f)))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

    private void SaveMonth(int year, int month)
    {
        System.IO.Directory.CreateDirectory(Directory);

        var lines = new List<string> { Header };
        lines.AddRange(EntriesIn(year, month).Select(e => $"{e.Key.ToString(DateFormat)},{e.Value}"));

        // Write to a temp file first so a crash mid-write cannot truncate the month.
        var target = FileFor(year, month);
        var tmp = target + ".tmp";
        File.WriteAllLines(tmp, lines);
        File.Move(tmp, target, overwrite: true);
    }
}
