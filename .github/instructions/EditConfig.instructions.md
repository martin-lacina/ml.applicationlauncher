# Edit‑Configuration Feature Instructions

## Purpose
These instructions guide the Copilot agent when implementing the **Edit‑Configuration** feature. They capture the user’s preferences and project conventions that should be followed throughout the implementation.

## Scope
- Applies to all C# source files in the *ML.ApplicationLauncher* solution.
- Applies to XAML files in the *ML.ApplicationLauncher.Shell* project.
- Applies to test projects that use **NUnit**.

## Coding Conventions
1. **Naming** – Follow the Copilot CS instructions (PascalCase for types, camelCase for local variables, underscore prefix for private fields).
2. **Async** – Use `async`/`await` for I/O operations (e.g., JSON file read/write). Do not block the UI thread.
3. **MVVM** – Separate UI logic into ViewModels. Use `ObservableCollection<T>` for collections that the UI binds to.
4. **JSON Persistence** – Use `System.Text.Json` with `JsonSerializerOptions` that preserve property names and ignore nulls.
5. **Unit Tests** – Use NUnit. Follow Arrange‑Act‑Assert pattern. Use Moq for dependencies.
6. **Error Handling** – Throw exceptions for invalid data; catch specific exceptions where appropriate.
7. **Documentation** – Provide XML documentation for public APIs.
8. **Tooling** – Use `dotnet test` for running tests.

## Feature‑Specific Rules
- **Data Model** – Define `CommandGroup` and `CommandProcess` in `ML.ApplicationLauncher.Source`. They must be serializable to JSON.
- **Repository** – `CommandDefinitionsRepository` must expose `LoadAsync`, `SaveAsync`, `AddGroupAsync`, `RemoveGroupAsync`, `AddProcessAsync`, `RemoveProcessAsync`, `ReorderAsync`.
- **ViewModel** – `EditViewModel` must expose `ObservableCollection<CommandGroupViewModel> Groups`, `CommandGroupViewModel SelectedItem`, `bool IsEditMode`, and commands for toggling edit mode, adding/removing/moving items, and saving.
- **UI** – The main window should switch between read‑only and edit views based on `IsEditMode`. The toolbar must contain a toggle button bound to `ToggleEditCommand`.
- **Tests** – Create a test project `ML.ApplicationLauncher.Tests`. Include tests for repository round‑trip, ViewModel command execution, and validation logic.

## Commit Strategy
- Commit after each major change (e.g., data model, repository, ViewModel, UI, tests).
- Use descriptive commit messages such as "Add CommandGroup model" or "Implement EditViewModel".

## Example Prompt
> *Create a new `CommandGroup` class with properties `Id`, `Name`, `Children`, and `Processes`. Ensure it is serializable to JSON and follows the naming conventions above.*

## Related Customizations
- `agent-customization` skill can be used to enforce these rules automatically.
- Consider adding a `dotnet-best-practices` instruction for additional guidance.

---

*These instructions are intended to be used by the Copilot agent when generating or modifying code for the Edit‑Configuration feature.*
