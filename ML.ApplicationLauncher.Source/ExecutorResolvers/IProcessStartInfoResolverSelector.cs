// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

public interface IProcessStartInfoResolverSelector
{
    Task<IProcessStartInfoResolver> SelectProcessStartInfoResolverAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);
}
