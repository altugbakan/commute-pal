using Microsoft.Win32;

namespace CommutePal;

/// <summary>
/// Registers the app under HKCU\...\Run so Windows launches it at sign-in (no admin rights needed).
/// </summary>
public static class StartupRegistration
{
    public const string StartupArg = "--startup";

    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "CommutePal";

    private static string ExePath =>
        Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];

    private static string Command => $"\"{ExePath}\" {StartupArg}";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(ValueName) is string;
    }

    /// <summary>Enables startup, or refreshes the stored path if the exe moved (e.g. Debug -> Release).</summary>
    public static void Enable()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        key.SetValue(ValueName, Command);
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
