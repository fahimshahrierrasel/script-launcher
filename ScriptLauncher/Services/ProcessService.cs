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
        if (OperatingSystem.IsMacOS())
        {
            return BuildMacOsStartInfo(command, workingDirectory);
        }
        else
        {
            return BuildLinuxTerminalStartInfo(command, workingDirectory);
        }
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

    private static string? DetectLinuxTerminal()
    {
        var terminals = new[] { "gnome-terminal", "konsole", "xfce4-terminal", "mate-terminal", "lxterminal", "xterm" };

        foreach (var terminal in terminals)
        {
            try
            {
                var whichProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/which",
                        Arguments = terminal,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                whichProcess.Start();
                var result = whichProcess.StandardOutput.ReadToEnd()?.Trim();
                whichProcess.WaitForExit();

                if (whichProcess.ExitCode == 0 && !string.IsNullOrEmpty(result))
                {
                    Console.WriteLine($"[DEBUG] Detected terminal: {terminal} at {result}");
                    return terminal;
                }
            }
            catch
            {
                continue;
            }
        }

        return null;
    }

    private static ProcessStartInfo BuildMacOsStartInfo(Command command, string workingDirectory)
    {
        var script = command.CommandText ?? string.Empty;
        var tempFileName = $"/tmp/script-launcher-{Guid.NewGuid()}.sh";
        var windowTitle = string.IsNullOrWhiteSpace(command.Name) ? "Script Launcher" : command.Name;

        var scriptContent = $@"#!/bin/bash
# Script Launcher - {command.Name}
cd ""{workingDirectory}"" || exit 1
echo 'Running command in: {workingDirectory}'
echo ''
{script}
echo ''
echo 'Press Ctrl+C to close...'
exec bash
";

        File.WriteAllText(tempFileName, scriptContent);

        var chmodProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/chmod",
                Arguments = $"+x \"{tempFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        chmodProcess.Start();
        chmodProcess.WaitForExit();

        var startInfo = new ProcessStartInfo("/usr/bin/open")
        {
            UseShellExecute = false,
            CreateNoWindow = false
        };
        startInfo.ArgumentList.Add("-a");
        startInfo.ArgumentList.Add("Terminal.app");
        startInfo.ArgumentList.Add(tempFileName);

        return startInfo;
    }

    private static ProcessStartInfo BuildLinuxTerminalStartInfo(Command command, string workingDirectory)
    {
        var terminal = DetectLinuxTerminal();
        if (terminal == null)
        {
            var errorMsg = "No terminal emulator found. Please install one of: gnome-terminal, konsole, xfce4-terminal, mate-terminal, lxterminal, or xterm.";
            Console.WriteLine($"[ERROR] {errorMsg}");
            throw new InvalidOperationException(errorMsg);
        }

        var script = command.CommandText ?? string.Empty;
        var windowTitle = string.IsNullOrWhiteSpace(command.Name) ? "Script Launcher" : command.Name;
        var escapedWorkingDir = workingDirectory.Replace("'", "'\\''");
        var escapedScript = script.Replace("'", "'\\''");

        var startInfo = new ProcessStartInfo(terminal)
        {
            UseShellExecute = false,
            CreateNoWindow = false
        };

        switch (terminal)
        {
            case "gnome-terminal":
                startInfo.ArgumentList.Add("--title");
                startInfo.ArgumentList.Add(windowTitle);
                startInfo.ArgumentList.Add("--");
                startInfo.ArgumentList.Add("bash");
                startInfo.ArgumentList.Add("-c");
                startInfo.ArgumentList.Add($"cd '{escapedWorkingDir}' && {escapedScript}; exec bash");
                break;

            case "konsole":
                startInfo.ArgumentList.Add("--new-tab");
                startInfo.ArgumentList.Add($"--title={windowTitle}");
                startInfo.ArgumentList.Add("-e");
                startInfo.ArgumentList.Add("bash");
                startInfo.ArgumentList.Add("-c");
                startInfo.ArgumentList.Add($"cd '{escapedWorkingDir}' && {escapedScript}; exec bash");
                break;

            case "xfce4-terminal":
                startInfo.ArgumentList.Add("--title");
                startInfo.ArgumentList.Add(windowTitle);
                startInfo.ArgumentList.Add("-e");
                startInfo.ArgumentList.Add($"bash -c 'cd '{escapedWorkingDir}' && {escapedScript}; exec bash'");
                break;

            case "mate-terminal":
                startInfo.ArgumentList.Add("--title");
                startInfo.ArgumentList.Add(windowTitle);
                startInfo.ArgumentList.Add("--command");
                startInfo.ArgumentList.Add($"bash -c 'cd '{escapedWorkingDir}' && {escapedScript}; exec bash'");
                break;

            case "lxterminal":
                startInfo.ArgumentList.Add($"--title={windowTitle}");
                startInfo.ArgumentList.Add("-e");
                startInfo.ArgumentList.Add($"bash -c 'cd '{escapedWorkingDir}' && {escapedScript}; exec bash'");
                break;

            default:
                startInfo.ArgumentList.Add("-title");
                startInfo.ArgumentList.Add(windowTitle);
                startInfo.ArgumentList.Add("-e");
                startInfo.ArgumentList.Add($"bash -c 'cd '{escapedWorkingDir}' && {escapedScript}; exec bash'");
                break;
        }

        return startInfo;
    }
}
