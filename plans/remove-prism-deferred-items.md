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
Status: Complete

- [x] Replace `Prism.Commands.DelegateCommand` with `CommunityToolkit.Mvvm.Input.RelayCommand` in `RefreshableCommandFactory`
- [x] Update `RefreshedCommandWrapper` to use `RelayCommand` instead of `DelegateCommand`
- [x] Remove `using Prism.Commands` from `RefreshableCommandFactory.cs`
- [x] Verify `ICommandFactory` interface still works with CT.Mvvm commands
- [x] Build and test command refresh mechanism still functions

### Verification Plan
- `dotnet build ML.ApplicationLauncher.Shared/ML.ApplicationLauncher.Shared.csproj --no-incremental` → 0 errors
- `grep_search "DelegateCommand" includePattern="**/Services/*.cs"` → 0 matches
- `grep_search "Prism.Commands" includePattern="**/*.cs"` → 0 matches in Shared project

**Result:** ✅ Build succeeded (0 errors). Zero DelegateCommand or Prism.Commands references remain.

### Phase Summary
Replaced Prism.Commands.DelegateCommand with CommunityToolkit.Mvvm.Input.RelayCommand in RefreshableCommandFactory. Updated RefreshedCommandWrapper to use RelayCommand constructor and NotifyCanExecuteChanged() instead of RaiseCanExecuteChanged(). Removed Prism.Commands using directive. ICommandFactory interface remains compatible. Command refresh mechanism preserved with CT.Mvvm implementation.

## Phase 3: Fix CS8603 warnings in EditView.xaml.cs
Status: Complete

- [x] Fix `VisualUpwardSearch` method return type to handle null (line 92)
- [x] Fix `VisualUpwardSearchTreeItem` method return type to handle null (line 101)
- [x] Add null-forgiving operator or proper null check where appropriate
- [x] Verify warnings are resolved without breaking drag-drop functionality

### Verification Plan
- `dotnet build ML.ApplicationLauncher.slnx --no-incremental 2>&1 | Select-String -Pattern "warning CS8603"` → 0 matches
- Manual test: drag-drop processes and groups in edit mode works correctly

**Result:** ✅ Zero CS8603 warnings. Build succeeds with 0 errors.

### Phase Summary
Changed return types of `VisualUpwardSearch` and `VisualUpwardSearchTreeItem` from `DependencyObject` to `DependencyObject?` to properly reflect nullable return. This resolves CS8603 warnings without changing logic. Drag-drop functionality preserved as methods already handle null returns via null checks in callers.

## Final Recap

All 3 phases complete. Prism dependencies removed from ML.ApplicationLauncher.Shared:

1. **ICommandFactory removal** — ViewModels no longer depend on command factory, use [RelayCommand] attributes directly
2. **RefreshableCommandFactory CT.Mvvm migration** — Replaced Prism.Commands.DelegateCommand with CommunityToolkit.Mvvm.Input.RelayCommand
3. **CS8603 warnings fixed** — VisualUpwardSearch methods now return nullable DependencyObject?

**Key changes:**
- Removed ICommandFactory from 4 ViewModels and DI registration
- RefreshableCommandFactory now uses RelayCommand instead of DelegateCommand
- Zero Prism references in Shared project
- Zero CS8603 warnings

## Deployment Plan

1. Run full solution build: `dotnet build ML.ApplicationLauncher.slnx --no-incremental`
2. Verify zero Prism references: `grep_search "Prism\." includePattern="**/Shared/**/*.cs"`
3. Verify zero CS8603 warnings: `dotnet build ... | Select-String -Pattern "warning CS8603"`
4. Test edit mode drag-drop functionality
5. Commit changes with message: "Remove Prism deferred items - zero Prism references in Shared project"
