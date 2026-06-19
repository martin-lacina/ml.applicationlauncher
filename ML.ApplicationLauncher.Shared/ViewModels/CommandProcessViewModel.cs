using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    /// <summary>
    /// ViewModel for editing a command process definition.
    /// </summary>
    public partial class CommandProcessViewModel : ValidatableViewModelBase
    {
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

        partial void OnNameChanged(string value)
        {
            SetValidation(nameof(Name), string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
        }

        partial void OnPathChanged(string value)
        {
            SetValidation(nameof(Path), string.IsNullOrWhiteSpace(value) ? "Path cannot be empty." : null);
        }
    }
}
