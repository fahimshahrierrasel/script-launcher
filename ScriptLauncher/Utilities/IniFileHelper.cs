namespace ScriptLauncher.Utilities;

/// <summary>
/// Helper utilities for managing the commands.ini file
/// </summary>
public static class IniFileHelper
{
    private const string IniFileName = "commands.ini";

    /// <summary>
    /// Gets the full path to the commands.ini file
    /// </summary>
    /// <returns>Path to the INI file in the application's base directory</returns>
    public static string GetIniFilePath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IniFileName);
    }

    /// <summary>
    /// Ensures the INI file exists, creating it if necessary
    /// </summary>
    public static void EnsureIniFileExists()
    {
        var filePath = GetIniFilePath();
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, string.Empty);
        }
    }
}
