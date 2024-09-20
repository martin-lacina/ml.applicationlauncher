// Copyright © Martin Lacina

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

internal class ProcessLauncher : IProcessLauncher, IDisposable
{
    private DateTime _lastStartup = DateTime.MinValue;
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(3);

    private readonly ActionBlock<Func<CancellationToken, Task>> _actionExecutor;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ProcessLauncher()
    {
        _actionExecutor = new(ProcessActionAsync, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
    }

    public async Task StartAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        if (processToLaunch.ExecutionMode != ExecutionMode.Raw && !File.Exists(processToLaunch.Executable))
            return;

        var executableArguments = string.Join(" ", processToLaunch.Arguments.Select(a => $"\"{a}\""));

        bool applyDelay = true;
        string executable = "wt.exe";
        switch (processToLaunch.ExecutionMode)
        {
            case ExecutionMode.PowerShell:
                executableArguments = $" -w VBO new-tab --title \"{processToLaunch.DisplayName}\" PowerShell -c \"{processToLaunch.Executable} {executableArguments}\"";
                break;

            case ExecutionMode.Direct:
                executableArguments = $" -w VBO new-tab --title \"{processToLaunch.DisplayName}\" \"{processToLaunch.Executable}\" {executableArguments}";
                break;

            case ExecutionMode.Standalone:
            case ExecutionMode.Raw:
                executable = processToLaunch.Executable;
                applyDelay = false;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var processStartInfo = new ProcessStartInfo(executable, executableArguments);



        if (applyDelay)
        {
            _actionExecutor.Post(DelayExecutionAsync);
        }

        _actionExecutor.Post(async ct => await StartProcessAsync(processStartInfo, ct));

    }

    private async Task ProcessActionAsync(Func<CancellationToken, Task> action)
    {
        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

        await action(_cancellationTokenSource.Token);
    }

    private async Task DelayExecutionAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(ComputeDelay(), cancellationToken);
    }

    private Task StartProcessAsync(ProcessStartInfo processStartInfo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Process.Start(processStartInfo)?.Dispose();

        return Task.CompletedTask;
    }

    private TimeSpan ComputeDelay()
    {
        var now = DateTime.UtcNow;

        var nextRun = _lastStartup.Add(_delay);

        _lastStartup = DateTime.UtcNow;

        if (nextRun < now)
            return TimeSpan.Zero;

        return nextRun - now;
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
