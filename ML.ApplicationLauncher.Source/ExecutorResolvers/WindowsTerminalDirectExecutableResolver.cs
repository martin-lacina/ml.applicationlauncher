// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class WindowsTerminalDirectExecutableResolver : WindowsTerminalExecutableResolverBase
{
    public WindowsTerminalDirectExecutableResolver()
        : base(ExecutionMode.Direct)
    {
    }
    protected override Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult(processToLaunch.Executable);
    }

    protected override Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var executableArguments = processToLaunch.GetArgumentsForProcessLaunch();

        return Task.FromResult(executableArguments);
    }
}
