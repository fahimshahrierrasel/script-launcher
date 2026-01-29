using ScriptLauncher.Models;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ScriptLauncher.Views;

/// <summary>
/// Right panel component displaying command details
/// </summary>
public class CommandDetailView : View
{
    private Label _nameLabel = null!;
    private Label _shellLabel = null!;
    private Label _directoryLabel = null!;
    private Label _commandLabel = null!;
    private Label _emptyStateLabel = null!;

    public CommandDetailView()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        CanFocus = true;

        // Empty state label
        _emptyStateLabel = new Label
        {
            Text = "No command selected.\nSelect a command to view details.",
            X = Pos.Center(),
            Y = Pos.Center(),
            VerticalTextAlignment = Alignment.Center,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Visible = true
        };

        // Name field label
        var nameLabel = new Label
        {
            Text = "Name:",
            X = 0,
            Y = 0,
            Width = 10
        };

        _nameLabel = new Label
        {
            X = 11,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            Visible = false
        };

        // Shell field label
        var shellLabel = new Label
        {
            Text = "Shell:",
            X = 0,
            Y = 2,
            Width = 10
        };

        _shellLabel = new Label
        {
            X = 11,
            Y = 2,
            Width = Dim.Fill(),
            Height = 1,
            Visible = false
        };

        // Working directory label
        var dirLabel = new Label
        {
            Text = "Directory:",
            X = 0,
            Y = 4,
            Width = 10
        };

        _directoryLabel = new Label
        {
            X = 11,
            Y = 4,
            Width = Dim.Fill(),
            Height = 1,
            Visible = false
        };

        // Command text label
        var cmdTextLabel = new Label
        {
            Text = "Command:",
            X = 0,
            Y = 6,
            Width = 10
        };

        _commandLabel = new Label
        {
            X = 11,
            Y = 6,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            Visible = false
        };

        Add(_emptyStateLabel, nameLabel, _nameLabel, shellLabel, _shellLabel,
            dirLabel, _directoryLabel, cmdTextLabel, _commandLabel);
    }

    /// <summary>
    /// Sets the command to display, or null to show empty state
    /// </summary>
    public void SetCommand(Command? command)
    {
        if (command == null)
        {
            _emptyStateLabel.Visible = true;
            _nameLabel.Visible = false;
            _shellLabel.Visible = false;
            _directoryLabel.Visible = false;
            _commandLabel.Visible = false;
        }
        else
        {
            _emptyStateLabel.Visible = false;
            _nameLabel.Visible = true;
            _shellLabel.Visible = true;
            _directoryLabel.Visible = true;
            _commandLabel.Visible = true;

            _nameLabel.Text = command.Name;
            _shellLabel.Text = command.ShellType.ToString();
            _directoryLabel.Text = string.IsNullOrWhiteSpace(command.WorkingDirectory)
                ? "(Current directory)"
                : command.WorkingDirectory;
            _commandLabel.Text = command.CommandText;
        }

        // In v2, text changes automatically trigger redraw
    }
}
