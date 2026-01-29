using Terminal.Gui.App;

namespace ScriptLauncher;

/// <summary>
/// Global application state
/// TODO: Remove this when Terminal.Gui v2 finalizes the modal dialog API pattern
/// </summary>
public static class Globals
{
    /// <summary>
    /// The Application instance created in Program.cs
    /// Needed for running modal dialogs from within MainWindow in Terminal.Gui v2 alpha
    /// </summary>
    public static IApplication? Application { get; set; }
}
