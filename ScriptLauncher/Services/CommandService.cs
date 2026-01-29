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

    /// <summary>
    /// Loads all commands from the INI file
    /// </summary>
    public List<Command> LoadCommands()
    {
        var commands = new List<Command>();
        var filePath = IniFileHelper.GetIniFilePath();

        if (!File.Exists(filePath))
        {
            return commands;
        }

        try
        {
            var iniFile = new IniFile(filePath);

            // Iterate through sections to find Command_XXX sections
            // We need to try incrementing numbers until we don't find any more
            int index = 1;
            while (true)
            {
                string sectionName = $"{SectionPrefix}{index:D3}";
                string? name = iniFile[sectionName, "Name"];

                if (string.IsNullOrEmpty(name))
                {
                    // No more command sections found
                    break;
                }

                try
                {
                    var command = new Command
                    {
                        Name = name,
                        ShellType = ParseShellType(iniFile[sectionName, "Shell"] ?? "PowerShell"),
                        WorkingDirectory = iniFile[sectionName, "WorkingDirectory"] ?? string.Empty,
                        CommandText = iniFile[sectionName, "CommandText"] ?? string.Empty
                    };

                    commands.Add(command);
                }
                catch (Exception ex)
                {
                    // Skip malformed sections but log the issue
                    Console.WriteLine($"Warning: Failed to parse section {sectionName}: {ex.Message}");
                }

                index++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading commands: {ex.Message}");
        }

        return commands;
    }

    /// <summary>
    /// Saves all commands to the INI file
    /// </summary>
    public void SaveCommands(List<Command> commands)
    {
        var filePath = IniFileHelper.GetIniFilePath();

        try
        {
            var iniFile = new IniFile();

            for (int i = 0; i < commands.Count; i++)
            {
                string sectionName = $"{SectionPrefix}{i + 1:D3}"; // Command_001, Command_002, etc.

                iniFile[sectionName, "Name"] = commands[i].Name;
                iniFile[sectionName, "Shell"] = commands[i].ShellType.ToString();
                iniFile[sectionName, "WorkingDirectory"] = commands[i].WorkingDirectory;
                iniFile[sectionName, "CommandText"] = commands[i].CommandText;
            }

            iniFile.WriteFile(filePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving commands: {ex.Message}");
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
    public void UpdateCommand(Command oldCommand, Command newCommand)
    {
        var commands = LoadCommands();
        var index = commands.FindIndex(c =>
            c.Name == oldCommand.Name &&
            c.CommandText == oldCommand.CommandText &&
            c.ShellType == oldCommand.ShellType &&
            c.WorkingDirectory == oldCommand.WorkingDirectory);

        if (index >= 0)
        {
            commands[index] = newCommand;
            SaveCommands(commands);
        }
    }

    /// <summary>
    /// Deletes a command from the INI file
    /// </summary>
    public void DeleteCommand(Command command)
    {
        var commands = LoadCommands();
        var index = commands.FindIndex(c =>
            c.Name == command.Name &&
            c.CommandText == command.CommandText &&
            c.ShellType == command.ShellType &&
            c.WorkingDirectory == command.WorkingDirectory);

        if (index >= 0)
        {
            commands.RemoveAt(index);
            SaveCommands(commands);
        }
    }

    /// <summary>
    /// Parses a shell type string into the ShellType enum
    /// </summary>
    private static ShellType ParseShellType(string shellString)
    {
        if (Enum.TryParse<ShellType>(shellString, out var shellType))
        {
            return shellType;
        }

        // Default to PowerShell if parsing fails
        return ShellType.PowerShell;
    }
}
