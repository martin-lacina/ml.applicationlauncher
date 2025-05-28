// Copyright © Martin Lacina

using System;

namespace ML.ApplicationLauncher.Source.Model;

public record ProcessLaunchInformation(
    string DisplayName,
    string Comment,
    string Executable,
    string[] Arguments,
    ExecutionMode ExecutionMode,
    bool Disabled = false,
    bool Hidden = false,
    string? WorkingDirectory = null);
