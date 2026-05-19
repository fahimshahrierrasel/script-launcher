using System.Diagnostics.CodeAnalysis;
using Terminal.Gui.App;

namespace ScriptLauncher;

[ExcludeFromCodeCoverage]
public static class Globals
{
    /// <summary>
    /// The Application instance created in Program.cs
    /// Needed for running modal dialogs from within MainWindow in Terminal.Gui v2 alpha
    /// </summary>
    public static IApplication? Application { get; set; }
}
