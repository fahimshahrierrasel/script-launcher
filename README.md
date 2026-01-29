# Script Runner TUI

A cross-platform Terminal User Interface (TUI) application built with .NET and Terminal.Gui that allows users to manage and execute scripts or shell commands from a text-based interface.

The application supports:
- PowerShell commands
- Batch scripts
- Bash commands
- Any terminal command supported by the operating system

All command definitions are stored in a portable INI file so the tool can be copied and used anywhere.

---

## Features

- Terminal-based UI (TUI)
- Add new scripts or commands
- Edit existing commands
- Delete commands
- Run commands by selecting them
- Specify working directory per command
- Choose shell type per command
- Store all configuration in a portable INI file
- Cross-platform (Windows, Linux, macOS)

---

## Technology Stack
1.  **Framework:** .NET 10 (Console Application).
2.  **UI Library:** `Terminal.Gui` (version 2.x or latest).
   - Documentation:
     - https://gui-cs.github.io/Terminal.Gui/docs/getting_started.html
     - https://gui-cs.github.io/Terminal.Gui/docs/views.html
     - https://gui-cs.github.io/Terminal.Gui/docs/application.html
3.  **INI Handling:** Use `IniParser` NuGet package.
4.  **Process Logic:** Use `System.Diagnostics.Process`.

---

## Command Model

Each command entry contains:
- Name (displayed in the list)
- Shell type (powershell, cmd, bash)
- Working directory
- Command text

---

## Example INI File

```ini
[Command1]
Name=Build Project
Shell=powershell
WorkingDirectory=C:\Projects\MyApp
Command=dotnet build

[Command2]
Name=Start Server
Shell=bash
WorkingDirectory=/home/user/project
Command=dotnet run
```
## How It Works

1. App loads commands from commands.ini on startup
2. Commands are shown in a selectable list
3. User can:
   - Press "Add" to create a new command 
   - Press "Edit" to modify a command 
   - Press "Delete" to remove a command 
   - Press "Run" to execute selected command 
4. Changes are saved immediately to the INI file

## Execution Logic
### Windows
- PowerShell → `powershell -Command`
- CMD → `cmd.exe /c`

### Linux/macOS
- Bash → `/bin/bash -c`

The working directory is applied before execution.

## UI Layout (Concept)
- Left panel: List of commands
- Right panel: Command details
- Bottom: Buttons (Run, Add, Edit, Delete, Quit)
