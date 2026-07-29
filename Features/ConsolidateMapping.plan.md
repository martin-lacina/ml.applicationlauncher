# Consolidate Mapping: Main View → Edit Path (via CommandDefinitionsRepository)

## Goal
Eliminate duplicate mapping logic across `MainWindowViewModel`, `CommandDefinitionsRepository`, and `EditViewModel` by extracting a shared `IProcessGroupMapper`. Make the main view load through `CommandDefinitionsRepository` instead of `IProcessListProvider`, while preserving the `IsVisible()` filter exclusively in the main view.

## Scope
- **In scope**: Extract mapper, refactor all three consumers, remove `ProcessListProvider`, update tests
- **Out of scope**: Changes to `ProcessGroup`/`CommandGroup` models, changes to `IsVisible()` logic, changes to undo/redo infrastructure

## Confirmed Design Decisions
1. **Remove `ProcessListProvider` entirely** — no other consumers exist; everything goes through `CommandDefinitionsRepository`.
2. **Use `Guid.CreateVersion7()`** in the mapper (matches record defaults).
3. **Move reverse mapping (`ToModel`) into the mapper** — full bidirectional mapping in one place.
4. **`IsVisible()` filter stays in `MainWindowViewModel`** — not in the mapper or repository. The main view decides what to show; edit mode sees everything.
5. **Mapper is dumb** — no filtering, no validation, just pure conversion.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to `Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue with zero context); run the phase's **Verification Plan** and record the result before moving on. When all phases are done, fill in **Final Recap** and **Deployment Plan**.

---

## Phase 1: Create `IProcessGroupMapper` interface and implementation
Status: Not started

- [ ] Create `ML.ApplicationLauncher.Source/Services/IProcessGroupMapper.cs` with full bidirectional mapping interface
- [ ] Create `ML.ApplicationLauncher.Source/Services/ProcessGroupMapper.cs` implementing all mapping methods
- [ ] Register in DI via `SourceServiceInstaller.cs`: `.RegisterSingleton<IProcessGroupMapper, ProcessGroupMapper>()`

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx` — must compile with no errors
- Check that `IProcessGroupMapper` has these methods:
  - `CommandGroup MapToCommandGroup(ProcessGroup source)` (recursive)
  - `List<CommandGroup> MapToCommandGroups(IEnumerable<ProcessGroup> sources)`
  - `ProcessGroup MapToProcessGroup(CommandGroup source)` (recursive)
  - `ProcessGroup[] MapToProcessGroups(IEnumerable<CommandGroup> sources)`
  - `CommandGroupViewModel MapToViewModel(CommandGroup source, IProcessLauncher?, ICommandFactory?)` (recursive)
  - `CommandProcessViewModel MapToViewModel(CommandProcess source, IProcessLauncher?, ICommandFactory?)`
  - `CommandGroup MapToCommandGroup(CommandGroupViewModel source)` (recursive reverse)
  - `List<CommandGroup> MapToCommandGroups(IEnumerable<CommandGroupViewModel> sources)` (reverse)

### Phase Summary
_(write when phase completes)_

---

## Phase 2: Refactor `CommandDefinitionsRepository` to use the mapper
Status: Not started

- [ ] Add `IProcessGroupMapper` constructor parameter to `CommandDefinitionsRepository`
- [ ] Replace `MapProcessGroupsToCommandGroups()` calls with `_mapper.MapToCommandGroups()`
- [ ] Replace `MapCommandGroupsToProcessGroups()` calls with `_mapper.MapToProcessGroups()`
- [ ] Delete `MapProcessGroupsToCommandGroups()` method from repository
- [ ] Delete `MapProcessGroupToCommandGroup()` method from repository
- [ ] Delete `MapCommandGroupsToProcessGroups()` method from repository
- [ ] Delete `MapCommandGroupToProcessGroup()` method from repository

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx` — must compile with no errors
- `dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~CommandDefinitionsRepository" --verbosity minimal` — all existing tests pass

### Phase Summary
_(write when phase completes)_

---

## Phase 3: Refactor `MainWindowViewModel` to use repository + mapper
Status: Not started

- [ ] Add `IProcessGroupMapper` constructor parameter to `MainWindowViewModel`
- [ ] Remove `_configurationProvider` field and constructor parameter (`IConfigurationLocationProvider<ProcessGroup[]>`)
- [ ] Remove `_processListProvider` field and constructor parameter (`IProcessListProvider`)
- [ ] Inject `IConfigurationManager<ProcessGroup[]>` (already present) into a new private `_repository` field
- [ ] Replace `LoadListAsync()` body: call `_repository.LoadAsync()`, then filter with `.Where(pg => pg.IsVisible())`, then map with `_mapper.MapToViewModel()`
- [ ] Delete `MapGroup(ProcessGroup)` method from `MainWindowViewModel`
- [ ] Delete `MapProcess(ProcessLaunchInformation)` method from `MainWindowViewModel`
- [ ] Remove `IProcessListProvider` registration from `SourceServiceInstaller.cs`
- [ ] Remove `IProcessListProvider` interface file (`ML.ApplicationLauncher.Source/Services/IProcessListProvider.cs`)
- [ ] Remove `ProcessListProvider` class file (`ML.ApplicationLauncher.Source/Services/ProcessListProvider.cs`)

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx` — must compile with no errors
- Verify `IsVisible()` filter logic is preserved in `LoadListAsync()`:
  - Groups: `!Hidden && (has visible children OR has visible processes)`
  - Processes: `!Hidden`
- Verify disabled filtering is also applied (from original `ProcessListProvider.Filter()`):
  - Groups: `!Disabled`
  - Processes: `!Disabled`

### Phase Summary
_(write when phase completes)_

---

## Phase 4: Refactor `EditViewModel` to use the mapper
Status: Not started

- [ ] Add `IProcessGroupMapper` constructor parameter to `EditViewModel`
- [ ] Replace `ToViewModel(CommandGroup)` calls with `_mapper.MapToViewModel()`
- [ ] Replace `ToModel(CommandGroupViewModel)` calls with `_mapper.MapToCommandGroup()` (reverse)
- [ ] Delete `ToViewModel(CommandGroup)` method from `EditViewModel`
- [ ] Delete `ToModel(CommandGroupViewModel)` method from `EditViewModel`

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx` — must compile with no errors
- Verify undo/redo still works: `SerializeGroups()` → `Deserialize<UndoState>` round-trip unchanged (uses `CommandGroup` model, not mapper)

### Phase Summary
_(write when phase completes)_

---

## Phase 5: Update tests
Status: Not started

- [ ] Update `CommandDefinitionsRepositoryTests.cs` — add mock for `IProcessGroupMapper` if constructor signature changed
- [ ] Create `ML.ApplicationLauncher.Tests/ProcessGroupMapperTests.cs` with test cases:
  - [ ] Test forward mapping: single `ProcessGroup` → `CommandGroup` (all fields)
  - [ ] Test forward mapping: nested groups (recursive)
  - [ ] Test forward mapping: null/empty collections handled gracefully
  - [ ] Test reverse mapping: `CommandGroup` → `ProcessGroup` (all fields)
  - [ ] Test reverse mapping: nested groups (recursive)
  - [ ] Test ViewModel mapping: `CommandGroup` → `CommandGroupViewModel` with launcher/factory
  - [ ] Test ViewModel mapping: `CommandGroup` → `CommandGroupViewModel` without launcher/factory (null case)
  - [ ] Test Guid generation: uses `Guid.CreateVersion7()` not `Guid.NewGuid()`

### Verification Plan
- `dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --verbosity minimal` — all tests pass
- Verify new mapper tests cover: forward, reverse, ViewModel, null safety, nested recursion

### Phase Summary
_(write when phase completes)_

---

## Phase 6: Final cleanup and verification
Status: Not started

- [ ] Run full test suite: `dotnet test ML.ApplicationLauncher.slnx --verbosity minimal`
- [ ] Verify no remaining references to `ProcessListProvider` or `IProcessListProvider` in codebase
- [ ] Verify no remaining references to deleted mapping methods in `CommandDefinitionsRepository`
- [ ] Check for any dead code / unused imports that can be removed
- [ ] Review all files for consistent naming and style

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx` — clean build, zero warnings
- `dotnet test ML.ApplicationLauncher.slnx --verbosity minimal` — all tests pass
- `grep -r "ProcessListProvider" .` — should return no matches (except possibly in git history)

### Phase Summary
_(write when phase completes)_

---

## Final Recap
_(write when all phases complete: summary of the entire piece of work)_

## Deployment Plan
_(write when all phases complete: step-by-step deployment instructions)_
