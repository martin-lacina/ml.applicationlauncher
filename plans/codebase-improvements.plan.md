# Codebase Improvements Implementation Plan

> **Based on**: `plans/codebase-review-improvements.md`
> **Goal**: Systematically implement all 20 improvement suggestions across 8 phases, transforming the codebase into a maintainable, testable, production-grade project.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done,
set its status to `Complete` and write its **Phase Summary** (what was done, key
decisions, anything needed to continue with zero context); run the phase's
**Verification Plan** and record the result before moving on. When all phases are
done, fill in **Final Recap** and **Deployment Plan**.

## Execution Order & Key Decisions
Phases are deliberately **not** in the original review's 1→6 order. Rationale:

- **Phase 3 (mapping dedup) precedes Phase 4 (DI + CancellationToken)** so that
  `CancellationToken` work lands on the final `SerializeGroups`/`RestoreGroupsFromJson`
  implementation rather than being reworked after the dedup refactor.
- **Phase 5 (test expansion) follows Phase 4** because uncommenting `EditViewModelTests`
  requires the DI fix.
- **Phase 6 (lifecycle) follows Phase 4** because `IAsyncDisposable` on `EditViewModel`
  wraps the `CancellationTokenSource` field introduced in Phase 4.
- **Phase 1 & 2 are independent quick wins** shipped first for fast value.

Confirmed decisions:
- **The 3-second launch delay is removed entirely** (no longer needed). It is a global
  rate limiter in `ProcessLauncher`, not a per-launch wait — see Phase 2.
- **The test project may reference WPF.** `ML.ApplicationLauncher.Tests` already
  references `ML.ApplicationLauncher.Shared` (UseWPF) and `Directory.Build.props` sets
  `net10.0-windows` globally, so WPF references work. If a build issue arises, make the
  test project WPF-enabled (`<UseWPF>true</UseWPF>`) — minor, not a blocker.
- **`CommandGroupViewModel.LaunchAllProcessesAsync` already uses `Task.WhenAll`** — the
  original #15 "parallel launching" is largely a no-op. Only the optional `ParallelLaunch`
  config flag remains (Phase 8).

---

## Phase 1: Quick Wins — Null Safety and JSON Options
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #4 (null-forgiving operators), #5 (JSON options allocation)
**Effort**: ~0.5 day
**Impact**: High — low-risk robustness and performance, ships value fast

- [ ] Replace `null!` in `CommandGroupViewModel` parameterless constructor with a nullable `IProcessLauncher?` parameter
- [ ] Replace `null!` in `CommandProcessViewModel` parameterless constructor with a nullable `IProcessLauncher?` parameter
- [ ] Add null check with `InvalidOperationException` in `StartAsync` commands when `_processLauncher` is null
- [ ] Create `private static readonly JsonSerializerOptions` for load and save in `ConfigurationManagerBase.cs`
- [ ] Replace inline `new JsonSerializerOptions` allocations with the static readonly fields in both `LoadConfigurationAsync` and `SaveConfigurationAsync`

### Analysis details
The null-forgiving operators hide potential null reference exceptions — making the
parameter nullable with a runtime check is safer. The JSON options allocation on every
load/save is a measurable performance cost, especially for undo/redo snapshots. These
two items are independent and touch different files, so they ship as a single low-risk
commit before any structural work begins.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Search for `null!` in ViewModel constructors — should return zero results
- Search for `new JsonSerializerOptions` in `ConfigurationManagerBase.cs` — should return zero results (static readonly only)

### Phase Summary
_(write when phase completes)_

---

## Phase 2: Remove Launch Delay
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #7 (hardcoded launch delay) — **decision: remove entirely**
**Effort**: ~0.5 day
**Impact**: High — removes an unnecessary 3-second spacing between launches

- [ ] Remove `LaunchDelaySeconds`, `_delay`, `_lastStartup`, `_delayLock` fields from `ProcessLauncher`
- [ ] Remove the `if (processStartInfoResolver.ApplyDelay) { _actionExecutor.Post(DelayExecutionAsync); }` block in `StartAsync`
- [ ] Remove the `DelayExecutionAsync` and `ComputeDelay` methods from `ProcessLauncher`
- [ ] Set `ApplyDelay => false` in `ExecutableProcessStartInfoResolverBase` (and confirm `StandaloneExecutableProcessStartInfoResolver` already returns false)
- [ ] Add a `ProcessLauncherTests` test verifying `StartAsync` launches immediately (no delay) using a mocked `IProcessStartInfoResolverSelector`

### Analysis details
The 3-second value is a **global rate limiter**: `ComputeDelay` tracks `_lastStartup`
and spaces every launch 3s apart, not a per-launch wait. The decision is to remove it
entirely — the spacing is no longer needed. Removing the delay logic also simplifies
`ProcessLauncher` (fewer fields, one fewer `ActionBlock` post path). `ApplyDelay` stays
on the resolver interface (standalone already false) but the base resolver flips to
false so no resolver requests a delay.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass, including the new `ProcessLauncherTests`
- Search for `LaunchDelaySeconds|ComputeDelay|_lastStartup` in `ProcessLauncher.cs` — should return zero results
- Launch two processes back-to-back in the app — second launches immediately (no 3s gap)

### Phase Summary
_(write when phase completes)_

---

## Phase 3: Mapping Deduplication and Mapper Tests
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #8 (mapping duplication) + mapper test slice of #9
**Effort**: ~1 day
**Impact**: Long-term maintainability; must land **before** Phase 4 to avoid rework

- [ ] Create `ToModel`/`ToViewModel` extension methods on `CommandGroup`/`CommandProcess` that delegate to `IProcessModelMapper`
- [ ] Update `EditViewModel.SerializeGroups()` to use `ProcessGroupMapper` (via the mapper) instead of the manual `ToModel` property-copying logic
- [ ] Update `EditViewModel.RestoreGroupsFromJson()` to use the mapper instead of the manual `ToViewModel` property-copying logic
- [ ] Remove the now-duplicate private `ToModel`/`ToViewModel` helpers from `EditViewModel`
- [ ] Update `CommandDefinitionsRepositoryTests` to use `ProcessGroupMapper` instead of manual mapping helpers
- [ ] Create `ProcessGroupMapperTests.cs` with round-trip mapping tests (ProcessGroup → CommandGroup → ProcessGroup)

### Analysis details
The mapping logic is duplicated across `EditViewModel` serialization, `ProcessGroupMapper`,
and test helpers. Consolidating to a single source of truth (the mapper) reduces
maintenance burden and prevents divergence. This phase deliberately precedes Phase 4
(DI + CancellationToken) so that the `CancellationToken` work in Phase 4 is applied to
the final, deduplicated `SerializeGroups`/`RestoreGroupsFromJson` implementation rather
than being reworked.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass, including new `ProcessGroupMapperTests`
- Search for manual property-copying mapping in `EditViewModel.cs` (`vm.Name = group.Name;` style) — should be gone, replaced by mapper calls

### Phase Summary
_(write when phase completes)_

---

## Phase 4: DI Bypass and CancellationToken Propagation
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #1 (EditViewModel DI bypass), #2 (CancellationToken gaps)
**Effort**: 2-3 days
**Impact**: Critical — enables testability and prevents hanging operations

- [ ] Create `ICommandDefinitionsRepository` interface in `ML.ApplicationLauncher.Source` exposing all public methods from `CommandDefinitionsRepository`
- [ ] Make `CommandDefinitionsRepository` implement `ICommandDefinitionsRepository`
- [ ] Register `ICommandDefinitionsRepository` as singleton in `SourceServiceInstaller.cs`
- [ ] Update `EditViewModel` constructor to accept `ICommandDefinitionsRepository` via DI instead of creating `CommandDefinitionsRepository` internally
- [ ] Remove internal `_repository` field initialization that bypasses DI
- [ ] Add `CancellationToken` parameter to `EditViewModel.SerializeGroups()` with `ThrowIfCancellationRequested()` at entry
- [ ] Add `CancellationToken` parameter to `EditViewModel.RestoreGroupsFromJson()` with `ThrowIfCancellationRequested()` at entry
- [ ] Add `CancellationToken` parameter to `CommandDefinitionsRepository.ValidateCircularReference()` with periodic cancellation checks in recursive traversal
- [ ] Add `CancellationToken` parameter to `ProcessGroupMapper.MapToCommandGroup()` and `MapToProcessGroup()` with cancellation checks during recursion
- [ ] Update all `EditViewModel` undo/redo calls to pass `CancellationToken` through `SerializeGroups` and `RestoreGroupsFromJson`
- [ ] Update `CommandDefinitionsRepository` public methods to accept and propagate `CancellationToken` to `ValidateCircularReference`
- [ ] Add `CancellationTokenSource` field to `EditViewModel` for cancelling all in-flight operations on cleanup
- [ ] Verify the test project can construct `EditViewModel` with injected mocks (WPF reference is fine — see Execution Order note)

### Analysis details
This phase addresses the two critical architectural concerns. The DI bypass in
`EditViewModel` is the root cause why all unit tests are commented out — the ViewModel
creates its own `CommandDefinitionsRepository` instead of receiving it from the
container. Introducing an interface abstraction enables mocking and proper lifecycle
management.

The CancellationToken gaps mean operations (serialization, recursive validation,
recursive mapping) can't be cancelled during rapid UI interactions or window close.
Adding cancellation throughout the call chain ensures responsive UI and clean shutdown.
Note: `SerializeGroups`/`RestoreGroupsFromJson` are synchronous JSON operations, so
cancellation there is an entry-point guard (cheap) — the recursive mapper and validator
methods get periodic checks.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — full test suite passes
- Search for `new CommandDefinitionsRepository(` in ViewModels — should return zero results outside DI registration
- Confirm `EditViewModel` is constructible in a test with mocked `ICommandDefinitionsRepository`

### Phase Summary
_(write when phase completes)_

---

## Phase 5: Test Coverage Expansion
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #9 (test coverage gaps) — the expansion slice
**Effort**: 1-2 days
**Impact**: Confidence in refactoring; unblocks safe future changes

- [ ] Uncomment and fix all tests in `EditViewModelTests.cs` now that DI injection is working
- [ ] Verify all `EditViewModelTests` pass with real injected dependencies (WPF reference is fine; if build fails, set `<UseWPF>true</UseWPF>` on the test project)
- [ ] Create `ArgumentExtensionsTests.cs` with edge cases (empty args, special characters, quotes, unicode)
- [ ] Create `ConfigurationFileProviderBaseTests.cs` with real implementation and mocked `IMessageService`
- [ ] Create `MainWindowViewModelTests.cs` with mocked dependencies for basic command tests
- [ ] Add integration test for full load-edit-save cycle using a temporary config file
- [ ] Verify all new tests pass and coverage improves

### Analysis details
Test coverage has gaps in ProcessStarter (added in Phase 2), ArgumentExtensions,
ConfigurationFileProviderBase, and MainWindowViewModel. The `EditViewModelTests` were
disabled precisely because of the DI bypass fixed in Phase 4 — now that DI works,
uncomment and fix them. Adding these tests provides confidence for future refactoring
and catches regressions. The mapper round-trip tests were already added in Phase 3.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass (50+ tests expected after this phase)
- `dotnet test ML.ApplicationLauncher.Tests --list-tests` — verify new test classes appear
- Confirm `EditViewModelTests` no longer contains commented-out test bodies

### Phase Summary
_(write when phase completes)_

---

## Phase 6: Fire-and-Forget Tasks and UI Thread Blocking
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #3 (fire-and-forget lifecycle), #6 (UI thread blocking in DI setup)
**Effort**: 1-2 days
**Impact**: High — prevents memory leaks, unobserved exceptions, and startup deadlocks

- [ ] Add `CancellationTokenSource _backgroundCts` field to `MainWindowViewModel` for background task lifecycle
- [ ] Replace `CancellationToken.None` in `ExpireLastExecutionTimeLoopAsync` with `_backgroundCts.Token`
- [ ] Replace fire-and-forget `_ = SaveAsync()` in `EditViewModel` with tracked tasks using `.ContinueWith()` for exception handling
- [ ] Add `IAsyncDisposable` implementation to `MainWindowViewModel` for `_backgroundCts` cleanup
- [ ] Add `IAsyncDisposable` implementation to `EditViewModel` for its `CancellationTokenSource` cleanup (field added in Phase 4)
- [ ] Capture `TaskScheduler.FromCurrentSynchronizationContext()` in `MainWindowViewModel` constructor for UI thread continuations
- [ ] Add error continuation on all background tasks to show errors via `_messageService.ShowError()`
- [ ] Refactor `SourceServiceInstaller.cs` to remove `Task.Run(...).GetAwaiter().GetResult()` pattern for `WindowsTerminalConfig` registration
- [ ] Replace blocking DI registration with lazy initialization or `OnStartup` async pattern in Prism shell
- [ ] Add loading indicator or splash screen during async startup initialization

### Analysis details
The fire-and-forget pattern (`_ = Task.Run(...)`) creates orphaned tasks with no
exception handling and no cancellation on shutdown. The `RunSafe` wrapper helps but
doesn't address lifecycle — tasks continue running after the ViewModel is disposed.
This phase runs **after** Phase 4 because the `EditViewModel` `IAsyncDisposable`
implementation wraps the `CancellationTokenSource` field introduced there.

The `SourceServiceInstaller` blocks the UI thread during DI container setup by
synchronously awaiting async file I/O. This can deadlock if `LoadConfigurationAsync`
shows a MessageBox (creates a new message loop). Moving this to an async startup
pattern eliminates the deadlock risk.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Search for `\.GetAwaiter\(\)\.GetResult\(\)` in Source project — should return zero results
- Search for `CancellationToken\.None` in ViewModel async methods — should be minimal (only where truly appropriate)
- Search for `_ = ` followed by async call in ViewModels — should have `.ContinueWith` or tracked task reference

### Phase Summary
_(write when phase completes)_

---

## Phase 7: Medium Priority — UX, Performance, and Project Cleanup
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
This phase addresses UX improvements (users can now see validation errors), performance
optimizations (command factory compaction, undo snapshot compression), first-run
experience (useful default config), and project cleanup (empty WorkerService project).
All items are independent of the earlier structural phases.

### Verification Plan
- `dotnet build --no-incremental` — solution builds with no errors
- `dotnet test ML.ApplicationLauncher.Tests` — all tests pass
- Launch app and trigger a validation error — verify red error text appears next to the invalid field
- Launch app with no config file — verify example commands appear in the UI
- `Get-ChildItem ML.WorkerService.Launcher -Recurse` — verify project is removed or has a README

### Phase Summary
_(write when phase completes)_

---

## Phase 8: Low Priority — Polish and Nice-to-Haves
Status: Not started   <!-- Not started | In progress | Complete -->

**Items**: #15 (parallel launch config — optional), #16 (XML docs), #17 (record types), #18 (logging), #19 (centralized error strings), #20 (Roslyn analyzers)
**Effort**: 1-2 days
**Impact**: Developer experience, code quality automation

- [ ] Add `ParallelLaunch` boolean property to `CommandGroup` model and config for sequential vs parallel control (note: `LaunchAllProcessesAsync` already uses `Task.WhenAll`, so this is an opt-in flag, not a new implementation)
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
This phase contains polish items that improve the developer experience (XML docs,
analyzers), modernize the codebase with record types, add structured logging for
post-mortem debugging, and centralize error strings for maintainability and
localization. The parallel-launch item is reduced to an opt-in config flag because
`LaunchAllProcessesAsync` already launches in parallel via `Task.WhenAll`.

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
