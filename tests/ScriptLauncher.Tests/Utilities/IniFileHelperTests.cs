using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptLauncher.Utilities;

namespace ScriptLauncher.Tests.Utilities;

[TestClass]
public class IniFileHelperTests
{
    private string _tempDir = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ini-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        IniFileHelper.OverridePath = null;
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [TestMethod]
    public void GetIniFilePath_ReturnsPathEndingWithCommandsIni()
    {
        var path = IniFileHelper.GetIniFilePath();
        path.Should().EndWith("commands.ini");
    }

    [TestMethod]
    public void GetIniFilePath_ReturnsRootedPath()
    {
        var path = IniFileHelper.GetIniFilePath();
        path.Should().NotBeNullOrEmpty();
        Path.IsPathRooted(path).Should().BeTrue();
    }

    [TestMethod]
    public void GetIniFilePath_UsesOverridePath()
    {
        var customPath = Path.Combine(_tempDir, "custom.ini");
        IniFileHelper.OverridePath = customPath;

        IniFileHelper.GetIniFilePath().Should().Be(customPath);
    }

    [TestMethod]
    public void EnsureIniFileExists_CreatesFileWhenMissing()
    {
        var customPath = Path.Combine(_tempDir, "commands.ini");
        IniFileHelper.OverridePath = customPath;

        File.Exists(customPath).Should().BeFalse();

        IniFileHelper.EnsureIniFileExists();

        File.Exists(customPath).Should().BeTrue();
        File.ReadAllText(customPath).Should().BeEmpty();
    }

    [TestMethod]
    public void EnsureIniFileExists_DoesNotOverwriteExistingFile()
    {
        var customPath = Path.Combine(_tempDir, "commands.ini");
        File.WriteAllText(customPath, "existing content");
        IniFileHelper.OverridePath = customPath;

        IniFileHelper.EnsureIniFileExists();

        File.ReadAllText(customPath).Should().Be("existing content");
    }

    [TestMethod]
    public void EnsureIniFileExists_IsIdempotent()
    {
        var customPath = Path.Combine(_tempDir, "commands.ini");
        IniFileHelper.OverridePath = customPath;

        IniFileHelper.EnsureIniFileExists();
        IniFileHelper.EnsureIniFileExists();

        File.Exists(customPath).Should().BeTrue();
    }
}
