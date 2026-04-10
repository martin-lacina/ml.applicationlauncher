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

## Contribution Notes

- This is a quick-hack tool; prioritize pragmatic, working solutions over architectural purity.
- Follow the structure and conventions as found—do not introduce major refactors unless requested.
- For detailed C# coding standards and preferences, see [instructions/Copilot_cs.instructions.md](instructions/Copilot_cs.instructions.md).
