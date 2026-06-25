using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ML.ApplicationLauncher.Core;
using ML.ApplicationLauncher.Core;
using ML.ApplicationLauncher.Shared.Services;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    /// <summary>
    /// ViewModel for editing a command group definition with launch capability.
    /// </summary>
    public partial class CommandGroupViewModel : ValidatableViewModelBase
    {
        private readonly IProcessLauncher? _processLauncher;
        public LastExecutedTracker LastExecutedTracker { get; } = new();

        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        string _name = string.Empty;

        [ObservableProperty]
        string _comment = string.Empty;

        [ObservableProperty]
        bool _canLaunch = false;

        [ObservableProperty]
        bool _disabled = false;

        [ObservableProperty]
        bool _hidden = false;

        public ObservableCollection<CommandGroupViewModel> Children { get; } = new();
        public ObservableCollection<CommandProcessViewModel> Processes { get; } = new();

        public string DisplayName => Name;

        public TimeOnly? LastExecuted => LastExecutedTracker.LastExecuted;

        /// <summary>
        /// Creates a new group view model with launch capability.
        /// </summary>
        public CommandGroupViewModel(IProcessLauncher processLauncher, ICommandFactory commandFactory)
        {
            _processLauncher = processLauncher;
        }

        /// <summary>
        /// Legacy parameterless constructor for serialization/editing scenarios where launcher is not available.
        /// </summary>
        public CommandGroupViewModel()
        {
            _processLauncher = null!;
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartAsync()
        {
            await LaunchAllProcessesAsync();
            LastExecutedTracker.SetLastExecuted();

            foreach (var child in Children)
                child.LastExecutedTracker.SetLastExecuted();

            foreach (var process in Processes)
                process.LastExecutedTracker.SetLastExecuted();
        }

        private bool CanStart() => GetLaunchableProcesses().Any();

        private async Task LaunchAllProcessesAsync()
        {
            var tasks = new System.Collections.Generic.List<Task>();

            foreach (var process in Processes.Where(p => !p.Disabled && File.Exists(p.Path)))
            {
                var info = new ProcessLaunchInformation(
                    process.Name,
                    process.Comment,
                    process.Path,
                    ArgumentExtensions.ParseArguments(process.Arguments),
                    process.ExecutionMode,
                    process.Disabled,
                    process.Hidden,
                    string.IsNullOrEmpty(process.WorkingDirectory) ? null : process.WorkingDirectory);

                tasks.Add(_processLauncher!.StartAsync(info));
            }

            foreach (var child in Children)
            {
                if (child.GetLaunchableProcesses().Any())
                {
                    tasks.Add(child.StartAsync());
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets all launchable processes in this group and its children recursively.
        /// </summary>
        public System.Collections.Generic.IEnumerable<CommandProcessViewModel> GetLaunchableProcesses()
        {
            var result = Processes.Where(p => !p.Disabled && File.Exists(p.Path));

            foreach (var child in Children)
            {
                result = result.Concat(child.GetLaunchableProcesses());
            }

            return result;
        }

        public bool CanBeStarted => GetLaunchableProcesses().Any();

        partial void OnNameChanged(string value)
        {
            SetValidation(nameof(Name), string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
        }
    }
}

