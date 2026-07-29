// Copyright © Martin Lacina

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ML.ApplicationLauncher.Core;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Shared.Services;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IConfigurationLocationProvider<ProcessGroup[]> _configurationProvider;
    private readonly IConfigurationManager<ProcessGroup[]> _configurationManager;
    private readonly IProcessModelMapper _mapper;
    private readonly IProcessLauncher _processLauncher;
    private readonly IProcessListProvider _processListProvider;
    private readonly ICommandFactory _commandFactory;
    private readonly IMyDialogService _dialogService;
    private readonly IMessageService _messageService;
    private bool _isEditMode;

    public MainWindowViewModel(
        IConfigurationLocationProvider<ProcessGroup[]> configurationProvider,
        IConfigurationManager<ProcessGroup[]> configurationManager,
        IProcessModelMapper mapper,
        IProcessListProvider processListProvider,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory,
        IMyDialogService dialogService,
        IMessageService messageService)
    {
        _configurationProvider = configurationProvider.ShouldNotBeNull();
        _configurationManager = configurationManager.ShouldNotBeNull();
        _mapper = mapper.ShouldNotBeNull();
        _processLauncher = processLauncher.ShouldNotBeNull();
        _processListProvider = processListProvider.ShouldNotBeNull();
        _commandFactory = commandFactory.ShouldNotBeNull();
        _dialogService = dialogService.ShouldNotBeNull();
        _messageService = messageService.ShouldNotBeNull();

        Task.Run(async () => await LoadListAsync(CancellationToken.None));

        Task.Run(async () => await ExpireLastExecutionTimeLoopAsync(CancellationToken.None));
    }


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

    /// <summary>
    /// Reference to the current edit session ViewModel for checking unsaved changes.
    /// </summary>
    private EditViewModel? _editViewModel;

    public ObservableCollection<CommandGroupViewModel> ProcessGroups { get; } = new();

    [RelayCommand]
    private async Task LoadListAsync(CancellationToken cancellationToken)
    {
        var processGroups = await _processListProvider
            .LoadProcessGroupsAsync(cancellationToken)
            .Where(pg => pg.IsVisible())
            .Select(pg => MapGroup(pg))
            .ToListAsync(cancellationToken);

        ProcessGroups.Clear();
        ProcessGroups.AddRange(processGroups);
    }

    private CommandGroupViewModel MapGroup(ProcessGroup group)
    {
        var vm = new CommandGroupViewModel(_processLauncher, _commandFactory)
        {
            Name = group.DisplayName,
            Comment = group.Comment,
            CanLaunch = group.CanLaunch,
            Disabled = group.Disabled,
            Hidden = group.Hidden,
        };

        foreach (var childGroup in group.Groups.Where(g => g.IsVisible()))
        {
            vm.Children.Add(MapGroup(childGroup));
        }

        foreach (var process in group.Processes.Where(p => p.IsVisible()))
        {
            vm.Processes.Add(MapProcess(process));
        }

        return vm;
    }

    private CommandProcessViewModel MapProcess(ProcessLaunchInformation process)
    {
        return new CommandProcessViewModel(_processLauncher, _commandFactory)
        {
            Name = process.DisplayName,
            Path = process.Executable,
            Arguments = ArgumentExtensions.FormatArguments(process.Arguments),
            Comment = process.Comment,
            ExecutionMode = process.ExecutionMode,
            Disabled = process.Disabled,
            Hidden = process.Hidden,
            WorkingDirectory = process.WorkingDirectory ?? string.Empty,
        };
    }

    [RelayCommand]
    private async Task EditListAsync(CancellationToken cancellationToken)
    {
        if (IsEditMode)
        {
            _editViewModel = new EditViewModel(_configurationManager, _mapper, _processLauncher, _commandFactory);
            EditViewContent = _editViewModel;
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
            // Check for unsaved changes before exiting edit mode
            if (_editViewModel?.HasUnsavedChanges == true)
            {
                var result = _messageService.ShowQuestion(
                    "You have unsaved changes. Do you want to exit without saving?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    // User chose not to exit — re-enable edit mode
                    IsEditMode = true;
                    return;
                }
            }

            _editViewModel = null;
            EditViewContent = null;
            Console.WriteLine("EditListAsync: Edit mode disabled; EditViewContent cleared.");
            await LoadListAsync(cancellationToken);
        }
    }

    [RelayCommand]
    private void ClearLastExecutedTime()
    {
        RunOnProcessGroups(clear: true);
    }

    [RelayCommand]
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
        RunOnProcessGroups(expire: true, expirationInterval: expirationInterval);
    }

    private void RunOnProcessGroups(bool clear = false, bool expire = false, TimeSpan? expirationInterval = null)
    {
        foreach (var processGroup in ProcessGroups)
        {
            VisitGroup(processGroup);
        }

        void VisitGroup(CommandGroupViewModel pg)
        {
            if (clear) pg.LastExecutedTracker.ClearLastExecuted();
            else if (expire && expirationInterval.HasValue) pg.LastExecutedTracker.ExpireLastExecuted(expirationInterval.Value);

            foreach (var childGroup in pg.Children)
            {
                VisitGroup(childGroup);
            }

            foreach (var process in pg.Processes)
            {
                if (clear) process.LastExecutedTracker.ClearLastExecuted();
                else if (expire && expirationInterval.HasValue) process.LastExecutedTracker.ExpireLastExecuted(expirationInterval.Value);
            }
        }
    }

    [RelayCommand]
    private static void Exit()
    {
        Application.Current?.MainWindow?.Close();
    }

    [RelayCommand]
    private async Task EditJsonDefinitionAsync(CancellationToken cancellationToken)
    {
        var editCommand = new ProcessLaunchInformation(
            "Edit JSON defition in Notepad",
            string.Empty,
            "notepad.exe",
            [_configurationProvider.ConfigurationFilePath],
            ExecutionMode.Raw);

        await _processLauncher.StartAsync(editCommand, cancellationToken);
    }
}
