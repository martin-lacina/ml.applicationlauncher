// Copyright © Martin Lacina

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ML.ApplicationLauncher.Core;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Shared.Services;
using ML.ApplicationLauncher.Source;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IConfigurationLocationProvider<ProcessGroup[]> _configurationProvider;
    private readonly IConfigurationManager<ProcessGroup[]> _configurationManager;
    private readonly CommandDefinitionsRepository _repository;
    private readonly IProcessModelMapper _mapper;
    private readonly IProcessLauncher _processLauncher;
    private readonly ICommandFactory _commandFactory;
    private readonly IMyDialogService _dialogService;
    private readonly IMessageService _messageService;
    private bool _isEditMode;

    public MainWindowViewModel(
        IConfigurationLocationProvider<ProcessGroup[]> configurationProvider,
        IConfigurationManager<ProcessGroup[]> configurationManager,
        CommandDefinitionsRepository repository,
        IProcessModelMapper mapper,
        IProcessLauncher processLauncher,
        ICommandFactory commandFactory,
        IMyDialogService dialogService,
        IMessageService messageService)
    {
        _configurationProvider = configurationProvider.ShouldNotBeNull();
        _configurationManager = configurationManager.ShouldNotBeNull();
        _repository = repository.ShouldNotBeNull();
        _mapper = mapper.ShouldNotBeNull();
        _processLauncher = processLauncher.ShouldNotBeNull();
        _commandFactory = commandFactory.ShouldNotBeNull();
        _dialogService = dialogService.ShouldNotBeNull();
        _messageService = messageService.ShouldNotBeNull();

        _ = RunSafe(LoadListAsync(CancellationToken.None), _messageService);

        _ = RunSafe(ExpireLastExecutionTimeLoopAsync(CancellationToken.None), _messageService);

        // Allow cross-thread collection changes (used by LoadListAsync running on background threads)
        BindingOperations.EnableCollectionSynchronization(ProcessGroups, new object());
    }

    /// <summary>
    /// Wraps a fire-and-forget task and surfaces any unhandled exceptions via the message service.
    /// </summary>
    private static async Task RunSafe(Task task, IMessageService messageService)
    {
        await Task.Yield();

        try
        {
            await task;
        }
        catch (Exception ex)
        {
            messageService.ShowError("An unexpected error occurred in a background task", ex);
        }
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
        await Task.Yield();

        var allGroups = await _repository.LoadAsync(cancellationToken).ConfigureAwait(false);

        // Filter by IsVisible (preserves disabled/hidden items from main view only)
        ProcessGroups.Clear();
        foreach (var g in allGroups.Where(g => !g.Hidden && HasVisibleDescendant(g)))
            ProcessGroups.Add(ToViewModel(g));
    }

    private static bool HasVisibleDescendant(CommandGroup group)
    {
        if (group.Processes.Any(p => !p.Hidden)) return true;
        return group.ChildGroups.Any(c => HasVisibleDescendant(c));
    }

    private CommandGroupViewModel ToViewModel(CommandGroup group, bool includeHidden = false)
    {
        var vm = new CommandGroupViewModel(_processLauncher, _commandFactory)
        {
            Id = group.Id,
            Name = group.Name,
            Comment = group.Comment,
            CanLaunch = group.CanLaunch,
            Disabled = group.Disabled,
            Hidden = group.Hidden,
        };
        foreach (var child in group.ChildGroups.Where(c => includeHidden || !c.Hidden))
            vm.Children.Add(ToViewModel(child, includeHidden));
        foreach (var proc in group.Processes.Where(p => includeHidden || !p.Hidden))
            vm.Processes.Add(new CommandProcessViewModel(_processLauncher, _commandFactory)
            {
                Id = proc.Id,
                Name = proc.Name,
                Path = proc.Path,
                Arguments = proc.Arguments,
                Comment = proc.Comment,
                ExecutionMode = proc.ExecutionMode,
                Disabled = proc.Disabled,
                Hidden = proc.Hidden,
                WorkingDirectory = proc.WorkingDirectory
            });
        return vm;
    }

    [RelayCommand]
    private async Task EditListAsync(CancellationToken cancellationToken)
    {
        if (IsEditMode)
        {
            _editViewModel = new EditViewModel(_configurationManager, _mapper, _processLauncher, _commandFactory);
            EditViewContent = _editViewModel;
            Debug.WriteLine($"EditListAsync: EditViewContent set to: {EditViewContent?.GetType().FullName}");
            try
            {
                var dt = Application.Current?.TryFindResource(typeof(EditViewModel));
                Debug.WriteLine(dt == null ? "EditListAsync: No DataTemplate found for EditViewModel" : $"EditListAsync: DataTemplate found for EditViewModel: {dt.GetType().FullName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditListAsync: Exception checking resources: {ex}");
            }
        }
        else
        {
            // Check for unsaved changes before exiting edit mode
            if (_editViewModel?.HasUnsavedChanges == true)
            {
                var result = _messageService.ShowQuestion(
                    "There are unsaved changes. Do you want to save them before closing edit mode?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // Save and close — re-enter edit mode temporarily so the command can execute
                        IsEditMode = true;
                        await _editViewModel.SaveCommand.ExecuteAsync(null);
                        IsEditMode = false;
                        _editViewModel = null;
                        EditViewContent = null;
                        Debug.WriteLine("EditListAsync: Changes saved; edit mode disabled.");
                        await LoadListAsync(cancellationToken);
                        return;

                    case MessageBoxResult.No:
                        // Discard and close
                        break;

                    case MessageBoxResult.Cancel:
                        // User cancelled — stay in edit mode
                        IsEditMode = true;
                        return;
                }
            }

            _editViewModel = null;
            EditViewContent = null;
            Debug.WriteLine("EditListAsync: Edit mode disabled; EditViewContent cleared.");
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
        var editCommand = new ProcessLaunchInformation
        {
            DisplayName = "Edit JSON defition in Notepad",
            Comment = string.Empty,
            Executable = "notepad.exe",
            Arguments = [_configurationProvider.ConfigurationFilePath],
            ExecutionMode = ExecutionMode.Raw
        };

        await _processLauncher.StartAsync(editCommand, cancellationToken);
    }
}
