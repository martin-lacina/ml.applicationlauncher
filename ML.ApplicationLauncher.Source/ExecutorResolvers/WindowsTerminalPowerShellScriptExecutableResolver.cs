// Copyright © Martin Lacina

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.ExecutorResolvers.PowerShell;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class WindowsTerminalPowerShellScriptExecutableResolver : WindowsTerminalExecutableResolverBase
{
    private readonly bool _isCore;

    public WindowsTerminalPowerShellScriptExecutableResolver(WindowsTerminalConfig config, bool isPowerShellCore)
        : base(isPowerShellCore ? ExecutionMode.PowerShellCoreScript : ExecutionMode.PowerShellScript, config)
    {
        _isCore = isPowerShellCore;
    }

    protected override Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult(_isCore ? "pwsh" : "PowerShell");
    }

    protected override Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var base64encoded = new PowerShellScriptExecutionContext(
            processToLaunch.DisplayName,
            processToLaunch.WorkingDirectory ?? Environment.CurrentDirectory,
            processToLaunch.Arguments).EncodeToBase64();

        var executable = Path.GetFullPath(processToLaunch.Executable);

        var executableArguments = $"\"{executable}\" -Base64Context {base64encoded}";

        return Task.FromResult(executableArguments);
    }
}
