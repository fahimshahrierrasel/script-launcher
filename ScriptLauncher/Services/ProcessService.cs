using System.Diagnostics;
using ScriptLauncher.Models;

namespace ScriptLauncher.Services;

/// <summary>
/// Service for executing commands in a cross-platform manner
/// </summary>
public class ProcessService
{
    /// <summary>
    /// Executes a command using the specified shell
    /// </summary>
    /// <param name="command">Command to execute</param>
    public void ExecuteCommand(Command command)
    {
        if (command == null)
        {
            Console.WriteLine("Error: No command selected");
            return;
        }

        try
        {
            var workingDirectory = string.IsNullOrWhiteSpace(command.WorkingDirectory)
                ? AppDomain.CurrentDomain.BaseDirectory
                : command.WorkingDirectory;

            var startInfo = OperatingSystem.IsWindows()
                ? BuildWindowsStartInfo(command, workingDirectory)
                : BuildCrossPlatformStartInfo(command, workingDirectory);

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to execute command: {ex.Message}");
        }
    }

    private static ProcessStartInfo BuildWindowsStartInfo(Command command, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("start");
        startInfo.ArgumentList.Add(string.IsNullOrWhiteSpace(command.Name) ? "Script Launcher" : command.Name);

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            startInfo.ArgumentList.Add("/d");
            startInfo.ArgumentList.Add(workingDirectory);
        }

        var (shellExe, shellArgs) = GetWindowsShellInvocation(command);
        startInfo.ArgumentList.Add(shellExe);
        foreach (var argument in shellArgs)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static string EscapeCommandLineArgument(string argument)
    {
        // Properly escape arguments for CMD
        if (argument.Contains(" ") || argument.Contains("\t") || argument.Contains("\""))
        {
            return $"\"{argument.Replace("\"", "\"\"")}\"";
        }
        return argument;
    }

    private static ProcessStartInfo BuildCrossPlatformStartInfo(Command command, string workingDirectory)
    {
        var (shellExe, shellArgs) = GetDefaultShellInvocation(command);
        var startInfo = new ProcessStartInfo(shellExe)
        {
            UseShellExecute = false,
            CreateNoWindow = false,
            WorkingDirectory = workingDirectory
        };

        foreach (var argument in shellArgs)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static (string executable, IEnumerable<string> arguments) GetWindowsShellInvocation(Command command)
    {
        var script = command.CommandText ?? string.Empty;

        return command.ShellType switch
        {
            ShellType.PowerShell => ("powershell.exe", new[] { "-NoExit", "-Command", script }),
            ShellType.Cmd => ("cmd.exe", new[] { "/k", script }),
            ShellType.Bash => ("bash.exe", new[] { "-c", $"{script}; exec bash" }),
            _ => ("powershell.exe", new[] { "-NoExit", "-Command", script })
        };
    }

    private static (string executable, IEnumerable<string> arguments) GetDefaultShellInvocation(Command command)
    {
        var script = command.CommandText ?? string.Empty;

        return command.ShellType switch
        {
            ShellType.PowerShell => ("pwsh", new[] { "-NoExit", "-Command", script }),
            ShellType.Cmd => ("cmd", new[] { "/k", script }),
            ShellType.Bash => ("/bin/bash", new[] { "-c", script }),
            _ => ("pwsh", new[] { "-NoExit", "-Command", script })
        };
    }
}
