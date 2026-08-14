# Codebase Improvements Implementation Plan

> **Based on**: `plans/codebase-review-improvements.md`
> **Goal**: Systematically implement all 20 improvement suggestions across 6 phases, transforming the codebase into a maintainable, testable, production-grade project.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done,
set its status to `Complete` and write its **Phase Summary** (what was done, key
decisions, anything needed to continue with zero context); run the phase's
**Verification Plan** and record the result before moving on. When all phases are
done, fill in **Final Recap** and **Deployment Plan**.

---

## Phase 1: DI Bypass and CancellationToken Propagation
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #1 (EditViewModel DI bypass), #2 (CancellationToken gaps)
**Effort**: 2-3 days
**Impact**: Critical — enables testability and prevents hanging operations

- [ ] Create `ICommandDefinitionsRepository` interface in `ML.ApplicationLauncher.Source` exposing all public methods from `CommandDefinitionsRepository`
- [ ] Make `CommandDefinitionsRepository` implement `ICommandDefinitionsRepository`
- [ ] Register `ICommandDefinitionsRepository` as singleton in `SourceServiceInstaller.cs`
- [ ] Update `EditViewModel` constructor to accept `ICommandDefinitionsRepository` via DI instead of creating `CommandDefinitionsRepository` internally
- [ ] Remove internal `_repository` field initialization that bypasses DI
- [ ] Add `CancellationToken` parameter to `EditViewModel.SerializeGroups()` with `ThrowIfCancellationRequested()`
- [ ] Add `CancellationToken` parameter to `EditViewModel.RestoreGroupsFromJson()` with `ThrowIfCancellationRequested()`
- [ ] Add `CancellationToken` parameter to `CommandDefinitionsRepository.ValidateCircularReference()` with periodic cancellation checks in recursive traversal
- [ ] Add `CancellationToken` parameter to `ProcessGroupMapper.MapToCommandGroup()` and `MapToProcessGroup()` with cancellation checks during recursion
- [ ] Update all `EditViewModel` undo/redo calls to pass `CancellationToken` through `SerializeGroups` and `RestoreGroupsFromJson`
- [ ] Update `CommandDefinitionsRepository` public methods to accept and propagate `CancellationToken` to `ValidateCircularReference`
- [ ] Add `CancellationTokenSource` field to `EditViewModel` for cancelling all in-flight operations on cleanup
- [ ] Uncomment and fix all tests in `EditViewModelTests.cs` now that DI injection is working
- [ ] Verify all `EditViewModelTests` pass with real injected dependencies

### Analysis details

This phase addresses the two critical architectural concerns. The DI bypass in `EditViewModel` is the root cause why all unit tests are commented out — the ViewModel creates its own `CommandDefinitionsRepository` instead of receiving it from the container. Introducing an interface abstraction enables mocking and proper lifecycle management.

The CancellationToken gaps mean operations (serialization, recursive validation, recursive mapping) can't be cancelled during rapid UI interactions or window close. Adding cancellation throughout the call chain ensures responsive UI and clean shutdown.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests --filter "FullyQualifiedName~EditViewModelTests"` — all uncommented tests pass
- `dotnet test ML.ApplicationLauncher.Tests` — full test suite passes (30+ tests expected)
- Search for `new CommandDefinitionsRepository(` in ViewModels — should return zero results outside DI registration

### Phase Summary
_(write when phase completes)_

---

## Phase 2: Fire-and-Forget Tasks and UI Thread Blocking
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #3 (fire-and-forget lifecycle), #6 (UI thread blocking in DI setup)
**Effort**: 1-2 days
**Impact**: High — prevents memory leaks, unobserved exceptions, and startup deadlocks

- [ ] Add `CancellationTokenSource _backgroundCts` field to `MainWindowViewModel` for background task lifecycle
- [ ] Replace `CancellationToken.None` in `ExpireLastExecutionTimeLoopAsync` with `_backgroundCts.Token`
- [ ] Replace fire-and-forget `_ = SaveAsync()` in `EditViewModel` with tracked tasks using `.ContinueWith()` for exception handling
- [ ] Add `IAsyncDisposable` implementation to `MainWindowViewModel` for `_backgroundCts` cleanup
- [ ] Add `IAsyncDisposable` implementation to `EditViewModel` for cancellation source cleanup
- [ ] Capture `TaskScheduler.FromCurrentSynchronizationContext()` in `MainWindowViewModel` constructor for UI thread continuations
- [ ] Add error continuation on all background tasks to show errors via `_messageService.ShowError()`
- [ ] Refactor `SourceServiceInstaller.cs` to remove `Task.Run(...).GetAwaiter().GetResult()` pattern for `WindowsTerminalConfig` registration
- [ ] Replace blocking DI registration with lazy initialization or `OnStartup` async pattern in Prism shell
- [ ] Add loading indicator or splash screen during async startup initialization

### Analysis details

The fire-and-forget pattern (`_ = Task.Run(...)`) creates orphaned tasks with no exception handling and no cancellation on shutdown. The `RunSafe` wrapper helps but doesn't address lifecycle — tasks continue running after the ViewModel is disposed.

The `SourceServiceInstaller` blocks the UI thread during DI container setup by synchronously awaiting async file I/O. This can deadlock if `LoadConfigurationAsync` shows a MessageBox (creates a new message loop). Moving this to an async startup pattern eliminates the deadlock risk.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Search for `\.GetAwaiter\(\)\.GetResult\(\)` in Source project — should return zero results
- Search for `CancellationToken\.None` in ViewModel async methods — should be minimal (only where truly appropriate)
- Search for `_ = ` followed by async call in ViewModels — should have `.ContinueWith` or tracked task reference

### Phase Summary
_(write when phase completes)_

---

## Phase 3: Null Safety, JSON Performance, Launch Delay Configuration
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #4 (null-forgiving operators), #5 (JSON options allocation), #7 (hardcoded launch delay)
**Effort**: 1 day
**Impact**: High — quick wins for robustness and UX

- [ ] Replace `null!` in `CommandGroupViewModel` parameterless constructor with nullable `IProcessLauncher?` parameter
- [ ] Replace `null!` in `CommandProcessViewModel` parameterless constructor with nullable `IProcessLauncher?` parameter
- [ ] Add null check with `InvalidOperationException` in `StartAsync` commands when `_processLauncher` is null
- [ ] Add `[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]` to properties that shouldn't use parameterless constructors
- [ ] Create `private static readonly JsonSerializerOptions` for load and save in `ConfigurationManagerBase.cs`
- [ ] Replace inline `new JsonSerializerOptions` allocations with the static readonly fields in both `LoadConfigurationAsync` and `SaveConfigurationAsync`
- [ ] Add `LaunchDelayMilliseconds` property to `ApplicationConfiguration` record with default value 3000
- [ ] Update `ProcessStarter` to read `LaunchDelayMilliseconds` from configuration instead of hardcoded constant
- [ ] Remove `const int LaunchDelayMilliseconds = 3000` from `ProcessStarter`

### Analysis details

Three independent but quick improvements. The null-forgiving operators hide potential null reference exceptions — making the parameter nullable with a runtime check is safer. The JSON options allocation on every load/save is a measurable performance cost, especially for undo/redo snapshots. The hardcoded 3-second delay is a magic number that should be user-configurable.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Search for `null!` in ViewModel constructors — should return zero results
- Search for `new JsonSerializerOptions` in `ConfigurationManagerBase.cs` — should return zero results (static readonly only)
- Search for `const int LaunchDelayMilliseconds` in `ProcessStarter.cs` — should return zero results

### Phase Summary
_(write when phase completes)_

---

## Phase 4: Mapping Deduplication and Test Coverage Expansion
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #8 (mapping duplication), #9 (test coverage gaps)
**Effort**: 2-3 days
**Impact**: Long-term maintainability and confidence in changes

- [ ] Update `EditViewModel.SerializeGroups()` to use `ProcessGroupMapper.MapToProcessGroups()` instead of manual serialization logic
- [ ] Create `ToModel`/`ToViewModel` extension methods on `CommandGroup`/`CommandProcess` that delegate to `IProcessModelMapper`
- [ ] Update `CommandDefinitionsRepositoryTests` to use `ProcessGroupMapper` instead of manual mapping helpers
- [ ] Create `ProcessGroupMapperTests.cs` with round-trip mapping tests (ProcessGroup → CommandGroup → ProcessGroup)
- [ ] Create `ProcessStarterTests.cs` with mocked `IProcessLauncher` and `IProcessModelMapper`
- [ ] Create `ArgumentExtensionsTests.cs` with edge cases (empty args, special characters, quotes, unicode)
- [ ] Create `ConfigurationFileProviderBaseTests.cs` with real implementation and mocked `IMessageService`
- [ ] Create `MainWindowViewModelTests.cs` with mocked dependencies for basic command tests
- [ ] Add integration test for full load-edit-save cycle using a temporary config file
- [ ] Verify all new tests pass and coverage improves

### Analysis details

The mapping logic is duplicated across EditViewModel serialization, ProcessGroupMapper, and test helpers. Consolidating to a single source of truth (the mapper) reduces maintenance burden and prevents divergence.

Test coverage has gaps in ProcessStarter, ProcessGroupMapper, ArgumentExtensions, and MainWindowViewModel. Adding these tests provides confidence for future refactoring and catches regressions.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass (50+ tests expected after this phase)
- `dotnet test ML.ApplicationLauncher.Tests -- --list-tests` — verify new test classes appear
- Search for duplicate mapping logic (manual property copying) in tests — should be minimal

### Phase Summary
_(write when phase completes)_

---

## Phase 5: Medium Priority — UX, Performance, and Project Cleanup
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #10 (XAML validation feedback), #11 (command factory compaction), #12 (undo snapshot optimization), #13 (default config template), #14 (WorkerService cleanup)
**Effort**: 3-4 days
**Impact**: UX improvements, performance optimization, clearer project structure

- [ ] Add `Validation.ErrorTemplate` style to `EditView.xaml` for TextBox controls showing validation errors in red
- [ ] Add validation summary `ItemsControl` at the top of `EditView` to display all current validation errors
- [ ] Bind `SaveCommand` button `IsEnabled` to `!HasErrors` so save is disabled when validation fails
- [ ] Add `_commandsSinceLastCompact` counter and `CompactThreshold` constant to `RefreshableCommandFactory`
- [ ] Replace full list scan in `CompactDeadReferences` with threshold-based compaction
- [ ] Add optional GZip compression to `UndoRedoManager` snapshot serialization for large configurations
- [ ] Add memory usage warning when total snapshot size exceeds a configurable threshold (e.g., 50MB)
- [ ] Create a default configuration template with example commands (calculator, notepad, terminal) embedded in `ML.ApplicationLauncher.Shell.Assets`
- [ ] Update `ConfigurationFileProviderBase` to use the embedded template instead of empty `[]` or `{}`
- [ ] Remove `ML.WorkerService.Launcher` from solution if not needed, or add a README explaining the intent

### Analysis details

This phase addresses UX improvements (users can now see validation errors), performance optimizations (command factory compaction, undo snapshot compression), first-run experience (useful default config), and project cleanup (empty WorkerService project).

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Launch app and trigger a validation error — verify red error text appears next to the invalid field
- Launch app with no config file — verify example commands appear in the UI
- `Get-ChildItem ML.WorkerService.Launcher -Recurse` — verify project is removed or has a README

### Phase Summary
_(write when phase completes)_

---

## Phase 6: Low Priority — Polish and Nice-to-Haves
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #15 (parallel process launching), #16 (XML docs), #17 (record types), #18 (logging), #19 (centralized error strings), #20 (Roslyn analyzers)
**Effort**: 1-2 days
**Impact**: Developer experience, code quality automation, parallel performance

- [ ] Update `CommandGroupViewModel.LaunchAllProcessesAsync` to use `Task.WhenAll` for parallel launching
- [ ] Add `ParallelLaunch` boolean property to `CommandGroup` model and config for sequential vs parallel control
- [ ] Add `<summary>` XML documentation to all public types in `ML.ApplicationLauncher.Source` models
- [ ] Add `<summary>` XML documentation to all public ViewModels in `ML.ApplicationLauncher.Shared`
- [ ] Convert `CommandGroup` class to `record` with value-based equality
- [ ] Convert `CommandProcess` class to `record` with value-based equality
- [ ] Add `Microsoft.Extensions.Logging` registration in `SourceServiceInstaller`
- [ ] Add log-to-file configuration with Serilog sink (rolling file)
- [ ] Replace hardcoded error message strings in ViewModels with a resource file (`Resources.resx`)
- [ ] Add `Meck.Lost` or `SonarAnalyzer.CSharp` NuGet package to `Directory.Packages.props`
- [ ] Enable `<AnalysisLevel>latest-All</AnalysisLevel>` in `Directory.Build.props`

### Analysis details

This phase contains polish items that improve the developer experience (XML docs, analyzers), enable parallel process launching for faster execution, modernize the codebase with record types, add structured logging for post-mortem debugging, and centralize error strings for maintainability and localization.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Build output should show no Roslyn analyzer warnings (or only suppressed ones)
- Launch app, trigger an error, verify log file is created with structured output
- Verify `CommandGroup` and `CommandProcess` support `with` expressions after record conversion

### Phase Summary
_(write when phase completes)_

---

## Final Recap
_(write when all phases complete: summary of the entire piece of work)_

## Deployment Plan
_(write when all phases complete: step-by-step deployment instructions)_
