// Copyright © Martin Lacina

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;
public interface IProcessStartInfoResolver
{
    ExecutionMode Mode { get; }

    bool CheckExistance { get; }

    bool ApplyDelay { get; }

    Task<ProcessStartInfo> ResolveAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken);
}
