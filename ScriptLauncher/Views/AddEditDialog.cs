 using System.Collections.ObjectModel;
 using ScriptLauncher.Models;
 using Terminal.Gui.ViewBase;
 using Terminal.Gui.Views;
 using Terminal.Gui;
 using ScriptCommand = ScriptLauncher.Models.Command;

namespace ScriptLauncher.Views;

/// <summary>
/// Modal dialog for adding or editing commands
/// </summary>
 public class AddEditDialog : Dialog
 {
     private TextField _nameField = null!;
     private ListView _shellListView = null!;
     private TextField _directoryField = null!;
     private TextView _commandText = null!;
     private Button _browseButton = null!;
     private Button _saveButton = null!;
     private Button _cancelButton = null!;
     private ScriptCommand? _originalCommand;

    /// <summary>
    /// Creates a new dialog for adding a command
    /// </summary>
    public AddEditDialog()
    {
        Initialize();
    }

    /// <summary>
    /// Creates a dialog for editing an existing command
    /// </summary>
    /// <param name="command">Command to edit</param>
    public AddEditDialog(ScriptCommand command)
    {
        _originalCommand = command;
        Initialize();
        LoadCommand(command);
    }

    private void Initialize()
    {
        Title = _originalCommand == null ? "Add Command" : "Edit Command";

        // Name field
        var nameLabel = new Label
        {
            Text = "Name:",
            X = 0,
            Y = 0,
            Width = 15
        };

        _nameField = new TextField
        {
            X = 16,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1
        };

        // Shell type selection (using ListView instead of obsolete ComboBox)
        var shellLabel = new Label
        {
            Text = "Shell:",
            X = 0,
            Y = 2,
            Width = 15
        };

        var shellTypes = new List<string>(Enum.GetNames<ShellType>());
        _shellListView = new ListView
        {
            X = 16,
            Y = 2,
            Width = Dim.Fill(),
            Height = 3 // Show first 3 shell types
        };
        _shellListView.SetSource<string>(new ObservableCollection<string>(shellTypes));

        // Working directory
        var dirLabel = new Label
        {
            Text = "Directory:",
            X = 0,
            Y = 6,
            Width = 15
        };

        _directoryField = new TextField
        {
            X = 16,
            Y = 6,
            Width = Dim.Fill(20), // Leave room for browse button
            Height = 1
        };

        _browseButton = new Button
        {
            Text = "Browse...",
            X = Pos.Right(_directoryField) + 1,
            Y = 6,
            Width = 15
        };
        _browseButton.Accepting += (sender, e) =>
        {
            OnBrowse();
            e.Handled = true;
        };

        // Command text (multi-line)
        var cmdLabel = new Label
        {
            Text = "Command:",
            X = 0,
            Y = 8,
            Width = 15
        };

        _commandText = new TextView
        {
            X = 16,
            Y = 8,
            Width = Dim.Fill(),
            Height = Dim.Fill(3) // Leave room for buttons
        };

        // Buttons
        _saveButton = new Button
        {
            Text = "Save",
            X = Pos.Center() - 10,
            Y = Pos.Bottom(_commandText) + 1,
            Width = 15
        };

        _cancelButton = new Button
        {
            Text = "Cancel",
            X = Pos.Center() + 1,
            Y = Pos.Bottom(_commandText) + 1,
            Width = 15
        };

        _saveButton.Accepting += (sender, e) =>
        {
            OnSave();
            e.Handled = true;
        };

        _cancelButton.Accepting += (sender, e) =>
        {
            OnCancel();
            e.Handled = true;
        };

         Add(nameLabel, _nameField, shellLabel, _shellListView,
             dirLabel, _directoryField, _browseButton, cmdLabel, _commandText,
             _saveButton, _cancelButton);

        Width = Dim.Percent(80);
        Height = Dim.Percent(60);
    }

    /// <summary>
    /// Sets initial focus to the name field when the dialog is displayed
    /// </summary>
    public void SetInitialFocus()
    {
        _nameField.SetFocus();
    }

    private void LoadCommand(ScriptCommand command)
    {
        _nameField.Text = command.Name;
        var shellTypes = Enum.GetNames<ShellType>().ToList();
        _shellListView.SetSource<string>(new ObservableCollection<string>(shellTypes));
        _shellListView.SelectedItem = shellTypes.IndexOf(command.ShellType.ToString());
        _directoryField.Text = command.WorkingDirectory;
        _commandText.Text = command.CommandText;
    }

     private void OnSave()
     {
         // Validate
         if (string.IsNullOrWhiteSpace(_nameField.Text))
         {
             _nameField.SetFocus();
             return;
         }

         if (string.IsNullOrWhiteSpace(_commandText.Text))
         {
             _commandText.SetFocus();
             return;
         }

         var shellTypes = Enum.GetNames<ShellType>().ToList();
         var selectedShell = _shellListView.SelectedItem.HasValue
             ? shellTypes[_shellListView.SelectedItem.Value]
             : "PowerShell";

         // Store the result in the Data property (inherited from View)
         Data = new ScriptCommand
         {
             Name = _nameField.Text.Trim(),
             ShellType = Enum.Parse<ShellType>(selectedShell),
             WorkingDirectory = _directoryField.Text.Trim(),
             CommandText = _commandText.Text
         };

         // Close dialog by requesting to stop
         RequestStop();
     }

     private void OnBrowse()
     {
         var dialog = new OpenDialog
         {
             Title = "Select Working Directory",
             OpenMode = OpenMode.Directory
         };

         // Use the Application instance from Program.cs to run the dialog
         if (Globals.Application == null)
         {
             return;
         }

         Globals.Application.Run(dialog);

         // Check if user selected a directory
         if (dialog.FilePaths.Count > 0)
         {
             var selectedPath = dialog.FilePaths[0].ToString();
             if (System.IO.Directory.Exists(selectedPath))
             {
                 _directoryField.Text = selectedPath;
             }
         }
     }

    private void OnCancel()
    {
        // Set Data to null to indicate cancellation
        Data = null;
        // Close dialog
        RequestStop();
    }
}
