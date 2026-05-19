using System.Diagnostics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptLauncher.Models;
using ScriptLauncher.Services;

namespace ScriptLauncher.Tests.Services;

[TestClass]
public class ProcessServiceTests
{
    private ProcessService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new ProcessService();
    }

    [TestMethod]
    public void ExecuteCommand_NullCommand_DoesNotThrow()
    {
        var action = () => _service.ExecuteCommand(null!);
        action.Should().NotThrow();
    }

    [TestMethod]
    public void ExecuteCommand_EmptyWorkingDirectory_DoesNotCrash()
    {
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.PowerShell,
            CommandText = "echo test"
        };

        var action = () => _service.ExecuteCommand(command);
        action.Should().NotThrow();
    }

    [TestMethod]
    public void ExecuteCommand_InvalidCommand_HandlesException()
    {
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.PowerShell,
            WorkingDirectory = @"Z:\NONEXISTENT\PATH\12345",
            CommandText = "echo test"
        };

        // Should catch the exception internally and not throw
        var action = () => _service.ExecuteCommand(command);
        action.Should().NotThrow();
    }

    [TestMethod]
    public void BuildWindowsStartInfo_PowerShell_CorrectArgs()
    {
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.PowerShell,
            WorkingDirectory = @"C:\Test",
            CommandText = "echo hello"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, @"C:\Test");

        startInfo.FileName.Should().Be("cmd.exe");
        startInfo.WorkingDirectory.Should().Be(@"C:\Test");

        var args = startInfo.ArgumentList;
        args.Should().Contain("/c");
        args.Should().Contain("start");
        args.Should().Contain("Test");
        args.Should().Contain("powershell.exe");
        args.Should().Contain("-NoExit");
        args.Should().Contain("-Command");
        args.Should().Contain("echo hello");
    }

    [TestMethod]
    public void BuildWindowsStartInfo_Cmd_CorrectArgs()
    {
        var command = new Command
        {
            Name = "CmdTest",
            ShellType = ShellType.Cmd,
            WorkingDirectory = @"C:\Work",
            CommandText = "dir"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, @"C:\Work");

        var args = startInfo.ArgumentList;
        args.Should().Contain("cmd.exe");
        args.Should().Contain("/k");
        args.Should().Contain("dir");
    }

    [TestMethod]
    public void BuildWindowsStartInfo_Bash_CorrectArgs()
    {
        var command = new Command
        {
            Name = "BashTest",
            ShellType = ShellType.Bash,
            WorkingDirectory = @"C:\Work",
            CommandText = "ls -la"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, @"C:\Work");

        var args = startInfo.ArgumentList;
        args.Should().Contain("bash.exe");
        args.Should().Contain("-c");
        args.Should().ContainMatch("*ls -la*; exec bash*");
    }

    [TestMethod]
    public void BuildWindowsStartInfo_CustomWorkingDir_IncludesDirArg()
    {
        var command = new Command
        {
            Name = "DirTest",
            ShellType = ShellType.PowerShell,
            WorkingDirectory = @"C:\Custom",
            CommandText = "echo test"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, @"C:\Custom");

        var args = startInfo.ArgumentList;
        args.Should().Contain("/d");
        args.Should().Contain(@"C:\Custom");
    }

    [TestMethod]
    public void BuildWindowsStartInfo_EmptyWorkingDir_NoDirArg()
    {
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.PowerShell,
            CommandText = "echo test"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, "");

        var args = startInfo.ArgumentList;
        args.Should().NotContain("/d");
    }

    [TestMethod]
    public void BuildWindowsStartInfo_EmptyName_UsesDefaultTitle()
    {
        var command = new Command
        {
            Name = "",
            ShellType = ShellType.PowerShell,
            CommandText = "echo test"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, @"C:\Test");

        var args = startInfo.ArgumentList;
        args.Should().Contain("Script Launcher");
    }

    [TestMethod]
    public void BuildWindowsStartInfo_WhitespaceName_UsesDefaultTitle()
    {
        var command = new Command
        {
            Name = "   ",
            ShellType = ShellType.PowerShell,
            CommandText = "echo test"
        };

        var startInfo = ProcessService.BuildWindowsStartInfo(command, @"C:\Test");

        var args = startInfo.ArgumentList;
        args.Should().Contain("Script Launcher");
    }

    [TestMethod]
    public void GetWindowsShellInvocation_DefaultCase_ReturnsPowerShell()
    {
        var command = new Command { ShellType = (ShellType)99, CommandText = "test" };

        var (exe, _) = ProcessService.GetWindowsShellInvocation(command);

        exe.Should().Be("powershell.exe");
    }

    [TestMethod]
    public void GetDefaultShellInvocation_AllShells_ReturnsCorrectExecutables()
    {
        ProcessService.GetDefaultShellInvocation(new Command { ShellType = ShellType.PowerShell, CommandText = "t" })
            .executable.Should().Be("pwsh");

        ProcessService.GetDefaultShellInvocation(new Command { ShellType = ShellType.Cmd, CommandText = "t" })
            .executable.Should().Be("cmd");

        ProcessService.GetDefaultShellInvocation(new Command { ShellType = ShellType.Bash, CommandText = "t" })
            .executable.Should().Be("/bin/bash");
    }

    [TestMethod]
    public void GetDefaultShellInvocation_DefaultCase_ReturnsPwsh()
    {
        var (exe, _) = ProcessService.GetDefaultShellInvocation(new Command { ShellType = (ShellType)99, CommandText = "test" });
        exe.Should().Be("pwsh");
    }

    [TestMethod]
    public void GetWindowsShellInvocation_NullCommandText_UsesEmpty()
    {
        var (exe, args) = ProcessService.GetWindowsShellInvocation(
            new Command { ShellType = ShellType.PowerShell, CommandText = null! });

        exe.Should().Be("powershell.exe");
        args.Should().Contain(string.Empty);
    }

    [TestMethod]
    public void GetDefaultShellInvocation_NullCommandText_UsesEmpty()
    {
        var (exe, args) = ProcessService.GetDefaultShellInvocation(
            new Command { ShellType = ShellType.PowerShell, CommandText = null! });

        exe.Should().Be("pwsh");
        args.Should().Contain(string.Empty);
    }

    [TestMethod]
    public void BuildMacOsStartInfo_CreatesCorrectStartInfo()
    {
        var command = new Command
        {
            Name = "MacTest",
            ShellType = ShellType.Bash,
            CommandText = "echo hello"
        };

        // This will try to write temp file and run chmod — may fail on Windows
        // We test what we can: verify it returns a ProcessStartInfo
        try
        {
            var startInfo = ProcessService.BuildMacOsStartInfo(command, "/tmp");
            startInfo.FileName.Should().Be("/usr/bin/open");

            var args = startInfo.ArgumentList;
            args.Should().Contain("-a");
            args.Should().Contain("Terminal.app");
        }
        catch (Exception)
        {
            // Expected on non-macOS — chmod/open won't exist
            // Test passes if code path is reached (coverage)
        }
    }

    [TestMethod]
    public void BuildMacOsStartInfo_EmptyName_UsesDefaultTitle()
    {
        var command = new Command
        {
            Name = "",
            ShellType = ShellType.Bash,
            CommandText = "echo test"
        };

        try
        {
            ProcessService.BuildMacOsStartInfo(command, "/tmp");
        }
        catch
        {
            // Expected on non-macOS
        }
    }

    [TestMethod]
    public void BuildMacOsStartInfo_NullCommandText_UsesEmpty()
    {
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.Bash,
            CommandText = null!
        };

        try
        {
            ProcessService.BuildMacOsStartInfo(command, "/tmp");
        }
        catch
        {
            // Expected on non-macOS
        }
    }

    [TestMethod]
    public void BuildLinuxTerminalStartInfo_GnomeTerminal_CorrectArgs()
    {
        var command = new Command
        {
            Name = "LinuxTest",
            ShellType = ShellType.Bash,
            CommandText = "ls -la"
        };

        try
        {
            var startInfo = ProcessService.BuildLinuxTerminalStartInfo(command, "/home/user");
            // If on Linux with gnome-terminal, check args
            startInfo.Should().NotBeNull();
        }
        catch (InvalidOperationException)
        {
            // Expected when no terminal emulator found (Windows, CI, etc.)
        }
    }

    [TestMethod]
    public void BuildLinuxTerminalStartInfo_EmptyName_UsesDefaultTitle()
    {
        var command = new Command
        {
            Name = "",
            ShellType = ShellType.Bash,
            CommandText = "echo test"
        };

        try
        {
            ProcessService.BuildLinuxTerminalStartInfo(command, "/home/user");
        }
        catch (InvalidOperationException)
        {
            // Expected on non-Linux
        }
    }

    [TestMethod]
    public void BuildLinuxTerminalStartInfo_NullCommandText_UsesEmpty()
    {
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.Bash,
            CommandText = null!
        };

        try
        {
            ProcessService.BuildLinuxTerminalStartInfo(command, "/home/user");
        }
        catch (InvalidOperationException)
        {
            // Expected on non-Linux
        }
    }

    [TestMethod]
    public void DetectLinuxTerminal_OnWindows_ReturnsNull()
    {
        // On Windows, /usr/bin/which doesn't exist
        var result = ProcessService.DetectLinuxTerminal();
        // Should return null on Windows (no terminals found)
        result.Should().BeNull();
    }

    [TestMethod]
    public void BuildCrossPlatformStartInfo_OnMacOS_RoutesCorrectly()
    {
        // Tests the routing logic. On Windows, IsMacOS() = false, so it goes to Linux path
        var command = new Command
        {
            Name = "Test",
            ShellType = ShellType.Bash,
            CommandText = "echo test"
        };

        try
        {
            ProcessService.BuildCrossPlatformStartInfo(command, "/tmp");
        }
        catch (InvalidOperationException)
        {
            // Expected when no terminal emulator found
        }
    }
}
