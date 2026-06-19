using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ML.ApplicationLauncher.Source
{
    /// <summary>
    /// Represents a launchable process.
    /// </summary>
    public partial class CommandProcess : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        string _name = string.Empty;

        [ObservableProperty]
        string _path = string.Empty;

        // Free-form arguments string for editor compatibility. Persisted configuration uses ProcessLaunchInformation.Arguments (string[]).
        [ObservableProperty]
        string _arguments = string.Empty;

        // Extended fields to mirror ProcessLaunchInformation
        [ObservableProperty]
        string _comment = string.Empty;

        [ObservableProperty]
        Model.ExecutionMode _executionMode = Model.ExecutionMode.Default;

        [ObservableProperty]
        bool _disabled = false;

        [ObservableProperty]
        bool _hidden = false;

        [ObservableProperty]
        string _workingDirectory = string.Empty;
    }
}
