using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    public class CommandProcessViewModel : ValidatableViewModelBase
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id { get => _id; set => SetProperty(ref _id, value); }

        private string _name = string.Empty;
        public string Name { get => _name; set { SetProperty(ref _name, value); ValidateProperty(nameof(Name), value); } }

        private string _path = string.Empty;
        public string Path { get => _path; set { SetProperty(ref _path, value); ValidateProperty(nameof(Path), value); } }

        private string _arguments = string.Empty;
        public string Arguments { get => _arguments; set => SetProperty(ref _arguments, value); }

        private string _comment = string.Empty;
        public string Comment { get => _comment; set => SetProperty(ref _comment, value); }

        private ExecutionMode _executionMode = ExecutionMode.Default;
        public ExecutionMode ExecutionMode { get => _executionMode; set => SetProperty(ref _executionMode, value); }

        private bool _disabled = false;
        public bool Disabled { get => _disabled; set => SetProperty(ref _disabled, value); }

        private bool _hidden = false;
        public bool Hidden { get => _hidden; set => SetProperty(ref _hidden, value); }

        private string _workingDirectory = string.Empty;
        public string WorkingDirectory { get => _workingDirectory; set => SetProperty(ref _workingDirectory, value); }
    }
}
