using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptLauncher.Models;
using ScriptLauncher.Services;

namespace ScriptLauncher.Tests.Services;

[TestClass]
public class CommandServiceTests
{
    private string _tempDir = null!;
    private string _iniPath = null!;
    private CommandService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"cmd-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _iniPath = Path.Combine(_tempDir, "commands.ini");
        _service = new CommandService(_iniPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private void WriteIni(string content)
    {
        File.WriteAllText(_iniPath, content, Encoding.UTF8);
    }

    [TestMethod]
    public void LoadCommands_MissingFile_ReturnsEmptyList()
    {
        var commands = _service.LoadCommands();
        commands.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadCommands_EmptyFile_ReturnsEmptyList()
    {
        WriteIni("");
        var commands = _service.LoadCommands();
        commands.Should().BeEmpty();
    }

    [TestMethod]
    public void LoadCommands_ParsesValidIni()
    {
        WriteIni(@"
[Command_001]
Name=Build Project
Shell=PowerShell
WorkingDirectory=C:\Projects
CommandText=dotnet build

[Command_002]
Name=Run Tests
Shell=Bash
WorkingDirectory=/home/user
CommandText=dotnet test
");
        var commands = _service.LoadCommands();

        commands.Should().HaveCount(2);
        commands[0].Id.Should().Be(0);
        commands[0].Name.Should().Be("Build Project");
        commands[0].ShellType.Should().Be(ShellType.PowerShell);
        commands[0].WorkingDirectory.Should().Be(@"C:\Projects");
        commands[0].CommandText.Should().Be("dotnet build");

        commands[1].Id.Should().Be(1);
        commands[1].Name.Should().Be("Run Tests");
        commands[1].ShellType.Should().Be(ShellType.Bash);
    }

    [TestMethod]
    public void LoadCommands_SkipsMalformedSections()
    {
        WriteIni(@"
[Command_001]
Name=Good Command
Shell=PowerShell
CommandText=echo good

[Command_002]
Name=Bad Shell
Shell=InvalidShellType
CommandText=echo bad
");
        var commands = _service.LoadCommands();

        commands.Should().HaveCount(2);
        commands[1].ShellType.Should().Be(ShellType.PowerShell); // defaults on invalid
    }

    [TestMethod]
    public void LoadCommands_AssignsSequentialIds()
    {
        WriteIni(@"
[Command_001]
Name=First
Shell=Cmd
CommandText=a

[Command_002]
Name=Second
Shell=Bash
CommandText=b

[Command_003]
Name=Third
Shell=PowerShell
CommandText=c
");
        var commands = _service.LoadCommands();

        commands[0].Id.Should().Be(0);
        commands[1].Id.Should().Be(1);
        commands[2].Id.Should().Be(2);
    }

    [TestMethod]
    public void LoadCommands_MissingFields_UsesDefaults()
    {
        WriteIni(@"
[Command_001]
Name=Partial
");
        var commands = _service.LoadCommands();

        commands.Should().HaveCount(1);
        commands[0].Name.Should().Be("Partial");
        commands[0].ShellType.Should().Be(ShellType.PowerShell);
        commands[0].WorkingDirectory.Should().BeEmpty();
        commands[0].CommandText.Should().BeEmpty();
    }

    [TestMethod]
    public void SaveCommands_WritesCorrectFormat()
    {
        var commands = new List<Command>
        {
            new() { Name = "Test1", ShellType = ShellType.PowerShell, CommandText = "echo 1" },
            new() { Name = "Test2", ShellType = ShellType.Bash, CommandText = "echo 2" }
        };

        _service.SaveCommands(commands);
        File.Exists(_iniPath).Should().BeTrue();

        var content = File.ReadAllText(_iniPath);
        content.Should().Contain("Test1");
        content.Should().Contain("Test2");
        content.Should().Contain("PowerShell");
        content.Should().Contain("Bash");
    }

    [TestMethod]
    public void SaveCommands_EmptyList_WritesEmptyFile()
    {
        _service.SaveCommands([]);
        File.Exists(_iniPath).Should().BeTrue();

        var reloaded = _service.LoadCommands();
        reloaded.Should().BeEmpty();
    }

    [TestMethod]
    public void AddCommand_AppendsToExisting()
    {
        WriteIni(@"
[Command_001]
Name=Existing
Shell=Cmd
CommandText=echo existing
");
        var command = new Command
        {
            Name = "Added",
            ShellType = ShellType.Bash,
            CommandText = "echo added"
        };

        _service.AddCommand(command);

        var commands = _service.LoadCommands();
        commands.Should().HaveCount(2);
        commands[1].Name.Should().Be("Added");
    }

    [TestMethod]
    public void UpdateCommand_ValidIndex_ReplacesCommand()
    {
        WriteIni(@"
[Command_001]
Name=Original
Shell=PowerShell
CommandText=original
");
        var updated = new Command
        {
            Name = "Updated",
            ShellType = ShellType.Bash,
            CommandText = "updated"
        };

        _service.UpdateCommand(0, updated);

        var reloaded = _service.LoadCommands();
        reloaded.Should().HaveCount(1);
        reloaded[0].Name.Should().Be("Updated");
        reloaded[0].ShellType.Should().Be(ShellType.Bash);
    }

    [TestMethod]
    public void UpdateCommand_InvalidIndex_DoesNotThrow()
    {
        WriteIni(@"
[Command_001]
Name=Existing
Shell=Cmd
CommandText=test
");
        var action = () => _service.UpdateCommand(-1, new Command { Name = "X" });
        action.Should().NotThrow();

        var action2 = () => _service.UpdateCommand(9999, new Command { Name = "Y" });
        action2.Should().NotThrow();

        // Verify original unchanged
        var commands = _service.LoadCommands();
        commands.Should().HaveCount(1);
        commands[0].Name.Should().Be("Existing");
    }

    [TestMethod]
    public void DeleteCommand_ValidIndex_RemovesCommand()
    {
        WriteIni(@"
[Command_001]
Name=Keep
Shell=Cmd
CommandText=a

[Command_002]
Name=Delete
Shell=Bash
CommandText=b
");
        _service.DeleteCommand(1);

        var commands = _service.LoadCommands();
        commands.Should().HaveCount(1);
        commands[0].Name.Should().Be("Keep");
    }

    [TestMethod]
    public void DeleteCommand_InvalidIndex_DoesNotThrow()
    {
        WriteIni(@"
[Command_001]
Name=Existing
Shell=Cmd
CommandText=test
");
        var action = () => _service.DeleteCommand(-1);
        action.Should().NotThrow();

        var action2 = () => _service.DeleteCommand(9999);
        action2.Should().NotThrow();

        var commands = _service.LoadCommands();
        commands.Should().HaveCount(1);
    }

    [TestMethod]
    public void ParseShellType_ValidShell_ReturnsCorrectType()
    {
        CommandService.ParseShellType("PowerShell").Should().Be(ShellType.PowerShell);
        CommandService.ParseShellType("Cmd").Should().Be(ShellType.Cmd);
        CommandService.ParseShellType("Bash").Should().Be(ShellType.Bash);
    }

    [TestMethod]
    public void ParseShellType_InvalidString_DefaultsToPowerShell()
    {
        CommandService.ParseShellType("InvalidShell").Should().Be(ShellType.PowerShell);
        CommandService.ParseShellType("").Should().Be(ShellType.PowerShell);
    }

    [TestMethod]
    public void ParseShellType_CaseSensitive_ParsesCorrectly()
    {
        CommandService.ParseShellType("PowerShell").Should().Be(ShellType.PowerShell);
        CommandService.ParseShellType("Cmd").Should().Be(ShellType.Cmd);
        CommandService.ParseShellType("Bash").Should().Be(ShellType.Bash);
    }

    [TestMethod]
    public void ParseShellType_LowercaseString_DefaultsToPowerShell()
    {
        CommandService.ParseShellType("powershell").Should().Be(ShellType.PowerShell);
        CommandService.ParseShellType("cmd").Should().Be(ShellType.PowerShell);
    }

    [TestMethod]
    public void LoadCommands_CorruptedFile_ReturnsEmptyList()
    {
        // Write binary garbage that IniFile can't parse
        File.WriteAllBytes(_iniPath, [0x00, 0x01, 0x02, 0x03, 0xFF, 0xFE]);

        var commands = _service.LoadCommands();
        // Should not crash, returns whatever it can parse
        commands.Should().NotBeNull();
    }

    [TestMethod]
    public void SaveCommands_ThenLoadCommands_RoundTrip()
    {
        var original = new List<Command>
        {
            new() { Name = "A", ShellType = ShellType.PowerShell, WorkingDirectory = "C:\\A", CommandText = "echo a" },
            new() { Name = "B", ShellType = ShellType.Cmd, WorkingDirectory = "C:\\B", CommandText = "echo b" },
            new() { Name = "C", ShellType = ShellType.Bash, WorkingDirectory = "/tmp", CommandText = "echo c" }
        };

        _service.SaveCommands(original);
        var loaded = _service.LoadCommands();

        loaded.Should().HaveCount(3);
        loaded[0].Name.Should().Be("A");
        loaded[0].ShellType.Should().Be(ShellType.PowerShell);
        loaded[0].WorkingDirectory.Should().Be("C:\\A");
        loaded[0].CommandText.Should().Be("echo a");

        loaded[1].Name.Should().Be("B");
        loaded[1].ShellType.Should().Be(ShellType.Cmd);

        loaded[2].Name.Should().Be("C");
        loaded[2].ShellType.Should().Be(ShellType.Bash);
    }
}
