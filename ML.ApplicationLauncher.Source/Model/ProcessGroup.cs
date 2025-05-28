// Copyright © Martin Lacina

using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source.Model;

public record ProcessGroup(
    string DisplayName,
    string Comment,
    bool CanLaunch,
    IEnumerable<ProcessGroup> Groups,
    IEnumerable<ProcessLaunchInformation> Processes,
    bool Disabled = false,
    bool Hidden = false);
