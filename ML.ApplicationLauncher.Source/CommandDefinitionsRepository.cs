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

namespace ML.ApplicationLauncher.Source
{
    public class CommandDefinitionsRepository
    {
        private readonly IConfigurationManager<ProcessGroup[]> _configurationManager;

        public CommandDefinitionsRepository(IConfigurationManager<ProcessGroup[]> configurationManager)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        }

        public async Task<List<CommandGroup>> LoadAsync()
        {
            var config = await _configurationManager.LoadConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
            return MapProcessGroupsToCommandGroups(config ?? Array.Empty<ProcessGroup>());
        }

        public async Task SaveAsync(List<CommandGroup> groups)
        {
            // Validate entire tree for duplicates and circular refs before persisting
            ValidateAll(groups);

            // Convert to new ProcessGroup[] model for persistence
            var processGroups = MapCommandGroupsToProcessGroups(groups);

            await _configurationManager.SaveConfigurationAsync(processGroups, CancellationToken.None).ConfigureAwait(false);
        }

        // ---------------------------------------------------------------------
        // Validation helpers
        // ---------------------------------------------------------------------
        private void ValidateGroup(CommandGroup group, HashSet<Guid>? seenIds = null)
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

        private CommandGroup? FindGroup(List<CommandGroup> groups, Guid id)
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

        // ---------------------------------------------------------------------
        // Mapping helpers between ProcessGroup (configuration model) and CommandGroup (editor model)
        // ---------------------------------------------------------------------
        private static List<CommandGroup> MapProcessGroupsToCommandGroups(IEnumerable<ProcessGroup> processGroups)
        {
            var result = new List<CommandGroup>();
            foreach (var pg in processGroups ?? Array.Empty<ProcessGroup>())
                result.Add(MapProcessGroupToCommandGroup(pg));
            return result;
        }

        private static CommandGroup MapProcessGroupToCommandGroup(ProcessGroup pg)
        {
            var cg = new CommandGroup
            {
                Id = Guid.NewGuid(),
                Name = pg.DisplayName ?? string.Empty
            };

            if (pg.Groups != null)
            {
                foreach (var child in pg.Groups)
                    cg.Children.Add(MapProcessGroupToCommandGroup(child));
            }

            if (pg.Processes != null)
            {
                foreach (var p in pg.Processes)
                {
                    cg.Processes.Add(new CommandProcess
                    {
                        Id = Guid.NewGuid(),
                        Name = p.DisplayName ?? string.Empty,
                        Path = p.Executable ?? string.Empty,
                        Arguments = (p.Arguments != null) ? string.Join(" ", p.Arguments) : string.Empty,
                        Comment = p.Comment ?? string.Empty,
                        ExecutionMode = p.ExecutionMode,
                        Disabled = p.Disabled,
                        Hidden = p.Hidden,
                        WorkingDirectory = p.WorkingDirectory ?? string.Empty
                    });
                }
            }

            return cg;
        }

        private static ProcessGroup[] MapCommandGroupsToProcessGroups(IEnumerable<CommandGroup> groups)
        {
            var list = new List<ProcessGroup>();
            foreach (var g in groups ?? new List<CommandGroup>())
                list.Add(MapCommandGroupToProcessGroup(g));
            return list.ToArray();
        }

        private static ProcessGroup MapCommandGroupToProcessGroup(CommandGroup g)
        {
            var childGroups = (g.Children ?? new List<CommandGroup>()).Select(MapCommandGroupToProcessGroup).ToArray();
            var processes = (g.Processes ?? new List<CommandProcess>()).Select(p =>
                new ProcessLaunchInformation(
                    p.Name ?? string.Empty,
                    p.Comment ?? string.Empty,
                    p.Path ?? string.Empty,
                    ParseArguments(p.Arguments),
                    p.ExecutionMode,
                    p.Disabled,
                    p.Hidden,
                    string.IsNullOrWhiteSpace(p.WorkingDirectory) ? null : p.WorkingDirectory
                )).ToArray();

            return new ProcessGroup(g.Name ?? string.Empty, string.Empty, true, childGroups, processes, false, false);
        }

        private static string[] ParseArguments(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return Array.Empty<string>();
            var result = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;
            for (int i = 0; i < args.Length; i++)
            {
                var c = args[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (sb.Length > 0)
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0)
                result.Add(sb.ToString());

            return result.ToArray();
        }
    }
}
