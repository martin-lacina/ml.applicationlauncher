# Edit‑Configuration Feature — Completion Plan

## Goal
Close all remaining gaps in the edit-configuration feature. The core functionality (data model, repository, ViewModel, UI views, toolbar toggle, detail panes, undo/redo, drag-and-drop) is already implemented and working. This plan tracks the finishing work: tests, validation hardening, documentation, CI enablement, and thread-safety audit.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to `Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue with zero context); run the phase's **Verification Plan** and record the result before moving on. When all phases are done, fill in **Final Recap** and **Deployment Plan**.

---

## Phase 1: Write EditViewModel unit tests
Status: Not started   <!-- The existing test file contains only dead/commented-out code -->

- [ ] Examine `ML.ApplicationLauncher.Tests/EditViewModelTests.cs` — currently has 1 placeholder test with all real tests commented out (WPF dependency issue)
- [ ] Determine if WPF mocking is needed or if ViewModel can be tested without UI thread dependencies
- [ ] Write tests for: AddGroup, AddProcess, RemoveGroup, RemoveProcess, MoveUp/MoveDown processes and groups
- [ ] Write test for Save command invocation (verify repository SaveAsync is called)
- [ ] Write test for HasUnsavedChanges state transitions

### Verification Plan
```powershell
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~EditViewModelTests" --verbosity minimal 2>&1
```
Expected: All EditViewModel tests pass (target: ≥8 new active tests replacing the current placeholder).

### Phase Summary
_(write when phase completes)_

---

## Phase 2: Add edge-case unit tests for repository and validation
Status: Not started   <!-- Only duplicate-ID test exists; circular refs, cancellation — no coverage -->

- [ ] Add test in `CommandDefinitionsRepositoryTests.cs`: `SaveAsync_CircularReference_Throws` (create parent-child with same ID)
- [ ] Add test: missing required fields (null/empty Name on group or process) are rejected during validation
- [ ] Add test: async cancellation — verify `LoadAsync` and `SaveAsync` respect CancellationToken
- [ ] Add test: URL path validation — verify paths like `https://...` pass through without `File.Exists` errors in repository layer (if applicable)

### Verification Plan
```powershell
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --filter "FullyQualifiedName~CommandDefinitionsRepository" --verbosity minimal 2>&1
```
Expected: All existing repo tests still pass + ≥4 new edge-case tests pass.

### Phase Summary
_(write when phase completes)_

---

## Phase 3: Harden path and argument validation in repository layer
Status: Not started   <!-- Empty-path check exists; no URL validation, no args non-empty check -->

- [ ] In `CommandDefinitionsRepository.ValidateGroup`: add check that process `Path` is not whitespace (already exists) — verify it covers all code paths
- [ ] Add optional validation for arguments being non-empty when ExecutionMode requires them (Raw/PowerShell)
- [ ] If URL support is intended: add `Uri.TryCreate(path, UriKind.Absolute)` check to allow HTTP(S) paths without `File.Exists` rejection
- [ ] Audit `CommandProcessViewModel.CanBeStarted` — currently checks `File.Exists(Path)` which will fail for URLs; add URI-aware logic if needed

### Verification Plan
```powershell
dotnet build ML.ApplicationLauncher.Source/ML.ApplicationLauncher.Source.csproj --no-incremental 2>&1 | Select-String -Pattern "error"
dotnet test ML.ApplicationLauncher.Tests/ML.ApplicationLauncher.Tests.csproj --verbosity minimal 2>&1
```
Expected: Zero errors, all tests pass.

### Phase Summary
_(write when phase completes)_

---

## Phase 4: Audit thread-safety of collection updates in repository
Status: Not started   <!-- EnableCollectionSynchronization exists on MainWindowViewModel; repository needs audit -->

- [ ] Read `CommandDefinitionsRepository.cs` — check if async file I/O (`SaveAsync`, `LoadAsync`) ever modifies ViewModel collections directly
- [ ] Verify that all collection mutations flow through the UI thread (Dispatcher.Invoke or similar) when called from background contexts
- [ ] Check `EditViewModel.SaveCommand` — does it marshal collection changes back to UI thread after repository save?
- [ ] If gaps are found, add `Dispatcher.Invoke` wrappers around collection modifications in async paths

### Verification Plan
```powershell
# Static review only: grep for collection mutations in async methods
grep_search("ProcessGroups\.", isRegexp=false, includePattern="**/CommandDefinitionsRepository.cs")
grep_search("\.Add\(|\.Remove\(|\.Clear()", isRegexp=true, includePattern="**/EditViewModel.cs")
```
Expected: All collection mutations either happen on UI thread or are protected by EnableCollectionSynchronization lock object.

### Phase Summary
_(write when phase completes)_

---

## Phase 5: Update README with edit-mode documentation
Status: Not started   <!-- README only mentions "open Notepad", no edit-mode UI docs -->

- [ ] Add section to `README.md` describing the in-app Edit Mode (toggle button, TreeView/ListView interface)
- [ ] Document key capabilities: add/remove/reorder groups and processes, undo/redo, detail pane editing
- [ ] List keyboard shortcuts if any exist (check XAML for InputBindings or KeyBinding)
- [ ] Document config schema fields relevant to edit mode (`Hidden`, `Disabled`, `CanLaunch`, `ExecutionMode`)

### Verification Plan
```powershell
# Verify README mentions Edit Mode
Select-String -Path "README.md" -Pattern "[Ee]dit [Mm]ode" | Select-Object LineNumber, Line
```
Expected: ≥1 mention of edit mode with description of the UI feature.

### Phase Summary
_(write when phase completes)_

---

## Phase 6: Enable `dotnet test` in CI pipeline
Status: Not started   <!-- dotnet.yml exists but test step is commented out -->

- [ ] Uncomment the test step in `.github/workflows/dotnet.yml`:
  ```yaml
  - name: Test
    run: dotnet test --no-build --verbosity normal
  ```
- [ ] Verify that `Publish-Debug.cmd` and `Publish-Release.cmd` still work (they should not need changes, they're publish-only scripts)

### Verification Plan
```powershell
# Confirm test step is uncommented
Select-String -Path ".github/workflows/dotnet.yml" -Pattern "dotnet test" | Select-Object LineNumber, Line
```
Expected: `dotnet test` appears as an active (uncommented) step in the workflow.

### Phase Summary
_(write when phase completes)_

---

## Already Completed (Verified During Audit)

The following items were marked "Not-started" or "In-progress" but are **already implemented**:

| Item | Description | Evidence |
|------|-------------|----------|
| #4 | Edit View (XAML) with TreeView + ListView | `EditView.xaml` — full layout with drag-and-drop, toolbar, split panes |
| #5 | Toolbar toggle button | `MainWindow.xaml` line 38 — ToggleButton bound to `EditListCommand` / `IsEditMode` |
| #6 | Right-hand detail pane (group + process editors) | `ProcessEditor.xaml` and `ProcessGroupEditor.xaml` with property editing for all fields |
| #7 | Drag-and-drop reordering | `EditView.xaml.cs` — full DnD handlers for both groups and processes with index calculation |
| #1,2,3 | Data model, repository, ViewModel | Core infrastructure fully implemented |
| #13 | Async repository methods | `LoadAsync`, `SaveAsync` with async helpers |
| #15 | Duplicate ID detection | `ValidateGroup` with `HashSet<Guid>` check |
| #16 | Circular reference guard | `ValidateCircularReference` in repository |
| #17 | Graceful handling of missing/invalid config | Returns empty default list on load failure |
| #19 | Undo/Redo support | Snapshot-based undo/redo with 12 stack tests passing |

---

## Final Recap
_(write when all phases complete: summary of the entire piece of work)_

## Deployment Plan
_(write when all phases complete: step-by-step deployment instructions)_
