# Consolidate Mapping — Completion Plan

## Goal
Verify and close remaining gaps from the mapping consolidation. The shared `IProcessModelMapper` is already implemented, registered in DI, and used by all three consumers (`CommandDefinitionsRepository`, `MainWindowViewModel`, `EditViewModel`). `ProcessListProvider` has been removed. Only unit tests for the mapper remain.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to `Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue with zero context); run the phase's **Verification Plan** and record the result before moving on. When all phases are done, fill in **Final Recap** and **Deployment Plan**.

---

## Phase 1: Create unit tests for `ProcessGroupMapper`
Status: Not started   <!-- Repository tests mock the mapper; no direct mapper tests exist -->

- [ ] Create `ML.ApplicationLauncher.Tests/ProcessGroupMapperTests.cs`
- [ ] Test forward mapping: single `ProcessGroup` → `CommandGroup` (all fields mapped correctly)
- [ ] Test forward mapping: nested groups with children and processes (recursive)
- [ ] Test forward mapping: null/empty Groups and Processes collections handled gracefully
- [ ] Test reverse mapping: single `CommandGroup` → `ProcessGroup` (all fields, including arguments parsing via `ArgumentExtensions`)
- [ ] Test reverse mapping: nested groups (recursive)
- [ ] Test Guid generation: uses `Guid.CreateVersion7()` not `Guid.NewGuid()`
- [ ] Test bulk extension methods: `MapToCommandGroups()` and `MapToProcessGroups()` with empty/null input

### Verification Plan
```powershell
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~ProcessGroupMapperTests" --verbosity minimal 2>&1
```
Expected: All new mapper tests pass (target: ≥8 tests).

### Phase Summary
_(write when phase completes)_

---

## Already Completed (Verified During Audit)

| Phase | Description | Evidence |
|-------|-------------|----------|
| **Phase 1 (mapper creation)** | `IProcessModelMapper` + `ProcessGroupMapper` with bidirectional mapping, `Guid.CreateVersion7()`, bulk extension methods in `ProcessModelMapperExtensions.cs` | Registered via `.RegisterSingleton<IProcessModelMapper, ProcessGroupMapper>()` |
| **Phase 2 (repository refactor)** | Repository uses `_mapper.MapToCommandGroups()` / `MapToProcessGroups()` — no inline mapping methods remain | `LoadAsync()` and `SaveAsync()` delegate to mapper entirely |
| **Phase 3 (remove ProcessListProvider)** | `ProcessListProvider` does not exist; MainWindowViewModel loads via `CommandDefinitionsRepository` + filters with `HasVisibleDescendant` | Grep for `ProcessListProvider` returns zero matches across all .cs files |
| **Phase 4 (EditViewModel mapper usage)** | EditViewModel uses `_mapper` constructor param, but keeps local `ToViewModel(CommandGroup)` and `ToModel(CommandGroupViewModel)` — these map Command↔ViewModel which cannot live in the Source-layer mapper (dependency direction) | Architectural limitation acknowledged; duplication is unavoidable without introducing ViewModel types into Source project |
| **Phase 6 (cleanup)** | No dead code, no ProcessListProvider references remaining | Codebase is clean |

### Why ViewModel Mapping Was Not Consolidated Into The Mapper

The plan originally proposed adding `MapToViewModel(CommandGroup → CommandGroupViewModel)` methods to the mapper. This is architecturally impossible:
- **Mapper lives in** `ML.ApplicationLauncher.Source` (domain layer)
- **ViewModels live in** `ML.ApplicationLauncher.Shared` (presentation layer)
- Adding ViewModel types to the mapper interface would create a Source → Shared dependency, violating the layered architecture

The current approach (each consumer has its own Command↔ViewModel mapping with launcher/factory parameters) is correct by design.

---

## Final Recap
_(write when all phases complete: summary of the entire piece of work)_

## Deployment Plan
_(write when all phases complete: step-by-step deployment instructions)_
