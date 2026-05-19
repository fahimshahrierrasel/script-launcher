using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using ScriptLauncher.Models;
using Terminal.Gui;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using ScriptCommand = ScriptLauncher.Models.Command;

namespace ScriptLauncher.Views;

[ExcludeFromCodeCoverage]
public class CommandListView : View
{
    private ListView _listView = null!;
    private List<ScriptCommand> _commands = new();
    private bool _disposed = false;

    /// <summary>
    /// Event raised when the selected command changes
    /// </summary>
    public event EventHandler<ScriptCommand?>? SelectionChanged;

    /// <summary>
    /// Event raised when a command is activated (Enter key)
    /// </summary>
    public event EventHandler<ScriptCommand?>? CommandActivated;

    public CommandListView()
    {
        InitializeComponents();
    }

    public void FocusList()
    {
        SetFocus();
        _listView.SetFocus();
    }

    private void InitializeComponents()
    {
        CanFocus = true;

        _listView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Visible = true,
            CanFocus = true
        };

        _listView.ValueChanged += OnListViewValueChanged;
        _listView.KeyDown += OnListViewKeyDown;

        Add(_listView);
    }

    /// <summary>
    /// Sets the commands to display in the list
    /// </summary>
    public void SetCommands(List<ScriptCommand> commands)
    {
        _commands = commands;
        var source = new ObservableCollection<string>(commands.Select(c => c.Name));
        _listView.SetSource<string>(source);

        // Raise selection changed event
        var selectedCommand = GetSelectedCommand();
        SelectionChanged?.Invoke(this, selectedCommand);
    }

    /// <summary>
    /// Gets the currently selected command
    /// </summary>
    public ScriptCommand? GetSelectedCommand()
    {
        if (_listView.SelectedItem.HasValue && _listView.SelectedItem.Value >= 0 && _listView.SelectedItem.Value < _commands.Count)
        {
            return _commands[_listView.SelectedItem.Value];
        }
        return null;
    }

    /// <summary>
    /// Gets the index of the currently selected command
    /// </summary>
    public int GetSelectedIndex()
    {
        if (_listView.SelectedItem.HasValue && _listView.SelectedItem.Value >= 0 && _listView.SelectedItem.Value < _commands.Count)
        {
            return _listView.SelectedItem.Value;
        }
        return -1;
    }

    /// <summary>
    /// Sets the selected command by index
    /// </summary>
    public void SetSelectedIndex(int index)
    {
        if (index >= 0 && index < _commands.Count)
        {
            _listView.SelectedItem = index;
        }
    }

    /// <summary>
    /// Clears the command selection
    /// </summary>
    public void ClearSelection()
    {
        _listView.SelectedItem = null;
    }

    private void OnListViewValueChanged(object? _, object? __)
    {
        var selectedCommand = GetSelectedCommand();
        SelectionChanged?.Invoke(this, selectedCommand);
    }

    private void OnListViewKeyDown(object? _, Key key)
    {
        if (key == Key.Enter)
        {
            var selectedCommand = GetSelectedCommand();
            CommandActivated?.Invoke(this, selectedCommand);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _listView != null)
            {
                // Unsubscribe from event handlers
                _listView.ValueChanged -= OnListViewValueChanged;
                _listView.KeyDown -= OnListViewKeyDown;
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}
