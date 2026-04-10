# Edit‑Configuration Feature Plan

## Overview

This document tracks the implementation steps for adding an edit‑mode UI that allows users to modify the command configuration at runtime.

## Tasks

1. **Data Model** – Define `CommandGroup` and `CommandProcess` classes.
2. **Repository** – Implement `CommandDefinitionsRepository` for JSON persistence.
3. **ViewModel** – Create `EditViewModel` with edit‑mode commands.
4. **UI** – Add XAML views and toolbar toggle.
5. **Unit Tests** – Add NUnit tests for repository and ViewModel.
6. **Documentation** – Update README and config schema.
7. **CI** – Add NUnit test step.

## Status

- [ ] Data Model
- [ ] Repository
- [ ] ViewModel
- [ ] UI
- [ ] Unit Tests
- [ ] Documentation
- [ ] CI

## Implementation Plan – Edit‑Mode UI & Unit Tests

| # | Task | Owner | Status | Notes |
|---|------|-------|--------|-------|
| 1 | **Define data model for groups & processes** | Core | Not‑started | Create `CommandGroup` (Id, Name, Children, Processes) and `CommandProcess` (Id, Name, Path, Args). Add to `ML.ApplicationLauncher.Source` for shared use. |
| 2 | **Persist changes to JSON** | Core | Not‑started | Extend `CommandDefinitions.json` schema to include `IsEditable` flag. Implement `CommandDefinitionsRepository` with `Load`, `Save`, `AddGroup`, `RemoveGroup`, `AddProcess`, `RemoveProcess`, `Reorder`. |
| 3 | **Add ViewModel for Edit Mode** | Shell | Not‑started | `EditViewModel` exposing `ObservableCollection<CommandGroupViewModel>`, `SelectedItem`, `IsEditMode`. Commands: `ToggleEditCommand`, `AddGroupCommand`, `AddProcessCommand`, `RemoveCommand`, `MoveCommand`, `SaveCommand`. |
| 4 | **Create Edit View (XAML)** | Shell | Not‑started | Replace main view with a `Grid` that shows either *Read‑Only* or *Edit* mode. Use `TreeView` for groups, `ListView` for processes. Bind to `EditViewModel`. |
| 5 | **Toolbar toggle** | Shell | Not‑started | Add `ToggleButton` bound to `ToggleEditCommand`. Change icon/text when toggled. |
| 6 | **Right‑hand detail pane** | Shell | Not‑started | Show `GroupDetailView` or `ProcessDetailView` depending on `SelectedItem`. Allow editing of properties. |
| 7 | **Reorder logic** | Shell | Not‑started | Implement drag‑and‑drop or up/down buttons. Update underlying collections and persist order. |
| 8 | **Unit tests (NUnit)** | Tests | Not‑started | Create `ML.ApplicationLauncher.Tests` project. Tests: |
|   8.1 | `CommandDefinitionsRepository` – Load/Save round‑trip, Add/Remove/Move operations. |
|   8.2 | `EditViewModel` – ToggleEdit, AddGroup, AddProcess, Remove, Move, Save. |
|   8.3 | Validation – ensure invalid paths are rejected. |
| 9 | **Integration tests** | Tests | Not‑started | Use `WPF` test harness (e.g., `Microsoft.VisualStudio.TestTools.UnitTesting` with `Dispatcher`) to verify UI toggles and data binding. |
|10 | **Documentation** | Docs | Not‑started | Update README with new Edit mode description, key shortcuts, and config schema changes. |
|11 | **CI pipeline update** | DevOps | Not‑started | Add NUnit test step to existing build script. |
|12 | **Code review & refactor** | Team | Not‑started | Ensure adherence to Copilot CS instructions (naming, XML docs, async patterns). |

### Key Deliverables

1. **Data Model & Repository** – `CommandGroup`, `CommandProcess`, `CommandDefinitionsRepository`.
2. **Edit Mode UI** – XAML views, ViewModel, toolbar toggle.
3. **Unit Test Project** – NUnit tests covering repository and ViewModel logic.
4. **Updated Config Schema** – `CommandDefinitions.json` with edit‑mode fields.
5. **Documentation & CI** – README updates, test integration.

### Next Steps

- Create the data model classes in `ML.ApplicationLauncher.Source`.
- Implement the repository with JSON serialization.
- Build the ViewModel and XAML for edit mode.
- Add NUnit test project and write tests.
- Run CI to ensure all tests pass.
This plan covers the architectural changes, UI implementation, persistence, and testing required to add the requested edit‑mode functionality.
