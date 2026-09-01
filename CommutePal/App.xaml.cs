using System.Windows;

namespace CommutePal;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var launchedAtStartup = e.Args.Contains(StartupRegistration.StartupArg, StringComparer.OrdinalIgnoreCase);
        var firstRun = !CommuteLog.HasAnyData;

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

        // Sign-in launch gets the minimal four-button popup; a manual launch gets the full view.
        MainWindow = new MainWindow(log, compact: launchedAtStartup);
        MainWindow.Show();
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
