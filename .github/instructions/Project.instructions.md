# Copilot Instructions for ML.ApplicationLauncher

## Project Overview

- **ML.ApplicationLauncher** is a WPF desktop app for Windows that launches commands/processes via a button-based UI, configured by JSON files in the executable directory.
- The solution is split into multiple projects:
  - `ML.ApplicationLauncher.Shell` (main WPF app)
  - `ML.ApplicationLauncher.Shell.Admin` (admin-elevated variant)
  - `ML.ApplicationLauncher.Core` (core logic, validation)
  - `ML.ApplicationLauncher.Shell.Shared` (shared WPF resources, controls, extensions)
  - `ML.ApplicationLauncher.Source` (source/configuration logic)
  - `ML.ApplicationLauncher.Shell.Assets` (resources, build targets)

## Configuration Patterns

- **CommandDefinitions.json**: Main user config for launchable commands. Use `CommandDefinitions.examples.json` as a template/fallback.
- **appsettings.json**: App config, fallback to `appsettings.template.json` if missing.
- Both config files are expected in the executable directory. The app will not load malformed JSON (manual fix required).

## Build & Run

- Use the provided solution file: `ML.ApplicationLauncher.slnx`.
- Build and publish scripts: `Publish-Debug.cmd`, `Publish-Release.cmd`.
- No unit tests are present (as noted in the main README), need to be added.
- Main entry points: `ML.ApplicationLauncher.Shell` and `ML.ApplicationLauncher.Shell.Admin`.

## UI & UX

- Main window displays the config file name and admin status.
- Toolbar buttons: Exit, Reload, Edit, Clear, About (see README for details).
- Groups and Processes are rendered from the config as vertical stacks and buttons.

## Coding Patterns & Conventions

- **Validation**: Uses `ShouldNotBeNull()` extension (from `ML.ApplicationLauncher.Core.Validation`).
- **WPF Extensions**: See `Extensions/HyperlinkExtensions.cs` for attached property patterns (e.g., `IsExternal` for hyperlinks).
- **Resource Sharing**: Shared XAML/resources in `Shell.Shared`.
- **No tests**: Do not expect or require test coverage.
- **Error Handling**: App disables buttons for missing executables unless `ExecutionMode` is `Raw`.

## External Integration

- Minimal external dependencies; see `nuget.config` and `packages/` for details.
- Uses standard .NET/WPF libraries and conventions unless otherwise noted.

## Examples

- See `CommandDefinitions.examples.json` for config structure.
- See `appsettings.template.json` for app settings structure.
- See `Extensions/HyperlinkExtensions.cs` for WPF attached property usage.

## Workflow: Todo List Management

**Always use the todo list to track multi-step work.** When starting any task that involves more than one step:

1. **Create or update the todo list** using `manage_todo_list` before beginning work — each item should be a specific, actionable step.
2. **Mark items as `in-progress`** before starting work on them, and **mark as `completed`** immediately after finishing each item — do not batch completions.
3. **Report status at the end of every session** — after completing all work, summarize what was done and explicitly state which todo items remain (if any). If all items are complete, say so clearly.
4. **Resume from the todo list** when continuing a previous session — check what's incomplete and pick up where you left off without asking the user.

This ensures the user never has to ask "any items left?" — the assistant always proactively reports remaining work.

## Contribution Notes

- This is a quick-hack tool; prioritize pragmatic, working solutions over architectural purity.
- Follow the structure and conventions as found—do not introduce major refactors unless requested.
- For detailed C# coding standards and preferences, see [instructions/Copilot_cs.instructions.md](Copilot_cs.instructions.md).
