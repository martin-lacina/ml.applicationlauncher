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
using ML.ApplicationLauncher.Shell.Shared.ViewModels;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IConfigurationLocationProvider<ProcessGroup[]> _configurationProvider;
    private readonly IConfigurationManager<ProcessGroup[]> _configurationManager;
    private readonly IProcessLauncher _processLauncher;
    private readonly IProcessListProvider _processListProvider;
    private readonly ICommandFactory _commandFactory;
    private readonly IMyDialogService _dialogService;
    private bool _isEditMode;

    public MainWindowViewModel(
        IConfigurationLocationProvider<ProcessGroup[]> configurationProvider,
        IConfigurationManager<ProcessGroup[]> configurationManager,
        IProcessListProvider processListProvider,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory,
        IMyDialogService dialogService)
    {
        _configurationProvider = configurationProvider.ShouldNotBeNull();
        _configurationManager = configurationManager.ShouldNotBeNull();
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
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private object? _editViewContent;
    public object? EditViewContent
    {
        get => _editViewContent;
        private set => SetProperty(ref _editViewContent, value);
    }

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
        if (IsEditMode)
        {
            var vm = new EditViewModel(_configurationManager);
            EditViewContent = vm;
            Console.WriteLine($"EditListAsync: EditViewContent set to: {EditViewContent?.GetType().FullName}");
            try
            {
                var dt = Application.Current?.TryFindResource(typeof(EditViewModel));
                Console.WriteLine(dt == null ? "EditListAsync: No DataTemplate found for EditViewModel" : $"EditListAsync: DataTemplate found for EditViewModel: {dt.GetType().FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditListAsync: Exception checking resources: {ex}");
            }
        }
        else
        {
            EditViewContent = null;
            Console.WriteLine("EditListAsync: Edit mode disabled; EditViewContent cleared.");
            await LoadListAsync(cancellationToken);
        }
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
