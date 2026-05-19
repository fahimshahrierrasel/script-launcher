namespace ScriptLauncher.Utilities;

/// <summary>
/// Helper utilities for managing the commands.ini file
/// </summary>
public static class IniFileHelper
{
    private const string IniFileName = "commands.ini";

    internal static string? OverridePath { get; set; }

    public static string GetIniFilePath()
    {
        if (OverridePath != null) return OverridePath;
        var exeDirectory = Path.GetDirectoryName(Environment.ProcessPath)
            ?? AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(exeDirectory, IniFileName);
    }

    public static void EnsureIniFileExists()
    {
        var filePath = GetIniFilePath();
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, string.Empty);
        }
    }
}
