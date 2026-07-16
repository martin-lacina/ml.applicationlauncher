# Codebase Improvements — ML.ApplicationLauncher

Address 15 codebase issues identified in the full-codebase review (2026-07-16), organized by severity. Each item is a self-contained phase with its own verification plan and completion summary.

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

## Phase 3: Synchronize `_lastStartup` access in `ProcessStarter`
Status: Not started   <!-- Critical -->

- [ ] Add a private `object _delayLock = new()` field to `ProcessStarter`
- [ ] Wrap the delay check and `_lastStartup` assignment in a `lock(_delayLock)` block inside `StartAsync`
- [ ] Verify the lock scope covers both the read of `_lastStartup` and the write — no partial reads possible

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
# Static review: confirm lock covers both read and write of _lastStartup in StartAsync
```
Expected: zero errors, static review confirms lock correctness.

### Phase Summary
_(write when phase completes)_

---

## Phase 4: Add size limits to undo/redo stacks in `EditViewModel`
Status: Not started   <!-- High -->

- [ ] Add a constant `MaxUndoRedoStackSize = 100` (or similar) as private static readonly
- [ ] Modify the push logic so that when `_undoStack.Count > MaxUndoRedoStackSize`, the oldest entry is removed (`Pop`)
- [ ] Apply same limit to `_redoStack`
- [ ] Add unit test verifying stack does not exceed max size after N pushes

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~EditViewModel" 2>&1
```
Expected: zero errors, all EditViewModel tests pass.

### Phase Summary
_(write when phase completes)_

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
Status: Not started   <!-- High -->

- [ ] Change `ValidateAll(List<CommandGroup> groups)` to create a single `HashSet<Guid>` and pass it through all recursive calls
- [ ] Remove the `seenIds ??= new HashSet<Guid>()` pattern from inside `ValidateGroup` — it should never be null when called recursively
- [ ] Add unit test verifying that duplicate IDs across nested groups are detected

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~CommandDefinitionsRepository" 2>&1
```
Expected: zero errors, duplicate-ID detection tests pass.

### Phase Summary
_(write when phase completes)_

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

- [ ] In `MessageService.cs`: extract `1000` → `private const int MaxErrorMessageLength = 1000;` and use it in `TrimMessageLength`
- [ ] In `RefreshableCommandFactory.cs`: extract `250` (ms) → `private const int CanExecuteRefreshIntervalMs = 250;` and use it in the timer constructor
- [ ] In `ProcessStarter.cs`: extract `3` (seconds) → `private const int LaunchDelaySeconds = 3;` and use it for `_delay` initialization

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
Status: Not started   <!-- Medium -->

- [ ] Wrap `JsonSerializer.Deserialize<TConfiguration>(...)` in a try-catch for `JsonException` and other serialization errors
- [ ] On exception, call `_messageService.ShowError($"Failed to parse config file '{_configurationProvider.ConfigurationFilePath}': {ex.Message}")` before falling back to default
- [ ] Add unit test with malformed JSON content verifying the error is surfaced

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
# Static review: confirm JsonException catch covers deserialization failure path
```
Expected: zero errors, static review confirms error handling.

### Phase Summary
_(write when phase completes)_

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
Status: Not started   <!-- Low -->

- [ ] In `ML.ApplicationLauncher.Shared/Views/EditView.xaml`: remove the commented-out DataTemplate block in `<UserControl.Resources>`
- [ ] Search all `.xaml` files for commented blocks (`<!--...-->`) and remove any that are clearly dead code (not referencing active features)
- [ ] Verify XAML compiles without warnings

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental 2>&1 | Select-String -Pattern "error|warning"
grep_search("<!--", isRegexp=false, includePattern="*.xaml")
```
Expected: zero errors; grep returns no (or only legitimate) comments.

### Phase Summary
_(write when phase completes)_

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
