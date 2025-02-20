// Copyright © Martin Lacina

using System.IO;
using ML.ApplicationLauncher.Shell.Services;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public class ProcessViewModel : ProcessViewModelBase
{
    private readonly ProcessLaunchInformation _process;
    private readonly IProcessLauncher _processLauncher;

    public ProcessViewModel(
        ProcessLaunchInformation process,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory)
        : base(commandFactory)
    {
        _process = process;
        _processLauncher = processLauncher;
    }

    public override string DisplayName => _process.DisplayName;

    public override string Comment => $"{_process.Comment}{ (string.IsNullOrEmpty(_process.Comment) ? string.Empty : "\n") }Execution Mode: {_process.ExecutionMode}\n{_process.Executable} {_process.GetArgumentsForProcessLaunch()}" ;

    // TODO fix with common validator
    protected override bool CanStartInternal() => File.Exists(_process.Executable);

    protected override void StartInternal()
    {
        SetLastExecuted();

        _processLauncher.StartAsync(_process).ConfigureAwait(false);
    }
}
