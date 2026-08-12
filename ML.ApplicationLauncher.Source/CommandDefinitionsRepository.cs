using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;
using ML.ApplicationLauncher.Core;

namespace ML.ApplicationLauncher.Source
{
    public class CommandDefinitionsRepository
    {
        private readonly IConfigurationManager<ProcessGroup[]> _configurationManager;
        private readonly IProcessModelMapper _mapper;

        public CommandDefinitionsRepository(IConfigurationManager<ProcessGroup[]> configurationManager, IProcessModelMapper mapper)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<CommandGroup>> LoadAsync(CancellationToken cancellationToken = default)
        {
            var config = await _configurationManager.LoadConfigurationAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.MapToCommandGroups(config ?? Array.Empty<ProcessGroup>()).ToList();
        }

        public async Task SaveAsync(List<CommandGroup> groups, CancellationToken cancellationToken = default)
        {
            // Validate entire tree for duplicates and circular refs before persisting
            ValidateAll(groups);

            // Convert to ProcessGroup[] model for persistence via mapper
            var processGroups = _mapper.MapToProcessGroups(groups).ToArray();

            await _configurationManager.SaveConfigurationAsync(processGroups, cancellationToken).ConfigureAwait(false);
        }

        // ---------------------------------------------------------------------
        // Validation helpers
        // ---------------------------------------------------------------------
        private void ValidateGroup(CommandGroup group, HashSet<Guid>? seenIds = null)
        {
            seenIds ??= new HashSet<Guid>();
            if (!seenIds.Add(group.Id))
                throw new InvalidOperationException($"Duplicate group Id detected: {group.Id}");
            foreach (var child in group.ChildGroups)
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
            foreach (var child in group.ChildGroups)
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

        private CommandGroup? FindGroup(List<CommandGroup> groups, Guid id)
        {
            foreach (var g in groups)
            {
                if (g.Id == id) return g;
                var child = FindGroup(g.ChildGroups, id);
                if (child != null) return child;
            }
            return null;
        }

        // Simple add/remove helpers – real implementation would handle IDs and ordering
        public async Task AddGroupAsync(CommandGroup group)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            groups.Add(group);
            ValidateAll(groups);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        public async Task RemoveGroupAsync(Guid id)
        {
            var groups = await LoadAsync().ConfigureAwait(false);
            groups.RemoveAll(g => g.Id == id);
            await SaveAsync(groups).ConfigureAwait(false);
        }

        private void ValidateAll(List<CommandGroup> groups)
        {
            var seen = new HashSet<Guid>();
            foreach (var g in groups)
            {
                ValidateGroup(g, seen);
                ValidateCircularReference(g, null);
            }
        }

    }
}
