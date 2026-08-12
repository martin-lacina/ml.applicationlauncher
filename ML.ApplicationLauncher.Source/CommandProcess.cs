// Copyright © Martin Lacina

using System;

namespace ML.ApplicationLauncher.Source;

/// <summary>
/// Represents a launchable process.
/// This is a runtime DTO — never data-bound to UI and never directly serialized.
/// </summary>
public record CommandProcess
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    // Free-form arguments string for editor compatibility. Persisted configuration uses ProcessLaunchInformation.Arguments (string[]).
    public string Arguments { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public Model.ExecutionMode ExecutionMode { get; set; }
    public bool Disabled { get; set; }
    public bool Hidden { get; set; }
    public string WorkingDirectory { get; set; } = string.Empty;
}
