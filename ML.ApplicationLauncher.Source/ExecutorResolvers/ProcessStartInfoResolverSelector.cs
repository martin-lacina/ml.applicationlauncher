// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class ProcessStartInfoResolverSelector : IProcessStartInfoResolverSelector
{
    private readonly Dictionary<ExecutionMode, IProcessStartInfoResolver> _resolvers = new()
    {
        [ExecutionMode.PowerShell] = new WindowsTerminalPowerShellExecutableResolver(),
        [ExecutionMode.Direct] = new WindowsTerminalDirectExecutableResolver(),
        [ExecutionMode.Raw] = new RawExecutableProcessStartInfoResolver(),
        [ExecutionMode.Standalone] = new StandaloneExecutableProcessStartInfoResolver(),
        [ExecutionMode.CmdScript] = new WindowsTerminalCmdScriptExecutableProcessStartInfoResolver(),
        [ExecutionMode.PowerShellScript] = new WindowsTerminalPowerShellScriptExecutableResolver(false),
        [ExecutionMode.PowerShellCoreScript] = new WindowsTerminalPowerShellScriptExecutableResolver(true),
    };

    public Task<IProcessStartInfoResolver> SelectProcessStartInfoResolverAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        if (_resolvers.TryGetValue(processToLaunch.ExecutionMode, out var result))
            return Task.FromResult(result);

        throw new InvalidOperationException($"Missing process start info resolver for {processToLaunch.ExecutionMode}");
    }
}
