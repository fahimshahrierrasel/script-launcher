namespace ScriptLauncher.Models;

/// <summary>
/// Represents a command that can be executed
/// </summary>
public class Command
{
    /// <summary>
    /// Display name for the command
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Shell type to use for execution
    /// </summary>
    public ShellType ShellType { get; set; } = ShellType.PowerShell;

    /// <summary>
    /// Working directory where the command should execute
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The actual command text to execute
    /// </summary>
    public string CommandText { get; set; } = string.Empty;

    /// <summary>
    /// Creates a deep copy of this command
    /// </summary>
    public Command Clone()
    {
        return new Command
        {
            Name = Name,
            ShellType = ShellType,
            WorkingDirectory = WorkingDirectory,
            CommandText = CommandText
        };
    }
}
