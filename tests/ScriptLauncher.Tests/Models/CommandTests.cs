using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptLauncher.Models;

namespace ScriptLauncher.Tests.Models;

[TestClass]
public class CommandTests
{
    [TestMethod]
    public void DefaultConstructor_SetsDefaults()
    {
        var command = new Command();

        command.Id.Should().Be(-1);
        command.Name.Should().BeEmpty();
        command.ShellType.Should().Be(ShellType.PowerShell);
        command.WorkingDirectory.Should().BeEmpty();
        command.CommandText.Should().BeEmpty();
    }

    [TestMethod]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new Command
        {
            Id = 1,
            Name = "Test",
            ShellType = ShellType.Bash,
            WorkingDirectory = "/tmp",
            CommandText = "echo hello"
        };

        var clone = original.Clone();

        clone.Name = "Modified";
        clone.ShellType = ShellType.Cmd;

        original.Name.Should().Be("Test");
        original.ShellType.Should().Be(ShellType.Bash);
    }

    [TestMethod]
    public void Clone_CopiesAllProperties()
    {
        var original = new Command
        {
            Id = 5,
            Name = "Build",
            ShellType = ShellType.Cmd,
            WorkingDirectory = @"C:\Projects",
            CommandText = "dotnet build"
        };

        var clone = original.Clone();

        clone.Id.Should().Be(5);
        clone.Name.Should().Be("Build");
        clone.ShellType.Should().Be(ShellType.Cmd);
        clone.WorkingDirectory.Should().Be(@"C:\Projects");
        clone.CommandText.Should().Be("dotnet build");
    }

    [TestMethod]
    public void Properties_AreReadWrite()
    {
        var command = new Command
        {
            Id = 10,
            Name = "Deploy",
            ShellType = ShellType.Bash,
            WorkingDirectory = "/home/user",
            CommandText = "./deploy.sh"
        };

        command.Id.Should().Be(10);
        command.Name.Should().Be("Deploy");
        command.ShellType.Should().Be(ShellType.Bash);
        command.WorkingDirectory.Should().Be("/home/user");
        command.CommandText.Should().Be("./deploy.sh");
    }
}
