// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal abstract class WindowsTerminalExecutableResolverBase : ExecutableProcessStartInfoResolverBase
{
    private const string WindowsTerminalExecutable = "wt.exe";

    public WindowsTerminalExecutableResolverBase(ExecutionMode mode)
        : base(mode)
    {
    }

    protected override Task<string> GetStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult(WindowsTerminalExecutable);
    }

    protected override async Task<string> GetExecutableArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var startExecutable = await GetWindowsTerminalStartExecutableAsync(processToLaunch, cancellationToken);
        var executableArguments = await GetWindowsTerminalArgumentsAsync(processToLaunch, cancellationToken);

        var displayName = processToLaunch.DisplayName;

        var terminalArguments = $" -w ML.ApplicationLauncher new-tab --title \"{displayName}\" \"{startExecutable}\" {executableArguments}";

        return terminalArguments;
    }

    protected abstract Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);

    protected abstract Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);
}
