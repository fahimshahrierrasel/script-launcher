using ScriptLauncher.Models;

namespace ScriptLauncher.Services;

/// <summary>
/// Service interface for managing commands in the INI file
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Loads all commands from the INI file
    /// </summary>
    /// <returns>List of commands</returns>
    List<Command> LoadCommands();

    /// <summary>
    /// Saves all commands to the INI file
    /// </summary>
    /// <param name="commands">List of commands to save</param>
    void SaveCommands(List<Command> commands);

    /// <summary>
    /// Adds a new command to the INI file
    /// </summary>
    /// <param name="command">Command to add</param>
    void AddCommand(Command command);

    /// <summary>
    /// Updates an existing command in the INI file
    /// </summary>
    /// <param name="index">Zero-based index of the command to update</param>
    /// <param name="newCommand">New command data</param>
    void UpdateCommand(int index, Command newCommand);

    /// <summary>
    /// Deletes a command from the INI file
    /// </summary>
    /// <param name="index">Zero-based index of the command to delete</param>
    void DeleteCommand(int index);
}
