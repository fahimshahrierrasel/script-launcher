using System.Diagnostics.CodeAnalysis;
using Terminal.Gui;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ScriptLauncher.Views;

[ExcludeFromCodeCoverage]
public class ButtonBarView : View
{
    private bool _disposed = false;
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

    private static void MarkHandled(CommandEventArgs e)
    {
        e.Handled = true;
    }

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
        _runButton.Accepting += OnRunButtonAccepting;

        _addButton = new Button
        {
            Text = "Add",
            X = Pos.Right(_runButton) + 1,
            Y = 0,
            Width = 15
        };
        _addButton.Accepting += OnAddButtonAccepting;

        _editButton = new Button
        {
            Text = "Edit",
            X = Pos.Right(_addButton) + 1,
            Y = 0,
            Width = 15
        };
        _editButton.Accepting += OnEditButtonAccepting;

        _deleteButton = new Button
        {
            Text = "Delete",
            X = Pos.Right(_editButton) + 1,
            Y = 0,
            Width = 15
        };
        _deleteButton.Accepting += OnDeleteButtonAccepting;

        _quitButton = new Button
        {
            Text = "Quit",
            X = Pos.Right(_deleteButton) + 1,
            Y = 0,
            Width = 15
        };
        _quitButton.Accepting += OnQuitButtonAccepting;

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

    private void OnRunButtonAccepting(object? _, CommandEventArgs e)
    {
        RunClicked?.Invoke(this, EventArgs.Empty);
        MarkHandled(e);
    }

    private void OnAddButtonAccepting(object? _, CommandEventArgs e)
    {
        AddClicked?.Invoke(this, EventArgs.Empty);
        MarkHandled(e);
    }

    private void OnEditButtonAccepting(object? _, CommandEventArgs e)
    {
        EditClicked?.Invoke(this, EventArgs.Empty);
        MarkHandled(e);
    }

    private void OnDeleteButtonAccepting(object? _, CommandEventArgs e)
    {
        DeleteClicked?.Invoke(this, EventArgs.Empty);
        MarkHandled(e);
    }

    private void OnQuitButtonAccepting(object? _, CommandEventArgs e)
    {
        QuitClicked?.Invoke(this, EventArgs.Empty);
        MarkHandled(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Unsubscribe from all button events
                if (_runButton != null) _runButton.Accepting -= OnRunButtonAccepting;
                if (_addButton != null) _addButton.Accepting -= OnAddButtonAccepting;
                if (_editButton != null) _editButton.Accepting -= OnEditButtonAccepting;
                if (_deleteButton != null) _deleteButton.Accepting -= OnDeleteButtonAccepting;
                if (_quitButton != null) _quitButton.Accepting -= OnQuitButtonAccepting;
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}
