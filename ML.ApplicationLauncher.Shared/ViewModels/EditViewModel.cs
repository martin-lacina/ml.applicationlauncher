using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ML.ApplicationLauncher.Source;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Shared.Services;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    /// <summary>
    /// ViewModel for the edit mode UI that allows users to modify command configuration.
    /// </summary>
    public partial class EditViewModel : ObservableObject
    {
        private readonly CommandDefinitionsRepository _repository;
        private readonly IProcessLauncher? _processLauncher;
        private readonly ICommandFactory? _commandFactory;

        [ObservableProperty]
        bool _isEditMode;

        public ObservableCollection<CommandGroupViewModel> Groups { get; } = new();

        [ObservableProperty]
        CommandGroupViewModel? _selectedGroup;

        partial void OnSelectedGroupChanged(CommandGroupViewModel? value) => OnPropertyChanged(nameof(SelectedDetail));

        [ObservableProperty]
        CommandProcessViewModel? _selectedProcess;

        partial void OnSelectedProcessChanged(CommandProcessViewModel? value) => OnPropertyChanged(nameof(SelectedDetail));

        public object? SelectedDetail => (object?)SelectedProcess ?? SelectedGroup;

        private readonly Stack<string> _undoStack = new();
        private readonly Stack<string> _redoStack = new();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public EditViewModel(IConfigurationManager<ProcessGroup[]> configurationManager, IProcessLauncher? processLauncher = null, ICommandFactory? commandFactory = null)
        {
            _repository = new CommandDefinitionsRepository(configurationManager);
            _processLauncher = processLauncher;
            _commandFactory = commandFactory;
            var _ = Task.Run(async () => await Load()).ConfigureAwait(false);
        }

        [RelayCommand]
        private void ToggleEdit() => IsEditMode = !IsEditMode;

        [RelayCommand]
        private void AddGroup()
        {
            PushUndo();
            var newGroup = CreateGroupViewModel();
            newGroup.Name = "New Group";
            Groups.Add(newGroup);
            SelectedGroup = newGroup;
        }

        [RelayCommand]
        private void AddProcess()
        {
            if (SelectedGroup == null) return;
            PushUndo();
            var proc = CreateProcessViewModel();
            proc.Name = "New Process";
            SelectedGroup.Processes.Add(proc);
            SelectedProcess = proc;
        }

        [RelayCommand]
        private void AddSubGroup()
        {
            if (SelectedGroup == null) return;
            PushUndo();
            var newGroup = CreateGroupViewModel();
            newGroup.Name = "New Subgroup";
            SelectedGroup.Children.Add(newGroup);
            SelectedGroup = newGroup;
        }

        [RelayCommand]
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

        [RelayCommand]
        private void MoveUp() => MoveSelectedInternal(up: true);

        [RelayCommand]
        private void MoveDown() => MoveSelectedInternal(up: false);

        [RelayCommand]
        private void RemoveProcess(CommandProcessViewModel process)
        {
            if (process == null || SelectedGroup == null) return;
            PushUndo();
            SelectedGroup.Processes.Remove(process);
            if (SelectedProcess == process)
                SelectedProcess = null;
        }

        private void MoveSelectedInternal(bool up)
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

        [RelayCommand]
        private async Task SaveAsync()
        {
            var groups = new List<CommandGroup>();
            foreach (var vm in Groups)
                groups.Add(ToModel(vm));
            await _repository.SaveAsync(groups);
        }

        [RelayCommand]
        private void Undo()
        {
            if (_undoStack.Count == 0) return;
            var snap = _undoStack.Pop();
            _redoStack.Push(SerializeGroups());
            LoadFromJson(snap);
        }

        [RelayCommand]
        private void Redo()
        {
            if (_redoStack.Count == 0) return;
            var snap = _redoStack.Pop();
            _undoStack.Push(SerializeGroups());
            LoadFromJson(snap);
        }

        private async Task Load()
        {
            var groups = await _repository.LoadAsync();
            Groups.Clear();
            foreach (var g in groups)
                Groups.Add(ToViewModel(g));

            // If no groups exist, create a default group and select it
            if (Groups.Count == 0)
            {
                Groups.Add(CreateGroupViewModel());
            }

            // Select the first group
            SelectedGroup = Groups.First();
        }

        private CommandGroupViewModel ToViewModel(CommandGroup group)
        {
            var vm = CreateGroupViewModel();
            vm.Id = group.Id;
            vm.Name = group.Name;
            foreach (var child in group.Children)
                vm.Children.Add(ToViewModel(child));
            foreach (var proc in group.Processes)
            {
                var processVm = CreateProcessViewModel();
                processVm.Id = proc.Id;
                processVm.Name = proc.Name;
                processVm.Path = proc.Path;
                processVm.Arguments = proc.Arguments;
                processVm.Comment = proc.Comment;
                processVm.ExecutionMode = proc.ExecutionMode;
                processVm.Disabled = proc.Disabled;
                processVm.Hidden = proc.Hidden;
                processVm.WorkingDirectory = proc.WorkingDirectory;
                vm.Processes.Add(processVm);
            }
            return vm;
        }

        private CommandGroupViewModel CreateGroupViewModel()
            => _processLauncher is not null && _commandFactory is not null
                ? new CommandGroupViewModel(_processLauncher, _commandFactory) { Name = "New Group" }
                : new CommandGroupViewModel() { Name = "New Group" };

        private CommandProcessViewModel CreateProcessViewModel()
            => _processLauncher is not null && _commandFactory is not null
                ? new CommandProcessViewModel(_processLauncher, _commandFactory) { Name = "New Process" }
                : new CommandProcessViewModel() { Name = "New Process" };

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
                    Arguments = procVm.Arguments,
                    Comment = procVm.Comment,
                    ExecutionMode = procVm.ExecutionMode,
                    Disabled = procVm.Disabled,
                    Hidden = procVm.Hidden,
                    WorkingDirectory = procVm.WorkingDirectory
                });
            return group;
        }
    }
}
