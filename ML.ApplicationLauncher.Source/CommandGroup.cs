using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ML.ApplicationLauncher.Source
{
    /// <summary>
    /// Represents a group of commands. Groups can contain nested groups and processes.
    /// </summary>
    public partial class CommandGroup : ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        string _name = string.Empty;

        [ObservableProperty]
        List<CommandGroup> _children = new();

        [ObservableProperty]
        List<CommandProcess> _processes = new();
    }
}
