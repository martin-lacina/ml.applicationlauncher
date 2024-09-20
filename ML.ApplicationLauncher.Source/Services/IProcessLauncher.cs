// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

public interface IProcessLauncher
{
    Task StartAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken = default);
}
