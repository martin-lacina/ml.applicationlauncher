// Copyright © Martin Lacina

using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source.Model;

public enum ExecutionMode
{
    Default = 0,
    PowerShell = Default,
    Direct = 1,
    Standalone = 2,
    Raw = 3,
    CmdScript = 4
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
