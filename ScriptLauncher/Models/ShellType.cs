namespace ScriptLauncher.Models;

/// <summary>
/// Specifies the shell type to use for command execution
/// </summary>
public enum ShellType
{
    /// <summary>
    /// PowerShell shell (Windows default)
    /// </summary>
    PowerShell,

    /// <summary>
    /// Windows Command Prompt (cmd.exe)
    /// </summary>
    Cmd,

    /// <summary>
    /// Bash shell (Linux/macOS default)
    /// </summary>
    Bash
}
