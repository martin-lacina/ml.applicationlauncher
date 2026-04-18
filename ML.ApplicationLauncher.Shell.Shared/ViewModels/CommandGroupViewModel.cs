using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
