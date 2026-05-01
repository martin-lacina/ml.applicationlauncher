using System;
using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source
{
    /// <summary>
    /// Represents a group of commands. Groups can contain nested groups and processes.
    /// </summary>
    public class CommandGroup
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<CommandGroup> Children { get; set; } = new();
        public List<CommandProcess> Processes { get; set; } = new();
    }
}
