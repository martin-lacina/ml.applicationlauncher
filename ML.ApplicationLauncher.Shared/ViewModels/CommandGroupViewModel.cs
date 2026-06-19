using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    /// <summary>
    /// ViewModel for editing a command group definition.
    /// </summary>
    public partial class CommandGroupViewModel : ValidatableViewModelBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        string _name = string.Empty;

        public ObservableCollection<CommandGroupViewModel> Children { get; } = new();
        public ObservableCollection<CommandProcessViewModel> Processes { get; } = new();

        partial void OnNameChanged(string value)
        {
            SetValidation(nameof(Name), string.IsNullOrWhiteSpace(value) ? "Name cannot be empty." : null);
        }
    }
}

