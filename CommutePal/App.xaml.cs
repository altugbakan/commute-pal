using System.Windows;
using Microsoft.Win32;

namespace CommutePal;

/// <summary>
/// Command line:
///   --startup   launched by Windows at sign-in: show the popup, or exit silently if today is logged
///   --popup     always show the popup (for testing)
///   --dark / --light   force the theme instead of following Windows (for testing)
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var launchedAtStartup = HasArg(e.Args, StartupRegistration.StartupArg);
        var forcePopup = HasArg(e.Args, "--popup");
        var firstRun = !CommuteLog.HasAnyData;

        ApplyTheme(dark: ResolveDarkMode(e.Args));

        CommuteLog log;
        try
        {
            log = CommuteLog.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not read the commute log:\n{ex.Message}\n\n{CommuteLog.Directory}",
                "CommutePal", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (launchedAtStartup && log.Get(today) is not null)
        {
            Shutdown(); // nothing to ask today
            return;
        }

        if (firstRun && !launchedAtStartup)
        {
            TryEnableStartup(); // first manual launch registers the sign-in prompt
        }

        MainWindow = launchedAtStartup || forcePopup
            ? new PopupWindow(log)
            : new MainWindow(log);
        MainWindow.Show();
    }

    private static bool HasArg(string[] args, string name) =>
        args.Contains(name, StringComparer.OrdinalIgnoreCase);

    private static bool ResolveDarkMode(string[] args)
    {
        if (HasArg(args, "--dark")) return true;
        if (HasArg(args, "--light")) return false;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int useLight && useLight == 0;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyTheme(bool dark)
    {
        var source = new Uri($"Themes/{(dark ? "Dark" : "Light")}.xaml", UriKind.Relative);
        Resources.MergedDictionaries.Add(new ResourceDictionary { Source = source });
        ThemeMode = dark ? ThemeMode.Dark : ThemeMode.Light;
    }

    private static void TryEnableStartup()
    {
        try
        {
            StartupRegistration.Enable();
        }
        catch
        {
            // Not fatal; the user can toggle it from the checkbox later.
        }
    }
}
