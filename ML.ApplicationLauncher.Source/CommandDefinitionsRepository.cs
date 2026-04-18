using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ML.ApplicationLauncher.Source
{
    public class CommandDefinitionsRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public CommandDefinitionsRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<CommandGroup>> LoadAsync()
        {
            if (!File.Exists(_filePath)) return new List<CommandGroup>();
            var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<List<CommandGroup>>(json, _options) ?? new List<CommandGroup>();
        }

        public async Task SaveAsync(List<CommandGroup> groups)
        {
            var json = JsonSerializer.Serialize(groups, _options);
            await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
        }

        // ---------------------------------------------------------------------
        // Validation helpers
        // ---------------------------------------------------------------------
        private void ValidateGroup(CommandGroup group, HashSet<Guid> seenIds = null)
        {
            seenIds ??= new HashSet<Guid>();
            if (!seenIds.Add(group.Id))
                throw new InvalidOperationException($"Duplicate group Id detected: {group.Id}");
            foreach (var child in group.Children)
                ValidateGroup(child, seenIds);
            foreach (var proc in group.Processes)
            {
                if (!seenIds.Add(proc.Id))
                    throw new InvalidOperationException($"Duplicate process Id detected: {proc.Id}");
                if (string.IsNullOrWhiteSpace(proc.Path))
                    throw new ArgumentException("Process Path cannot be empty.");
            }
        }

        private void ValidateCircularReference(CommandGroup group, Guid? parentId = null)
        {
            if (parentId.HasValue && group.Id == parentId.Value)
                throw new InvalidOperationException("Circular reference detected in group hierarchy.");
            foreach (var child in group.Children)
                ValidateCircularReference(child, group.Id);
        }

        // ---------------------------------------------------------------------
        // CRUD operations
        // ---------------------------------------------------------------------
        public async Task AddProcessAsync(Guid groupId, CommandProcess process)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            var group = FindGroup(groups, groupId) ?? throw new KeyNotFoundException("Group not found");
            group.Processes.Add(process);
            ValidateGroup(group);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        public async Task RemoveProcessAsync(Guid groupId, Guid processId)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            var group = FindGroup(groups, groupId) ?? throw new KeyNotFoundException("Group not found");
            group.Processes.RemoveAll(p => p.Id == processId);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        public async Task MoveGroupAsync(Guid groupId, int newIndex)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            var group = groups.Find(g => g.Id == groupId) ?? throw new KeyNotFoundException("Group not found");
            groups.Remove(group);
            groups.Insert(newIndex, group);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        public async Task MoveProcessAsync(Guid groupId, Guid processId, int newIndex)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            var group = FindGroup(groups, groupId) ?? throw new KeyNotFoundException("Group not found");
            var proc = group.Processes.Find(p => p.Id == processId) ?? throw new KeyNotFoundException("Process not found");
            group.Processes.Remove(proc);
            group.Processes.Insert(newIndex, proc);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        private CommandGroup FindGroup(List<CommandGroup> groups, Guid id)
        {
            foreach (var g in groups)
            {
                if (g.Id == id) return g;
                var child = FindGroup(g.Children, id);
                if (child != null) return child;
            }
            return null;
        }

        // Simple add/remove helpers – real implementation would handle IDs and ordering
        public async Task AddGroupAsync(CommandGroup group)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            groups.Add(group);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        public async Task RemoveGroupAsync(Guid id)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            groups.RemoveAll(g => g.Id == id);
            await SaveAsync(groups).ConfigureAwait(false);
        }
    }
}
