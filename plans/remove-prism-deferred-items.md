# Remove Prism Deferred Items from Shared Project

Eliminate remaining Prism dependencies from ML.ApplicationLauncher.Shared to achieve zero Prism references in the Shared project. This addresses deferred items from migrate-main-view-to-ctmvvm.md: ICommandFactory removal, RefreshableCommandFactory Prism DelegateCommand usage, and CS8603 warnings in EditView.xaml.cs.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to `Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue with zero context); run the phase's **Verification Plan** and record the result before moving on. When all phases are done, fill in **Final Recap** and **Deployment Plan**.

## Phase 1: Remove ICommandFactory from ViewModels
Status: Complete

- [x] Remove `ICommandFactory` parameter from `CommandGroupViewModel` constructor and field usage
- [x] Remove `ICommandFactory` parameter from `CommandProcessViewModel` constructor and field usage
- [x] Remove `ICommandFactory` from `EditViewModel` constructor, field, and `CreateGroupViewModel`/`CreateProcessViewModel` methods
- [x] Remove `ICommandFactory` from `MainWindowViewModel` constructor, field, and `ToViewModel` method
- [x] Update `ShellServiceInstaller` to remove `ICommandFactory` registration
- [x] Verify ViewModels use `[RelayCommand]` attributes only (no factory-created commands)

### Verification Plan
- `dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental` → 0 errors
- `grep_search "ICommandFactory" includePattern="**/ViewModels/*.cs"` → 0 matches
- `grep_search "ICommandFactory" includePattern="**/Dependencies/*.cs"` → 0 matches

**Result:** ✅ Build succeeded (0 errors). Grep confirms zero ICommandFactory references in ViewModels and Dependencies.

### Phase Summary
Removed ICommandFactory from all ViewModels and DI registration. CommandGroupViewModel and CommandProcessViewModel constructors now accept only IProcessLauncher. EditViewModel and MainWindowViewModel no longer depend on ICommandFactory. ShellServiceInstaller registration removed. All ViewModels now use [RelayCommand] attributes exclusively. No breaking changes to public APIs.

## Phase 2: Replace RefreshableCommandFactory Prism DelegateCommand with CT.Mvvm
Status: Not started

- [ ] Replace `Prism.Commands.DelegateCommand` with `CommunityToolkit.Mvvm.Input.RelayCommand` in `RefreshableCommandFactory`
- [ ] Update `RefreshedCommandWrapper` to use `RelayCommand` instead of `DelegateCommand`
- [ ] Remove `using Prism.Commands` from `RefreshableCommandFactory.cs`
- [ ] Verify `ICommandFactory` interface still works with CT.Mvvm commands
- [ ] Build and test command refresh mechanism still functions

### Verification Plan
- `dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental` → 0 errors
- `grep_search "DelegateCommand" includePattern="**/Services/*.cs"` → 0 matches
- `grep_search "Prism.Commands" includePattern="**/*.cs"` → 0 matches in Shared project

### Phase Summary
_(write when phase completes)_

## Phase 3: Fix CS8603 warnings in EditView.xaml.cs
Status: Not started

- [ ] Fix `VisualUpwardSearch` method return type to handle null (line 92)
- [ ] Fix `VisualUpwardSearchTreeItem` method return type to handle null (line 101)
- [ ] Add null-forgiving operator or proper null check where appropriate
- [ ] Verify warnings are resolved without breaking drag-drop functionality

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx --no-incremental 2>&1 | Select-String -Pattern "warning CS8603"` → 0 matches
- Manual test: drag-drop processes and groups in edit mode works correctly

### Phase Summary
_(write when phase completes)_

## Final Recap
_(write when all phases complete: summary of the entire piece of work)_

## Deployment Plan
_(write when all phases complete: step-by-step deployment instructions)_
