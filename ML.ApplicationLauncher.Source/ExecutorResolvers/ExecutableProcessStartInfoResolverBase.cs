// Copyright © Martin Lacina

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal abstract class ExecutableProcessStartInfoResolverBase : IProcessStartInfoResolver
{
    public ExecutableProcessStartInfoResolverBase(ExecutionMode mode)
    {
        Mode = mode;
    }

    public ExecutionMode Mode { get; }

    public virtual bool CheckExistance => true;

    public virtual bool ApplyDelay => true;

    public async Task<ProcessStartInfo> ResolveAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var startExecutable = await GetStartExecutableAsync(processToLaunch, cancellationToken);
        var executableArguments = await GetExecutableArgumentsAsync(processToLaunch, cancellationToken);

        var processStartInfo = new ProcessStartInfo(startExecutable, executableArguments);
        if (!string.IsNullOrEmpty(processToLaunch.WorkingDirectory))
        {
            processStartInfo.WorkingDirectory = processToLaunch.WorkingDirectory;
        }
        else
        {
            processStartInfo.WorkingDirectory = Environment.CurrentDirectory;
        }
        return processStartInfo;
    }

    protected abstract Task<string> GetStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);

    protected abstract Task<string> GetExecutableArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);
}
