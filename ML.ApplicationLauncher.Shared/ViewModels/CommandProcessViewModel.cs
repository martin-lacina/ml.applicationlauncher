using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ML.ApplicationLauncher.Core;
using ML.ApplicationLauncher.Shared.Services;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    /// <summary>
    /// ViewModel for editing a command process definition with launch capability.
    /// </summary>
    public partial class CommandProcessViewModel : ValidatableViewModelBase
    {
        private readonly IProcessLauncher? _processLauncher;
        public LastExecutedTracker LastExecutedTracker { get; } = new();

        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        string _name = string.Empty;

        [ObservableProperty]
        string _path = string.Empty;

        [ObservableProperty]
        string _arguments = string.Empty;

        [ObservableProperty]
        string _comment = string.Empty;

        [ObservableProperty]
        ExecutionMode _executionMode = ExecutionMode.Default;

        [ObservableProperty]
        bool _disabled = false;

        [ObservableProperty]
        bool _hidden = false;

        [ObservableProperty]
        string _workingDirectory = string.Empty;

        public string DisplayName => Name;

        public bool CanBeStarted => !Disabled && File.Exists(Path);

        public TimeOnly? LastExecuted => LastExecutedTracker.LastExecuted;

        /// <summary>
        /// Creates a new process view model with launch capability.
        /// </summary>
        public CommandProcessViewModel(IProcessLauncher processLauncher, ICommandFactory commandFactory)
        {
            _processLauncher = processLauncher;
        }

        /// <summary>
        /// Legacy parameterless constructor for serialization/editing scenarios where launcher is not available.
        /// </summary>
        public CommandProcessViewModel()
        {
            _processLauncher = null!;
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartAsync()
        {
            var processInfo = BuildProcessLaunchInformation();
            await _processLauncher!.StartAsync(processInfo);
            SetLastExecuted();
        }

        public void SetLastExecuted()
        {
            LastExecutedTracker.SetLastExecuted();
            OnPropertyChanged(nameof(LastExecuted));
        }

        private bool CanStart() => !Disabled && File.Exists(Path);

        private ProcessLaunchInformation BuildProcessLaunchInformation()
        {
            return new ProcessLaunchInformation(
                Name,
                Comment,
                Path,
                ArgumentExtensions.ParseArguments(Arguments),
                ExecutionMode,
                Disabled,
                Hidden,
                string.IsNullOrEmpty(WorkingDirectory) ? null : WorkingDirectory);
        }



        partial void OnNameChanged(string value)
        {
            SetValidation(nameof(Name), string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
        }

        partial void OnPathChanged(string value)
        {
            SetValidation(nameof(Path), string.IsNullOrWhiteSpace(value) ? "Path cannot be empty." : null);
        }

        partial void OnDisabledChanged(bool value) => StartCommand.NotifyCanExecuteChanged();
    }
}
