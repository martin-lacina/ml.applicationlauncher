// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

public static class ProcessStarterExtension
{
    public static async Task StartAsync(this IProcessLauncher processLauncher, ProcessGroup processGroup, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (ProcessGroup group in processGroup.Groups)
        {
            await processLauncher.StartAsync(group, cancellationToken).ConfigureAwait(false);
        }

        foreach (ProcessLaunchInformation process in processGroup.Processes)
        {
            await processLauncher.StartAsync(process, cancellationToken).ConfigureAwait(false);
        }
    }
}
