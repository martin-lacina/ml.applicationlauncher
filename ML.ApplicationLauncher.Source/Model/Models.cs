// Copyright © Martin Lacina

using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source.Model;

public enum ExecutionMode
{
    Default = 0,
    PowerShell = Default,
    Direct = 1,
    Standalone = 2,
    Raw = 3
}

public record ProcessGroup(string DisplayName, bool CanLaunch, IEnumerable<ProcessGroup> Groups, IEnumerable<ProcessLaunchInformation> Processes, bool Disabled = false);

public record ProcessLaunchInformation(string DisplayName, string Executable, string[] Arguments, ExecutionMode ExecutionMode, bool Disabled = false);
