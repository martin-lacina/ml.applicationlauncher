// Copyright © Martin Lacina

using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source.Model;

public enum ExecutionMode
{
    // Run in Windows Terminal
    Default = 0,
    PowerShell = Default,
    PowerShellScript,
    PowerShellCoreScript,
    CmdScript,

    // Run without Windows Terminal
    Direct,
    Standalone,
    Raw,
}

public record ProcessGroup(
    string DisplayName,
    string Comment,
    bool CanLaunch,
    IEnumerable<ProcessGroup> Groups,
    IEnumerable<ProcessLaunchInformation> Processes,
    bool Disabled = false,
    bool Hidden = false);

public record ProcessLaunchInformation(
    string DisplayName,
    string Comment,
    string Executable,
    string[] Arguments,
    ExecutionMode ExecutionMode,
    bool Disabled = false,
    bool Hidden = false,
    string? WorkingDirectory = null);
