# Fix Argument Splitting in Edit Mode Preview Panel

The edit mode preview panel splits arguments by whitespace instead of by newline, causing multi-word arguments like `Write-Host $env:ComputerName` to be broken into individual tokens.

## For Future Agents
As work proceeds: mark checkboxes `- [x]` as items complete; when a phase is done, set its status to `Complete` and write its **Phase Summary** (what was done, key decisions, anything needed to continue with zero context); run the phase's **Verification Plan** and record the result before moving on. When all phases are done, fill in **Final Recap** and **Deployment Plan**.

## Phase 1: Fix BuildProcessLaunchInformation in CommandProcessViewModel
Status: Complete   <!-- Not started | In progress | Complete -->

- [x] Replace `Arguments.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries)` with `ArgumentExtensions.ParseArguments(Arguments)` in `BuildProcessLaunchInformation()`

### Verification Plan
- Build Shared project → **PASS**, 0 errors

### Phase Summary
Replaced inline whitespace split with shared helper call. Added `using ML.ApplicationLauncher.Shared.Extensions;` for the new helper.

## Phase 2: Fix LaunchAllProcessesAsync in CommandGroupViewModel
Status: Complete   <!-- Not started | In progress | Complete -->

- [x] Replace `Arguments.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries)` with `ArgumentExtensions.ParseArguments(process.Arguments)` in `LaunchAllProcessesAsync()`

### Verification Plan
- Build Shared project → **PASS**, 0 errors

### Phase Summary
Same fix as Phase 1 — replaced inline whitespace split with shared helper call. Added `using ML.ApplicationLauncher.Shared.Extensions;` for the new helper.

## Phase 3: Extract shared helper to avoid duplication
Status: Complete   <!-- Not started | In progress | Complete -->

- [x] Created `ArgumentExtensions.ParseArguments(string args)` in `ML.ApplicationLauncher.Shared/Extensions/ArgumentExtensions.cs`
- [x] Replaced both inline splits with calls to the shared helper

### Verification Plan
- Build Shared project → **PASS**, 0 errors
- Grep for remaining `Arguments.Split` in ViewModels → **0 matches** (all replaced by helper)

### Phase Summary
Created new static helper class matching the existing `ParseArguments` pattern from `CommandDefinitionsRepository`. Uses `\n` split with `RemoveEmptyEntries | TrimEntries` flags. Both ViewModels now delegate to this single source of truth.

## Final Recap

All 3 phases complete. The argument splitting bug is fixed in both launch paths (process and group). A shared helper prevents future regressions. Build passes clean (0 errors, 4 pre-existing warnings unrelated to this change).

**Changes:**
- `CommandProcessViewModel.cs`: replaced inline split with `ArgumentExtensions.ParseArguments(Arguments)`
- `CommandGroupViewModel.cs`: replaced inline split with `ArgumentExtensions.ParseArguments(process.Arguments)`
- `ArgumentExtensions.cs`: new shared helper (newline split + trim, matching repository pattern)

## Deployment Plan

1. Stop running app instance (locks DLL files during build)
2. Run full solution build: `dotnet build ML.ApplicationLauncher.slnx --no-incremental`
3. Test PowerShell Core script launch from edit mode preview panel — each argument line should execute as a single command, not be split by whitespace
4. Commit changes with message: "fix: use newline-separated argument parsing in ViewModel launch paths"
