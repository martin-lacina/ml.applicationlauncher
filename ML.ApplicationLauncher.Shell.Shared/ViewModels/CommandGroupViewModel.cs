using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shell.Shared.ViewModels
{
    public class CommandGroupViewModel : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        public ObservableCollection<CommandGroupViewModel> Children { get; } = new();
        public ObservableCollection<CommandProcessViewModel> Processes { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CommandProcessViewModel : INotifyPropertyChanged
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _path = string.Empty;
        public string Path { get => _path; set { _path = value; OnPropertyChanged(); } }

        private string _arguments = string.Empty;
        public string Arguments { get => _arguments; set { _arguments = value; OnPropertyChanged(); } }

        private string _comment = string.Empty;
        public string Comment { get => _comment; set { _comment = value; OnPropertyChanged(); } }

        private ExecutionMode _executionMode = ExecutionMode.Default;
        public ExecutionMode ExecutionMode { get => _executionMode; set { _executionMode = value; OnPropertyChanged(); } }

        private bool _disabled = false;
        public bool Disabled { get => _disabled; set { _disabled = value; OnPropertyChanged(); } }

        private bool _hidden = false;
        public bool Hidden { get => _hidden; set { _hidden = value; OnPropertyChanged(); } }

        private string _workingDirectory = string.Empty;
        public string WorkingDirectory { get => _workingDirectory; set { _workingDirectory = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
