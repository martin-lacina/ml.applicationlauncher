// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers;

internal class WindowsTerminalCmdScriptExecutableProcessStartInfoResolver : WindowsTerminalExecutableResolverBase
{
    public WindowsTerminalCmdScriptExecutableProcessStartInfoResolver()
        : base(ExecutionMode.CmdScript)
    {
    }

    public override bool CheckExistance => false;

    protected override Task<string> GetWindowsTerminalStartExecutableAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var executable = GetExecutableVariants(processToLaunch).FirstOrDefault(File.Exists) ?? throw new InvalidOperationException($"Executable not found {processToLaunch.Executable}");

        return Task.FromResult(executable);
    }

    protected override Task<string> GetWindowsTerminalArgumentsAsync(ProcessLaunchInformation processToLaunch, CancellationToken cancellationToken)
    {
        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(processToLaunch.WorkingDirectory))
            arguments.Add(processToLaunch.WorkingDirectory);
        else
            arguments.Add(Environment.CurrentDirectory);

        arguments.AddRange(processToLaunch.Arguments);
                
        return Task.FromResult(arguments.GetArgumentsForProcessLaunch());
    }

    private IEnumerable<string> GetExecutableVariants(ProcessLaunchInformation processToLaunch)
    {
        yield return Path.GetFullPath(processToLaunch.Executable);

        if (!string.IsNullOrEmpty(processToLaunch.WorkingDirectory))
        {
            yield return Path.Combine(processToLaunch.WorkingDirectory, processToLaunch.Executable);
        }

        yield return Path.Combine(Environment.CurrentDirectory, processToLaunch.Executable);

        using var processModule = Process.GetCurrentProcess().MainModule;
        var originalAppDirectory = Path.GetDirectoryName(processModule?.FileName);
        if (originalAppDirectory != null)
        {
            yield return Path.Combine(originalAppDirectory, processToLaunch.Executable);
        }
    }

}
