using Terminal.Gui;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ScriptLauncher.Views;

/// <summary>
/// Button bar view containing Run, Add, Edit, Delete, and Quit buttons
/// </summary>
public class ButtonBarView : View
{
    private Button _runButton = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _quitButton = null!;

    /// <summary>
    /// Event raised when the Run button is clicked
    /// </summary>
    public event EventHandler? RunClicked;

    /// <summary>
    /// Event raised when the Add button is clicked
    /// </summary>
    public event EventHandler? AddClicked;

    /// <summary>
    /// Event raised when the Edit button is clicked
    /// </summary>
    public event EventHandler? EditClicked;

    /// <summary>
    /// Event raised when the Delete button is clicked
    /// </summary>
    public event EventHandler? DeleteClicked;

    /// <summary>
    /// Event raised when the Quit button is clicked
    /// </summary>
    public event EventHandler? QuitClicked;

    public ButtonBarView()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _runButton = new Button
        {
            Text = "Run",
            X = 0,
            Y = 0,
            Width = 15
        };
        _runButton.Accepting += (_, e) =>
        {
            RunClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        };

        _addButton = new Button
        {
            Text = "Add",
            X = Pos.Right(_runButton) + 1,
            Y = 0,
            Width = 15
        };
        _addButton.Accepting += (_, e) =>
        {
            AddClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        };

        _editButton = new Button
        {
            Text = "Edit",
            X = Pos.Right(_addButton) + 1,
            Y = 0,
            Width = 15
        };
        _editButton.Accepting += (_, e) =>
        {
            EditClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        };

        _deleteButton = new Button
        {
            Text = "Delete",
            X = Pos.Right(_editButton) + 1,
            Y = 0,
            Width = 15
        };
        _deleteButton.Accepting += (_, e) =>
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        };

        _quitButton = new Button
        {
            Text = "Quit",
            X = Pos.Right(_deleteButton) + 1,
            Y = 0,
            Width = 15
        };
        _quitButton.Accepting += (_, e) =>
        {
            QuitClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        };

        Add(_runButton, _addButton, _editButton, _deleteButton, _quitButton);
    }
    
    /// <summary>
    /// Updates button states based on whether a command is selected
    /// </summary>
    public void UpdateButtonStates(bool hasSelection)
    {
        _runButton.Enabled = hasSelection;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }
}
