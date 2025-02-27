// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class StandaloneExecutableProcessStartInfoResolver : ExecutableProcessStartInfoResolverBase
{
    public StandaloneExecutableProcessStartInfoResolver()
        : base(ExecutionMode.Standalone)
    {
    }

    protected StandaloneExecutableProcessStartInfoResolver(ExecutionMode executionMode)
        : base(executionMode)
    {
    }

    public override bool ApplyDelay => false;

    protected override Task<string> GetStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult(processToLaunch.Executable);
    }

    protected override Task<string> GetExecutableArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        return Task.FromResult(processToLaunch.GetArgumentsForProcessLaunch());
    }
}
