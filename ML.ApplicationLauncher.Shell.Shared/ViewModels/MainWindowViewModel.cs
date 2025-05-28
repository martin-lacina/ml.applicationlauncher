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
    private readonly IConfigurationLocationProvider<ProcessGroup[]> _configurationProvider;
    private readonly IProcessLauncher _processLauncher;
    private readonly IProcessListProvider _processListProvider;
    private readonly ICommandFactory _commandFactory;
    private readonly IMyDialogService _dialogService;

    public MainWindowViewModel(
        IConfigurationLocationProvider<ProcessGroup[]> configurationProvider,
        IProcessListProvider processListProvider,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory,
        IMyDialogService dialogService)
    {
        _configurationProvider = configurationProvider.ShouldNotBeNull();
        _processLauncher = processLauncher.ShouldNotBeNull();
        _processListProvider = processListProvider.ShouldNotBeNull();
        _commandFactory = commandFactory.ShouldNotBeNull();
        _dialogService = dialogService.ShouldNotBeNull();

        ExitCommand = new DelegateCommand(Exit);
        LoadListCommand = new AsyncDelegateCommand(LoadListAsync);
        EditListCommand = new AsyncDelegateCommand(EditListAsync);
        ClearLastExecutedTimeCommand = new DelegateCommand(ClearLastExecutedTime);
        ShowAboutDialogCommand = new DelegateCommand(ShowAboutDialog);

        LoadListCommand.Execute();

        Task.Run(async () => await ExpireLastExecutionTimeLoopAsync(CancellationToken.None));
    }

    public DelegateCommand ExitCommand { get; }
    public AsyncDelegateCommand LoadListCommand { get; }
    public AsyncDelegateCommand EditListCommand { get; }
    public DelegateCommand ClearLastExecutedTimeCommand { get; }
    public DelegateCommand ShowAboutDialogCommand { get; }

    public ObservableCollection<ProcessGroupViewModel> ProcessGroups { get; } = new();

    private async Task LoadListAsync(CancellationToken cancellationToken)
    {
        var processGroups = await _processListProvider
            .LoadProcessGroupsAsync(cancellationToken)
            .Select(pg => new ProcessGroupViewModel(pg, _processLauncher, _commandFactory))
            .ToListAsync(cancellationToken);

        ProcessGroups.Clear();
        ProcessGroups.AddRange(processGroups);
    }

    private async Task EditListAsync(CancellationToken cancellationToken)
    {
        var editCommand =
            new ProcessLaunchInformation("Edit list", string.Empty, "notepad.exe", new[] { _configurationProvider.ConfigurationFilePath }, ExecutionMode.Raw);

        await _processLauncher.StartAsync(editCommand, cancellationToken);
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

    private void ShowAboutDialog()
    {
        _dialogService.ShowAboutDialog();
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
