using System.IO;
using Microsoft.Win32;

namespace CommutePal;

/// <summary>
/// Registers the app under HKCU\...\Run so Windows launches it at sign-in (no admin rights needed).
/// Nothing is registered unless the user ticks the box in the app.
/// </summary>
public static class StartupRegistration
{
    public const string StartupArg = "--startup";

    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "CommutePal";

    private static string ExePath =>
        Path.GetFullPath(Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0]);

    private static string Command => $"\"{ExePath}\" {StartupArg}";

    /// <summary>True only if the registered entry points at this very exe.</summary>
    public static bool IsEnabled() =>
        RegisteredPath() is { } registered &&
        string.Equals(registered, ExePath, StringComparison.OrdinalIgnoreCase);

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

    /// <summary>
    /// If an entry exists but points somewhere else (the exe was moved, or an old copy registered it),
    /// remove it so the user can decide again. Returns true if something was removed.
    /// </summary>
    public static bool RemoveIfStale()
    {
        if (RegisteredPath() is not { } registered ||
            string.Equals(registered, ExePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Disable();
        return true;
    }

    private static string? RegisteredPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        if (key?.GetValue(ValueName) is not string command)
        {
            return null;
        }

        // Stored as: "C:\path\CommutePal.exe" --startup
        var trimmed = command.TrimStart();
        var path = trimmed.StartsWith('"') && trimmed.IndexOf('"', 1) is var end && end > 0
            ? trimmed[1..end]
            : trimmed.Split(' ', 2)[0];

        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }
}
