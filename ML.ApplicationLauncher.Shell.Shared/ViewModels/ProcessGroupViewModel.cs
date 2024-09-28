// Copyright © Martin Lacina

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Shell.Services;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public class ProcessGroupViewModel : ProcessViewModelBase
{
    private readonly ProcessGroup _processGroup;
    private readonly IProcessLauncher _processLauncher;
    private readonly ICommandFactory _commandFactory;

    public ProcessGroupViewModel(
        ProcessGroup processGroup,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory)
        : base(commandFactory)
    {
        _processGroup = processGroup;
        _processLauncher = processLauncher;
        _commandFactory = commandFactory;
        Populate(processGroup);
        CanBeStarted = _processGroup.CanLaunch;
    }

    public override string DisplayName => _processGroup.DisplayName;

    public ObservableCollection<ProcessViewModelBase> Children { get; } = new();

    protected override bool CanStartInternal() => _processGroup.CanLaunch;

    protected override void StartInternal()
    {
        SetLastExecuted();

        Task.Run(() => _processLauncher.StartAsync(_processGroup));

        foreach (var child in Children)
        {
            child.SetLastExecuted();
        }
    }

    private void Populate(ProcessGroup processGroup)
    {
        foreach (ProcessGroup group in processGroup.Groups)
        {
            Children.Add(new ProcessGroupViewModel(group, _processLauncher, _commandFactory));
        }

        foreach (ProcessLaunchInformation process in processGroup.Processes)
        {
            Children.Add(new ProcessViewModel(process, _processLauncher, _commandFactory));
        }
    }
}
