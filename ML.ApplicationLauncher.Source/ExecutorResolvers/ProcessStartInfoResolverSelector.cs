// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class ProcessStartInfoResolverSelector : IProcessStartInfoResolverSelector
{
    private readonly Dictionary<ExecutionMode, IProcessStartInfoResolver> _resolvers;

    public ProcessStartInfoResolverSelector(WindowsTerminalConfig config)
    {
        _resolvers = Buildxecutors(config);
    }

    public Task<IProcessStartInfoResolver> SelectProcessStartInfoResolverAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        if (_resolvers.TryGetValue(processToLaunch.ExecutionMode, out var result))
            return Task.FromResult(result);

        throw new InvalidOperationException($"Missing process start info resolver for {processToLaunch.ExecutionMode}");
    }

    private static Dictionary<ExecutionMode, IProcessStartInfoResolver> Buildxecutors(WindowsTerminalConfig windowsTerminalConfig) => new()
    {
        [ExecutionMode.PowerShell] = new WindowsTerminalPowerShellExecutableResolver(windowsTerminalConfig),
        [ExecutionMode.Direct] = new WindowsTerminalDirectExecutableResolver(windowsTerminalConfig),
        [ExecutionMode.Raw] = new RawExecutableProcessStartInfoResolver(),
        [ExecutionMode.Standalone] = new StandaloneExecutableProcessStartInfoResolver(),
        [ExecutionMode.CmdScript] = new WindowsTerminalCmdScriptExecutableProcessStartInfoResolver(windowsTerminalConfig),
        [ExecutionMode.PowerShellScript] = new WindowsTerminalPowerShellScriptExecutableResolver(windowsTerminalConfig, false),
        [ExecutionMode.PowerShellCoreScript] = new WindowsTerminalPowerShellScriptExecutableResolver(windowsTerminalConfig, true),
    };
}
