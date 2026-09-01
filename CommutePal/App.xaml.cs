using System.Windows;
using Microsoft.Win32;

namespace CommutePal;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var launchedAtStartup = e.Args.Contains(StartupRegistration.StartupArg, StringComparer.OrdinalIgnoreCase);
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

        // Launched by Windows at sign-in and today is already logged: stay silent.
        if (launchedAtStartup && log.Get(today) is not null)
        {
            Shutdown();
            return;
        }

        // First time the app is opened by hand: register it to run at sign-in.
        if (firstRun && !launchedAtStartup)
        {
            TryEnableStartup();
        }

        // Sign-in launch gets the minimal icon popup; a manual launch gets the full view.
        MainWindow = new MainWindow(log, compact: launchedAtStartup);
        MainWindow.Show();
    }

    /// <summary>Follows the Windows "Choose your default app mode" setting. --dark / --light force it (handy for testing).</summary>
    private static bool ResolveDarkMode(string[] args)
    {
        if (args.Contains("--dark", StringComparer.OrdinalIgnoreCase)) return true;
        if (args.Contains("--light", StringComparer.OrdinalIgnoreCase)) return false;

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

        // The Fluent theme normally follows the OS on its own; forcing it keeps --dark/--light consistent.
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
