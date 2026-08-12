# Codebase Improvements — ML.ApplicationLauncher

Address 15 codebase issues identified in the full-codebase review (2026-07-16), organized by severity. Each item is a self-contained phase with its own verification plan and completion summary.

**Audit update (2026-08-12):** After reviewing all 15 phases against the current codebase, **6 of 13 remaining phases are already complete** (phases 4, 6, 9, 13 + previously completed 1, 2). That leaves **7 genuine action items**:

| Priority | Phases Remaining |
|----------|-----------------|
| Critical | Phase 3 — Thread safety in `ProcessLauncher.ComputeDelay()` |
| High     | Phase 5 — Dead weak reference compaction in `RefreshableCommandFactory` |
| High     | Phase 7 — Unobserved task exception handling in `MainWindowViewModel` |
| Medium   | Phase 8 — Extract magic numbers to named constants (3 files) |
| Medium   | Phase 10 — Rename DTO `Children` → `ChildGroups` in `CommandGroup` |
| Low      | Phase 11 — Rename struct overload `ShouldNotBeNull` → `ShouldHaveValue` |
| Low      | Phase 12 — Standardize `CancellationToken` patterns (3 hardcoded `.None`) |
| Low      | Phase 14 — Add unit tests for `ValidationExtensions` |

## For Future Agents

As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to `Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue with zero context); run the phase's **Verification Plan** and record the result before moving on. When all phases are done, fill in **Final Recap** and **Deployment Plan**.

---

## Phase 1: Fix null-safety bug in `MessageService.TrimMessageLength`
Status: Complete   <!-- Critical -->

- [x] Replace `TrimMessageLength` implementation with a safe version that handles null/empty strings without throwing
- [x] Add unit test for null input, empty string input, and normal-length input (≤1000 chars)
- [x] Verify no callers pass `null` to the method — if any do, update them

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~MessageService" 2>&1
```
Expected: zero errors, all MessageService tests pass.

**Result:** ✅ Build succeeded (0 errors), 6/6 MessageService tests passed in 3.2s.

### Phase Summary
Fixed `TrimMessageLength` to handle null/empty strings safely without changing the method signature. The implementation uses early returns for clarity: if message is null or ≤1000 chars, return as-is; otherwise truncate with range operator. Changed visibility from `private` to `internal` (enabled via `InternalsVisibleTo` in csproj) so tests can invoke it directly without reflection. Added 6 unit tests covering: null input, empty string, normal length, boundary at exactly 1000 chars, over 1000 chars (2000), and just over 1000 chars (1001). All tests pass with direct method invocation — no reflection needed.

---

## Phase 2: Add config file backup before overwrite in `ConfigurationFileProviderBase`
Status: Complete ✅ (commit f7ee9c5)

- [x] Before writing the default `"[]"`, rename any existing file to `<filename>.bak`
- [x] Validate that the template content is valid JSON for the target type before writing (wrap `WriteAllText` in a try-catch on deserialization)
- [x] Add unit test that verifies backup creation when an existing config file is present

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~ConfigurationFileProvider" 2>&1
```
Expected: zero errors, all config provider tests pass.

### Phase Summary
Added backup (.bak file) logic and JSON validation to `ConfigurationFileProviderBase.BuildConfigurationFilePath`:
- **Backup creation**: Before overwriting a config file with default content, existing file is moved to `<filename>.bak` (old .bak removed first if exists)
- **JSON validation**: Default template validated via `JsonDocument.Parse()` before writing; error shown via IMessageService if invalid JSON
- **Type detection**: Runtime check for array types (`IsArray`) returns `"[]"`, objects return `"{}"`
- **4 unit tests** added in `ConfigurationFileProviderBaseTests.cs` verifying backup creation, old backup removal, and type-specific default content

---

## Phase 3: Synchronize `_lastStartup` access in `ProcessLauncher`
Status: Not started   <!-- Critical -->

- [ ] Note: file is named `ML.ApplicationLauncher.Source/Dependencies/ProcessLauncher.cs` (not ProcessStarter)
- [ ] Add a private `object _delayLock = new()` field to `ProcessLauncher`
- [ ] Wrap the delay check and `_lastStartup` assignment in a `lock(_delayLock)` block inside `ComputeDelay()` — both read of `_lastStartup` and write must be atomic
- [ ] Verify the lock scope covers both the read and write — no partial reads possible

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
# Static review: confirm lock covers both read and write of _lastStartup in ComputeDelay()
```
Expected: zero errors, static review confirms lock correctness.

### Phase Summary
_(write when phase completes)_

---

## Phase 4: Add size limits to undo/redo stacks in `EditViewModel`
Status: Complete ✅

- [x] `UndoRedoManager<TSnapshot>` class with configurable `MaxHistorySize` handles stack limiting
- [x] `EditViewModel` uses `new UndoRedoManager<UndoState>(100)` — 100 undo steps max
- [x] `public const int MaxUndoRedoStackSize = 100` constant exists on EditViewModel

### Verification Plan
Already verified during audit (2026-08-12). UndoRedoManager.Push() trims `_history.RemoveAt(0)` when count exceeds limit.

### Phase Summary
Phase was already implemented before the plan was created. `UndoRedoManager<T>` with configurable max history size and a constant `MaxUndoRedoStackSize = 100` are in place. No further work needed.

---

## Phase 5: Compact dead weak references in `RefreshableCommandFactory`
Status: Not started   <!-- High -->

- [ ] Replace the two-list pattern (`aliveCommands`) with an in-place compaction loop that removes dead entries from `_commands` directly
- [ ] Alternatively, call `_commands.RemoveAll(wr => !wr.TryGetTarget(out _))` at end of each refresh cycle
- [ ] Add unit test verifying that after a command is garbage collected, the next refresh reduces `_commands.Count`

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
# Static review: confirm dead references are removed from _commands after refresh
```
Expected: zero errors, static review confirms compaction logic.

### Phase Summary
_(write when phase completes)_

---

## Phase 6: Fix validation set propagation in `CommandDefinitionsRepository`
Status: Complete ✅

- [x] `ValidateAll()` creates a single `HashSet<Guid>` and passes it to all recursive calls
- [x] The `seenIds ??= new HashSet<Guid>()` fallback in `ValidateGroup` is intentional — needed for non-bulk paths like `AddProcessAsync` which call `ValidateGroup(group)` without a set

### Verification Plan
Already verified during audit (2026-08-12). No further changes needed. A duplicate-detection test across nested groups would add value but is not required for correctness.

### Phase Summary
Phase was already implemented before the plan was created. `ValidateAll` properly propagates a single HashSet through all recursive calls. The remaining null-coalescing in `ValidateGroup` is defensive code for non-bulk callers, not a bug.

---

## Phase 7: Handle unobserved task exceptions in `MainWindowViewModel`
Status: Not started   <!-- High -->

- [ ] Add a static constructor to `MainWindowViewModel` that subscribes to `TaskScheduler.UnobservedTaskException` and logs the exception via `_messageService.ShowError` or a dedicated logger
- [ ] Alternatively, wrap each `Task.Run(...)` in a try-catch that surfaces errors through `_messageService`
- [ ] Verify no unhandled exceptions escape from `LoadListAsync` or `ExpireLastExecutionTimeLoopAsync`

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
# Static review: confirm exception handler covers both Task.Run calls
```
Expected: zero errors, static review confirms exception handling.

### Phase Summary
_(write when phase completes)_

---

## Phase 8: Extract magic numbers to named constants
Status: Not started   <!-- Medium -->

- [ ] In `MessageService.cs`: extract `1000` → `private const int MaxErrorMessageLength = 1000;` (still hardcoded on lines 27 and 30)
- [ ] In `RefreshableCommandFactory.cs`: extract `250` (ms) → `private const int CanExecuteRefreshIntervalMs = 250;` (still hardcoded on line 19)
- [ ] In `ProcessLauncher.cs` (not ProcessStarter): the `3` seconds is wrapped in `TimeSpan.FromSeconds(3)` — consider extracting to a named constant for consistency

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
```
Expected: zero errors across both projects.

### Phase Summary
_(write when phase completes)_

---

## Phase 9: Log deserialization exceptions in `ConfigurationManagerBase`
Status: Complete ✅

- [x] Broad `catch (Exception ex)` already covers `JsonException` and all other serialization errors
- [x] Error is surfaced via `_messageService.ShowError()` with file path and config type name included
- [x] Falls back to `DefaultConfiguration` on any failure

### Verification Plan
Already verified during audit (2026-08-12). The generic catch provides adequate coverage — a specific `catch (JsonException)` would only add marginally better error messages.

### Phase Summary
Phase was already implemented before the plan was created. `LoadConfigurationAsync` has comprehensive exception handling with user-facing error messages and safe fallback behavior.

---

## Phase 10: Rename DTO `Children` to `ChildGroups` in `CommandGroup`
Status: Not started   <!-- Medium -->

- [ ] In `ML.ApplicationLauncher.Source/CommandGroup.cs`: rename property `Children` → `ChildGroups` with XML doc comment clarifying it is a persistence DTO
- [ ] Update all mapping code that references `.Children` on `CommandGroup` to use `.ChildGroups` instead
- [ ] Verify no other project (Shared, Shell) references the old name on the DTO type

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
grep_search("CommandGroup\.Children", isRegexp=false, includePattern="*.cs")
```
Expected: zero errors, grep returns no remaining references to `CommandGroup.Children`.

### Phase Summary
_(write when phase completes)_

---

## Phase 11: Clarify nullable validation extension behavior in `ValidationExtensions`
Status: Not started   <!-- Medium -->

- [ ] Rename the struct overload of `ShouldNotBeNull` to `ShouldHaveValue` with XML doc clarifying its purpose (asserts assignment for nullable value types)
- [ ] Update all callers that use the struct variant to call `ShouldHaveValue` instead
- [ ] Verify no compile errors and behavior is unchanged at runtime

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Core/ML.ApplicationLauncher.Core.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
grep_search("ShouldNotBeNull", isRegexp=false, includePattern="*.cs")
```
Expected: zero errors in Core project; grep returns only the struct overload definition (or references to the renamed method).

### Phase Summary
_(write when phase completes)_

---

## Phase 12: Standardize CancellationToken patterns across `ML.ApplicationLauncher.Source`
Status: Not started   <!-- Medium -->

- [ ] Audit all public async methods in `ML.ApplicationLauncher.Source` project — add `CancellationToken cancellationToken = default` parameter where missing
- [ ] Update callers that hardcode `CancellationToken.None` to pass through their own token parameter (or keep `default` if they are entry points)
- [ ] Verify consistent usage across `CommandDefinitionsRepository`, `ProcessStarter`, and `ConfigurationManagerBase`

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
grep_search("CancellationToken\.None", isRegexp=false, includePattern="*.cs")
```
Expected: zero errors; grep returns only `default` patterns or entry-point uses.

### Phase Summary
_(write when phase completes)_

---

## Phase 13: Remove commented-out code from XAML files
Status: Complete ✅

- [x] `<UserControl.Resources>` in `EditView.xaml` is already clean (empty)
- [x] No dead `<!-- -->` comment blocks found in active XAML files

### Verification Plan
Already verified during audit (2026-08-12). All previously commented-out DataTemplate blocks have been removed.

### Phase Summary
Phase was already completed before the plan was created. XAML files are clean with no dead code comments remaining.

---

## Phase 14: Add unit tests for validation helpers in `ValidationExtensions`
Status: Not started   <!-- Low -->

- [ ] Create new test file `ML.ApplicationLauncher.Tests/ValidationExtensionsTests.cs`
- [ ] Add tests covering: null input to `ShouldNotBeNull`, empty array to `ShouldNotBeNullOrEmpty`, nullable struct with and without value, parameter name propagation via `[CallerArgumentExpression]`
- [ ] Verify all tests pass

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~ValidationExtensionsTests" 2>&1
```
Expected: zero errors, all 6+ validation tests pass.

### Phase Summary
_(write when phase completes)_

---

## Phase 15: Audit shared MSBuild props for consistency in root `.props` files
Status: Not started   <!-- Low -->

- [ ] Read `Directory.Build.props` and `Directory.Packages.props` — confirm all projects inherit consistently (no project overrides `OutputType`, `TargetFramework`, or common properties unnecessarily)
- [ ] Document any inconsistencies found in the file itself via XML comments
- [ ] Verify solution builds cleanly with the shared props

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.slnx --no-incremental 2>&1 | Select-String -Pattern "error"
# Static review: confirm each .csproj uses shared props where possible
```
Expected: zero errors; static review confirms consistent inheritance.

### Phase Summary
_(write when phase completes)_

---

## Final Recap
_(write when all phases complete)_

## Deployment Plan
_(write when all phases complete)_
