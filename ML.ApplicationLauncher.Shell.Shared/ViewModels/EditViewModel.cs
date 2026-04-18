using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ML.ApplicationLauncher.Shell.Shared.ViewModels
{
    public class EditViewModel : INotifyPropertyChanged
    {
        private readonly CommandDefinitionsRepository _repository;
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CommandGroupViewModel> Groups { get; } = new();
        public ICommand ToggleEditCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand AddProcessCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand MoveCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public EditViewModel(string repositoryPath)
        {
            _repository = new CommandDefinitionsRepository(repositoryPath);
            ToggleEditCommand = new RelayCommand(_ => IsEditMode = !IsEditMode);
            AddGroupCommand = new RelayCommand(_ => AddGroup());
            AddProcessCommand = new RelayCommand(_ => AddProcess());
            RemoveCommand = new RelayCommand(_ => RemoveSelected());
            MoveCommand = new RelayCommand(_ => MoveSelected());
            SaveCommand = new RelayCommand(_ => Save());
            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
            Load();
        }

        private async void Load()
        {
            var groups = await _repository.LoadAsync();
            Groups.Clear();
            foreach (var g in groups)
                Groups.Add(ToViewModel(g));
        }

        private CommandGroupViewModel ToViewModel(CommandGroup group)
        {
            var vm = new CommandGroupViewModel
            {
                Id = group.Id,
                Name = group.Name
            };
            foreach (var child in group.Children)
                vm.Children.Add(ToViewModel(child));
            foreach (var proc in group.Processes)
                vm.Processes.Add(new CommandProcessViewModel
                {
                    Id = proc.Id,
                    Name = proc.Name,
                    Path = proc.Path,
                    Arguments = proc.Arguments
                });
            return vm;
        }

        private void AddGroup()
        {
            var newGroup = new CommandGroupViewModel { Name = "New Group" };
            Groups.Add(newGroup);
        }

        private void AddProcess()
        {
            // placeholder – actual selection logic omitted
        }

        private void RemoveSelected()
        {
            // placeholder – actual selection logic omitted
        }

        private void MoveSelected()
        {
            // placeholder – actual selection logic omitted
        }

        private async void Save()
        {
            var groups = new List<CommandGroup>();
            foreach (var vm in Groups)
                groups.Add(ToModel(vm));
            await _repository.SaveAsync(groups);
        }

        private CommandGroup ToModel(CommandGroupViewModel vm)
        {
            var group = new CommandGroup
            {
                Id = vm.Id,
                Name = vm.Name
            };
            foreach (var child in vm.Children)
                group.Children.Add(ToModel(child));
            foreach (var procVm in vm.Processes)
                group.Processes.Add(new CommandProcess
                {
                    Id = procVm.Id,
                    Name = procVm.Name,
                    Path = procVm.Path,
                    Arguments = procVm.Arguments
                });
            return group;
        }

        private void Undo() { /* placeholder */ }
        private void Redo() { /* placeholder */ }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
