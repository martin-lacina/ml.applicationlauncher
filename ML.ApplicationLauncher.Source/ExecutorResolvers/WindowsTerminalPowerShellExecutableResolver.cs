// Copyright © Martin Lacina

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class WindowsTerminalPowerShellExecutableResolver : WindowsTerminalExecutableResolverBase
{
    public WindowsTerminalPowerShellExecutableResolver(WindowsTerminalConfig config)
        : base(ExecutionMode.PowerShell, config)
    {
    }

    protected override Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult("PowerShell");
    }

    protected override Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var executableArguments = $"\"{processToLaunch.Executable}\" {processToLaunch.GetArgumentsForProcessLaunch()}";
        
        return Task.FromResult(executableArguments);
    }
}
