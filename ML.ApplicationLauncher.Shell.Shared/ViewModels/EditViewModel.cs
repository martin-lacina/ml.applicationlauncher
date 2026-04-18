using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO;
using System.Windows.Input;
using ML.ApplicationLauncher.Source;

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
        private CommandGroupViewModel? _selectedGroup;
        public CommandGroupViewModel? SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedDetail)); }
        }

        private CommandProcessViewModel? _selectedProcess;
        public CommandProcessViewModel? SelectedProcess
        {
            get => _selectedProcess;
            set { _selectedProcess = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedDetail)); }
        }

        public object? SelectedDetail => (object?)SelectedProcess ?? SelectedGroup;

        public ICommand ToggleEditCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand AddProcessCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        private readonly Stack<string> _undoStack = new();
        private readonly Stack<string> _redoStack = new();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public EditViewModel(string repositoryPath)
        {
            _repository = new CommandDefinitionsRepository(repositoryPath);
            ToggleEditCommand = new RelayCommand(_ => IsEditMode = !IsEditMode);
            AddGroupCommand = new RelayCommand(_ => AddGroup());
            AddProcessCommand = new RelayCommand(_ => AddProcess());
            RemoveCommand = new RelayCommand(_ => RemoveSelected());
            MoveUpCommand = new RelayCommand(_ => MoveSelected(up: true));
            MoveDownCommand = new RelayCommand(_ => MoveSelected(up: false));
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
            // UI state is intentionally not persisted to disk for edit mode.
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
            PushUndo();
            var newGroup = new CommandGroupViewModel { Name = "New Group" };
            Groups.Add(newGroup);
            SelectedGroup = newGroup;
        }

        private void AddProcess()
        {
            if (SelectedGroup == null) return;
            PushUndo();
            var proc = new CommandProcessViewModel { Name = "New Process" };
            SelectedGroup.Processes.Add(proc);
            SelectedProcess = proc;
        }

        private void RemoveSelected()
        {
            if (SelectedProcess != null && SelectedGroup != null)
            {
                PushUndo();
                SelectedGroup.Processes.Remove(SelectedProcess);
                SelectedProcess = null;
                return;
            }

            if (SelectedGroup != null)
            {
                PushUndo();
                // try to remove from root
                if (!RemoveGroupById(SelectedGroup.Id, Groups))
                {
                    // not found in root - nothing
                }
                SelectedGroup = null;
            }
        }

        private void MoveSelected(bool up)
        {
            if (SelectedProcess != null && SelectedGroup != null)
            {
                var list = SelectedGroup.Processes;
                var idx = list.IndexOf(SelectedProcess);
                if (idx < 0) return;
                var newIdx = up ? idx - 1 : idx + 1;
                if (newIdx < 0 || newIdx >= list.Count) return;
                PushUndo();
                list.Move(idx, newIdx);
                return;
            }

            if (SelectedGroup != null)
            {
                // find parent collection
                var parent = FindParentCollection(SelectedGroup.Id, Groups) ?? Groups;
                var idx = parent.IndexOf(SelectedGroup);
                if (idx < 0) return;
                var newIdx = up ? idx - 1 : idx + 1;
                if (newIdx < 0 || newIdx >= parent.Count) return;
                PushUndo();
                parent.Move(idx, newIdx);
            }
        }

        public void MoveProcess(Guid processId, int newIndex)
        {
            if (SelectedGroup == null) return;
            var list = SelectedGroup.Processes;
            var proc = list.FirstOrDefault(p => p.Id == processId);
            if (proc == null) return;
            var oldIndex = list.IndexOf(proc);
            if (oldIndex < 0) return;
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= list.Count) newIndex = list.Count - 1;
            if (oldIndex == newIndex) return;
            PushUndo();
            list.Move(oldIndex, newIndex);
            SelectedProcess = proc;
        }

        public void MoveGroup(Guid groupId, int newIndex)
        {
            var parent = FindParentCollection(groupId, Groups) ?? Groups;
            var group = parent.FirstOrDefault(g => g.Id == groupId);
            if (group == null) return;
            var oldIndex = parent.IndexOf(group);
            if (oldIndex < 0) return;
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= parent.Count) newIndex = parent.Count - 1;
            if (oldIndex == newIndex) return;
            PushUndo();
            parent.Move(oldIndex, newIndex);
            SelectedGroup = group;
        }

        private ObservableCollection<CommandGroupViewModel>? FindParentCollection(Guid id, ObservableCollection<CommandGroupViewModel> current)
        {
            foreach (var g in current)
            {
                if (g.Children.Any(c => c.Id == id)) return g.Children;
                var nested = FindParentCollection(id, g.Children);
                if (nested != null) return nested;
            }
            return null;
        }

        private bool RemoveGroupById(Guid id, ObservableCollection<CommandGroupViewModel> current)
        {
            var found = current.FirstOrDefault(g => g.Id == id);
            if (found != null)
            {
                current.Remove(found);
                return true;
            }
            foreach (var g in current)
            {
                if (RemoveGroupById(id, g.Children)) return true;
            }
            return false;
        }

        private async void Save()
        {
            var groups = new List<CommandGroup>();
            foreach (var vm in Groups)
                groups.Add(ToModel(vm));
            await _repository.SaveAsync(groups);
        }

        // UI state persistence intentionally omitted; edit mode is transient/in-memory.

        private CommandGroupViewModel? FindGroupById(Guid id, ObservableCollection<CommandGroupViewModel> current)
        {
            foreach (var g in current)
            {
                if (g.Id == id) return g;
                var found = FindGroupById(id, g.Children);
                if (found != null) return found;
            }
            return null;
        }

        // removed UiState class

        private void PushUndo()
        {
            try
            {
                _undoStack.Push(SerializeGroups());
                _redoStack.Clear();
            }
            catch { }
        }

        private string SerializeGroups()
        {
            var models = Groups.Select(g => ToModel(g)).ToList();
            return JsonSerializer.Serialize(models, _jsonOptions);
        }

        private void LoadFromJson(string json)
        {
            var groups = JsonSerializer.Deserialize<List<CommandGroup>>(json, _jsonOptions) ?? new List<CommandGroup>();
            Groups.Clear();
            foreach (var g in groups) Groups.Add(ToViewModel(g));
            SelectedGroup = null;
            SelectedProcess = null;
            OnPropertyChanged(nameof(SelectedGroup));
            OnPropertyChanged(nameof(SelectedProcess));
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

        private void Undo()
        {
            if (_undoStack.Count == 0) return;
            var snap = _undoStack.Pop();
            _redoStack.Push(SerializeGroups());
            LoadFromJson(snap);
        }

        private void Redo()
        {
            if (_redoStack.Count == 0) return;
            var snap = _redoStack.Pop();
            _undoStack.Push(SerializeGroups());
            LoadFromJson(snap);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
