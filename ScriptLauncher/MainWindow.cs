using ScriptLauncher.Services;
using ScriptLauncher.Utilities;
using ScriptLauncher.Views;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ScriptCommand = ScriptLauncher.Models.Command;

namespace ScriptLauncher;

public class MainWindow : Runnable<string?>
{
    private CommandService _commandService = null!;
    private ProcessService _processService = null!;
    private List<ScriptCommand> _commands = new();
    private CommandListView _commandListView = null!;
    private CommandDetailView _commandDetailView = null!;
    private ButtonBarView _buttonBar = null!;
    private Label _statusLabel = null!;
    private Label _titleLabel = null!;

    public MainWindow()
    {
        Title = "Script Launcher";
        InitializeServices();
        InitializeUi();
        LoadCommands();
    }

    private void InitializeServices()
    {
        IniFileHelper.EnsureIniFileExists();
        _commandService = new CommandService();
        _processService = new ProcessService();
    }

    private void InitializeUi()
    {
        _titleLabel = new Label
        {
            Text = "═══ Script Launcher ═══",
            X = Pos.Center(),
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            TextAlignment = Alignment.Center,
            Visible = true
        };
        // Create horizontal split view for top section
        var splitView = new View
        {
            X = 0,
            Y = Pos.Bottom(_titleLabel) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
        };

        // Command list view (left panel) with border
        var leftPanel = new FrameView
        {
            Title = "Commands",
            X = 0,
            Y = 0,
            Width = Dim.Percent(40),
            Height = Dim.Fill(),
            CanFocus = false
        };
        _commandListView = new CommandListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _commandListView.SelectionChanged += OnSelectionChanged;
        _commandListView.CommandActivated += OnCommandActivated;
        leftPanel.Add(_commandListView);

        // Command detail view (right panel) with border
        var rightPanel = new FrameView
        {
            Title = "Command Details",
            X = Pos.Right(leftPanel),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = false
        };
        _commandDetailView = new CommandDetailView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        rightPanel.Add(_commandDetailView);

        splitView.Add(leftPanel, rightPanel);

        // Button bar
        var buttonBarY = Pos.Bottom(splitView);
        _buttonBar = new ButtonBarView
        {
            X = Pos.Center(),
            Y = buttonBarY,
            Width = Dim.Fill(),
            Height = 1
        };

        _buttonBar.RunClicked += (_, _) => OnRun();
        _buttonBar.AddClicked += (_, _) => OnAdd();
        _buttonBar.EditClicked += (_, _) => OnEdit();
        _buttonBar.DeleteClicked += (_, _) => OnDelete();
        _buttonBar.QuitClicked += (_, _) => OnQuit();

        // Status bar
        _statusLabel = new Label
        {
            Text = "Ready",
            X = 0,
            Y = Pos.Bottom(_buttonBar),
            Width = Dim.Fill(),
            Height = 1,
            Visible = true,
            TextAlignment = Alignment.Start
        };

        Add(_titleLabel, splitView, _buttonBar, _statusLabel);

        // Set initial focus to command list
        _commandListView.FocusList();

        // Handle Q key to quit
        KeyDown += (_, key) =>
        {
            if (key == Key.Q)
            {
                OnQuit();
            }
        };
    }

    private void LoadCommands()
    {
        _commands = _commandService.LoadCommands();
        _commandListView.SetCommands(_commands);
        UpdateStatus($"Loaded {_commands.Count} commands");
        UpdateButtonStates();
    }

    private void OnSelectionChanged(object? _, ScriptCommand? command)
    {
        _commandDetailView.SetCommand(command);
        UpdateButtonStates();
    }

    private void OnCommandActivated(object? _, ScriptCommand? command)
    {
        if (command != null)
        {
            ExecuteCommand(command);
        }
    }

    private void OnRun()
    {
        var command = _commandListView.GetSelectedCommand();
        if (command != null)
        {
            ExecuteCommand(command);
        }
    }

    private void OnAdd()
    {
        var dialog = new AddEditDialog();
        var result = ShowAddEditDialog(dialog);

        if (result != null)
        {
            _commandService.AddCommand(result);
            LoadCommands();
            UpdateStatus("Command added successfully");
        }
    }

    private void OnEdit()
    {
        var command = _commandListView.GetSelectedCommand();
        if (command == null)
        {
            UpdateStatus("No command selected to edit");
            return;
        }

        var dialog = new AddEditDialog(command);
        var result = ShowAddEditDialog(dialog);

        if (result != null)
        {
            _commandService.UpdateCommand(command, result);
            LoadCommands();
            UpdateStatus("Command updated successfully");
        }
    }

    private void OnDelete()
    {
        var command = _commandListView.GetSelectedCommand();
        if (command == null)
        {
            UpdateStatus("No command selected to delete");
            return;
        }

        // Simple console-based confirmation
        Console.WriteLine($"Delete command '{command.Name}'? (y/n)");
        // For now, just delete without confirmation since we can't easily do modal dialogs in v2
        _commandService.DeleteCommand(command);
        LoadCommands();
        UpdateStatus($"Command '{command.Name}' deleted");
    }

    private void OnQuit()
    {
        // Properly stop the application by requesting to stop this runnable
        RequestStop();
    }

    private void ExecuteCommand(ScriptCommand command)
    {
        UpdateStatus($"Running: {command.Name}...");
        _processService.ExecuteCommand(command);
        UpdateStatus($"Command '{command.Name}' executed");
    }

    private void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _commandListView.GetSelectedCommand() != null;
        _buttonBar.UpdateButtonStates(hasSelection);
    }

    private ScriptCommand? ShowAddEditDialog(AddEditDialog dialog)
    {
        // Position the dialog
        dialog.X = Pos.Center();
        dialog.Y = Pos.Center();

        // Set initial focus to name field
        dialog.SetInitialFocus();

        // Use the Application instance from Program.cs to run the dialog modally
        // This is the Terminal.Gui v2 instance-based pattern
        if (Globals.Application == null)
        {
            throw new InvalidOperationException("Application instance not available. " +
                "Ensure Program.cs sets Globals.Application before running MainWindow.");
        }

        Globals.Application.Run(dialog);

        // Get the result from the dialog's Data property
        return dialog.Data as ScriptCommand;
    }
}
