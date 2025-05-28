// Copyright © Martin Lacina

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
