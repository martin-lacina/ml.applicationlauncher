// Copyright © Martin Lacina

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Shell.Services;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IProcessLauncher _processLauncher;
    private readonly IProcessListProvider _processListProvider;
    private readonly ICommandFactory _commandFactory;

    public MainWindowViewModel(
        IConfigurationProvider configurationProvider,
        IProcessListProvider processListProvider,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory)
    {
        _configurationProvider = configurationProvider.ShouldNotBeNull();
        _processLauncher = processLauncher.ShouldNotBeNull();
        _processListProvider = processListProvider.ShouldNotBeNull();
        _commandFactory = commandFactory.ShouldNotBeNull();

        ExitCommand = new DelegateCommand(Exit);
        LoadListCommand = new DelegateCommand(LoadList);
        EditListCommand = new DelegateCommand(EditList);
        ClearLastExecutedTimeCommand = new DelegateCommand(ClearLastExecutedTime);

        LoadList();

        Task.Run(async () => await ExpireLastExecutionTimeLoopAsync(CancellationToken.None));
    }

    public DelegateCommand ExitCommand { get; }
    public DelegateCommand LoadListCommand { get; }
    public DelegateCommand EditListCommand { get; }
    public DelegateCommand ClearLastExecutedTimeCommand { get; }

    public ObservableCollection<ProcessGroupViewModel> ProcessGroups { get; } = new();

    private void LoadList()
    {
        ProcessGroups.Clear();
        ProcessGroups.AddRange(_processListProvider.ProcessGroups.Select(pg => new ProcessGroupViewModel(pg, _processLauncher, _commandFactory)));
    }

    private void EditList()
    {
        var editCommand =
            new ProcessLaunchInformation("Edit list", "notepad.exe", new[] { _configurationProvider.ConfigurationFilePath }, ExecutionMode.Raw);

        _processLauncher.StartAsync(editCommand);
    }

    private void ClearLastExecutedTime()
    {
        RunOnProcessGroups(ClearLastExecuted, ClearLastExecuted);

        return;

        static void ClearLastExecuted<T>(T model) where T : ProcessViewModelBase
        {
            model.ClearLastExecuted();
        }
    }

    private async Task ExpireLastExecutionTimeLoopAsync(CancellationToken cancellationToken)
    {
        var expirationInterval = TimeSpan.FromHours(1);
        var checkInterval = TimeSpan.FromSeconds(15);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(checkInterval, cancellationToken);

            ExpireLastExecutionTime(expirationInterval);
        }
    }

    private void ExpireLastExecutionTime(TimeSpan expirationInterval)
    {
        RunOnProcessGroups(ExpireLastExecuted, ExpireLastExecuted);

        return;

        void ExpireLastExecuted<T>(T model) where T : ProcessViewModelBase
        {
            model.ExpireLastExecuted(expirationInterval);
        }
    }

    private void RunOnProcessGroups(Action<ProcessGroupViewModel> executeOnGroup, Action<ProcessViewModel> executeOnChild)
    {
        foreach (var processGroup in ProcessGroups)
        {
            RunOnGroup(processGroup);
        }

        return;

        void RunOnGroup(ProcessGroupViewModel pg)
        {
            foreach (var childGroup in pg.Children.OfType<ProcessGroupViewModel>())
            {
                executeOnGroup(childGroup);
                RunOnGroup(childGroup);
            }

            foreach (var childProcess in pg.Children.OfType<ProcessViewModel>())
            {
                executeOnChild(childProcess);
            }
        }
    }

    private static void Exit()
    {
        Application.Current?.MainWindow?.Close();
    }
}
