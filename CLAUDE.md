# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a cross-platform Terminal User Interface (TUI) application built with .NET 10 and Terminal.Gui v2 (alpha). It provides a terminal-based interface for managing and executing scripts/shell commands with configuration stored in a portable INI file.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project "Script Launcher/Script Launcher.csproj"

# Build and run in one command
dotnet run
```

## Architecture

### Application Structure

The application follows Terminal.Gui v2 patterns:

- **Program.cs**: Entry point that initializes the application with theme configuration ("Amber Phosphor") and creates the main application loop
- **MainWindow.cs**: The main view class inheriting from `Runnable<string?>` - this is where the UI will be implemented
- **Namespace**: `Script_Launcher` (note the underscore due to C# naming restrictions)

### Key Technologies

- **Terminal.Gui v2**: Version `2.0.0-alpha.4146` - note this is an alpha version with API differences from v1
  - Documentation: 
    - https://gui-cs.github.io/Terminal.Gui/docs/getting_started.html
    - https://gui-cs.github.io/Terminal.Gui/docs/views.html
    - https://gui-cs.github.io/Terminal.Gui/docs/application.html
- **.NET 10**: Uses `ImplicitUsings`, `Nullable` enabled, and `latestmajor` LangVersion
- **Configuration Manager**: Used for theme initialization - `ConfigurationManager.RuntimeConfig` and `ConfigurationManager.Enable(ConfigLocations.All)`

### Planned Command Model

Commands will be stored in `commands.ini` with the following structure per command:
- Name: Display name in the list
- Shell: powershell, cmd, or bash
- WorkingDirectory: Execution path
- Command: The actual command/text to execute

### UI Layout Plan

- Left panel: Command list view
- Right panel: Command details
- Bottom: Action buttons (Run, Add, Edit, Delete, Quit)

### Execution Model

Commands will be executed using `System.Diagnostics.Process` with platform-specific shell invocation:
- Windows: PowerShell (`powershell -Command`), CMD (`cmd.exe /c`)
- Linux/macOS: Bash (`/bin/bash -c`)

## Implementation Notes

- The project is in early stages - MainWindow.cs is a stub that needs full implementation
- Terminal.Gui v2 uses `Application.Create()` pattern rather than static Application class
- Theme is configured via JSON configuration string before app initialization
- INI file parsing will need to be implemented (consider IniParser package mentioned in README)

