using System.Diagnostics.CodeAnalysis;
using System.Text;
using INIParser;
using ScriptLauncher.Models;
using ScriptLauncher.Utilities;

namespace ScriptLauncher.Services;

/// <summary>
/// Service for managing commands in the INI file
/// </summary>
public class CommandService : ICommandService
{
    private const string SectionPrefix = "Command_";
    private readonly string? _filePath;

    public CommandService() { }

    internal CommandService(string filePath)
    {
        _filePath = filePath;
    }

    private string GetFilePath() => _filePath ?? IniFileHelper.GetIniFilePath();

    [ExcludeFromCodeCoverage]
    private static void LogError(string message) => Console.WriteLine(message);

    /// <summary>
    /// Loads all commands from the INI file
    /// </summary>
    public List<Command> LoadCommands()
    {
        var commands = new List<Command>();
        var filePath = GetFilePath();

        if (!File.Exists(filePath))
        {
            return commands;
        }

        try
        {
            var iniFile = new IniFile(filePath);

            int index = 1;
            while (true)
            {
                string sectionName = $"{SectionPrefix}{index:D3}";
                string? name = iniFile[sectionName, "Name"];

                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                try
                {
                    var command = new Command
                    {
                        Id = commands.Count,
                        Name = name,
                        ShellType = ParseShellType(iniFile[sectionName, "Shell"] ?? "PowerShell"),
                        WorkingDirectory = iniFile[sectionName, "WorkingDirectory"] ?? string.Empty,
                        CommandText = iniFile[sectionName, "CommandText"] ?? string.Empty
                    };

                    commands.Add(command);
                }
                catch (Exception ex)
                {
                    LogError($"Warning: Failed to parse section {sectionName}: {ex.Message}");
                }

                index++;
            }
        }
        catch (Exception ex)
        {
            LogError($"Error loading commands: {ex.Message}");
        }

        return commands;
    }

    /// <summary>
    /// Saves all commands to the INI file
    /// </summary>
    public void SaveCommands(List<Command> commands)
    {
        var filePath = GetFilePath();

        try
        {
            var iniFile = new IniFile();

            for (int i = 0; i < commands.Count; i++)
            {
                string sectionName = $"{SectionPrefix}{i + 1:D3}";

                iniFile[sectionName, "Name"] = commands[i].Name;
                iniFile[sectionName, "Shell"] = commands[i].ShellType.ToString();
                iniFile[sectionName, "WorkingDirectory"] = commands[i].WorkingDirectory;
                iniFile[sectionName, "CommandText"] = commands[i].CommandText;
            }

            iniFile.WriteFile(filePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            LogError($"Error saving commands: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Adds a new command to the INI file
    /// </summary>
    public void AddCommand(Command command)
    {
        var commands = LoadCommands();
        commands.Add(command);
        SaveCommands(commands);
    }

    /// <summary>
    /// Updates an existing command in the INI file
    /// </summary>
    public void UpdateCommand(int index, Command newCommand)
    {
        var commands = LoadCommands();

        if (index >= 0 && index < commands.Count)
        {
            commands[index] = newCommand;
            SaveCommands(commands);
        }
    }

    /// <summary>
    /// Deletes a command from the INI file
    /// </summary>
    public void DeleteCommand(int index)
    {
        var commands = LoadCommands();

        if (index >= 0 && index < commands.Count)
        {
            commands.RemoveAt(index);
            SaveCommands(commands);
        }
    }

    /// <summary>
    /// Parses a shell type string into the ShellType enum
    /// </summary>
    internal static ShellType ParseShellType(string shellString)
    {
        if (Enum.TryParse<ShellType>(shellString, out var shellType))
        {
            return shellType;
        }

        return ShellType.PowerShell;
    }
}
