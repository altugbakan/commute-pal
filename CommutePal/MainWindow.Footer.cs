using System.Diagnostics;
using System.Windows;

namespace CommutePal;

/// <summary>The sign-in checkbox and the log-folder link at the bottom of the full view.</summary>
public partial class MainWindow
{
    private bool _suppressStartupToggle;

    private void InitializeStartupCheckBox()
    {
        _suppressStartupToggle = true;
        StartupCheckBox.IsChecked = StartupRegistration.IsEnabled();
        _suppressStartupToggle = false;
    }

    private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_suppressStartupToggle)
        {
            return;
        }

        try
        {
            if (StartupCheckBox.IsChecked == true)
            {
                StartupRegistration.Enable();
            }
            else
            {
                StartupRegistration.Disable();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not update the sign-in setting:\n{ex.Message}",
                "CommutePal", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenLog_Click(object sender, RoutedEventArgs e)
    {
        System.IO.Directory.CreateDirectory(CommuteLog.Directory);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{CommuteLog.Directory}\"") { UseShellExecute = true });
    }
}
