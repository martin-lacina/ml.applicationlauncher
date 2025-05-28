// Copyright © Martin Lacina

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal abstract class WindowsTerminalExecutableResolverBase : ExecutableProcessStartInfoResolverBase
{
    private const string WindowsTerminalExecutable = "wt.exe";
    private readonly WindowsTerminalConfig _config;

    public WindowsTerminalExecutableResolverBase(ExecutionMode mode, WindowsTerminalConfig config)
        : base(mode)
    {
        _config = config;
    }

    protected override async Task<string> GetStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        if (_config.Enable)
            return WindowsTerminalExecutable;

        return await GetWindowsTerminalStartExecutableAsync(processToLaunch, cancellationToken);
    }

    protected override async Task<string> GetExecutableArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        if (_config.Enable)
        {
            var startExecutable = await GetWindowsTerminalStartExecutableAsync(processToLaunch, cancellationToken);
            var executableArguments = await GetWindowsTerminalArgumentsAsync(processToLaunch, cancellationToken);

            var displayName = processToLaunch.DisplayName;
            var windowId = Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.WindowId));

            var terminalArguments = $" -w {windowId} new-tab --title \"{displayName}\" \"{startExecutable}\" {executableArguments}";

            return terminalArguments;
        }

        return await GetWindowsTerminalArgumentsAsync(processToLaunch, cancellationToken);
    }

    protected abstract Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);

    protected abstract Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);
}
