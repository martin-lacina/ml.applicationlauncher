// Copyright © Martin Lacina

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class WindowsTerminalPowerShellScriptExecutableResolver : WindowsTerminalExecutableResolverBase
{
    private readonly bool _isCore;

    public WindowsTerminalPowerShellScriptExecutableResolver(bool isCore)
        : base(isCore ? ExecutionMode.PowerShellCoreScript : ExecutionMode.PowerShellScript)
    {
        _isCore = isCore;
    }

    protected override Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult(_isCore ? "pwsh" : "PowerShell");
    }

    protected override Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var context = new WindowsTerminalPowerShellScriptExecutionContext(
            processToLaunch.DisplayName,
            processToLaunch.WorkingDirectory ?? Environment.CurrentDirectory,
            processToLaunch.Arguments);

        var serialized = System.Text.Json.JsonSerializer.Serialize(context);
        var base64encoded = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));

        var executable = Path.GetFullPath(processToLaunch.Executable);

        var executableArguments = $"\"{executable}\" -Base64Context {base64encoded}";

        return Task.FromResult(executableArguments);
    }

    private record WindowsTerminalPowerShellScriptExecutionContext(string Title, string WorkingDirectory, string[] Commands);
}
