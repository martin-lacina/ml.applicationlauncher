// Copyright © Martin Lacina

using System;
using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source;

/// <summary>
/// Represents a group of commands. Groups can contain nested groups and processes.
/// This is a persistence DTO — never data-bound to UI.
/// </summary>
public record CommandGroup
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool CanLaunch { get; set; }
    public bool Disabled { get; set; }
    public bool Hidden { get; set; }
    public List<CommandGroup> Children { get; init; } = new();
    public List<CommandProcess> Processes { get; init; } = new();
}
