using ScriptLauncher;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;

ConfigurationManager.Enable (ConfigLocations.All);


var app = Application.Create();
app.Init();

// Store the app instance globally so MainWindow can access it for modal dialogs
// This is a workaround for Terminal.Gui v2 alpha where the modal dialog pattern is not yet finalized
Globals.Application = app;

try
{
    app.Run<MainWindow>();
}
finally
{
    app.Dispose();
    Globals.Application = null;
}