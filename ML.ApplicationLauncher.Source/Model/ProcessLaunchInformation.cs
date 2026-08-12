// Copyright © Martin Lacina

using System;
using System.Text.Json.Serialization;

namespace ML.ApplicationLauncher.Source.Model;

/// <summary>
/// Represents a launchable process in the configuration file.
/// This is the serialization model — serialized to/from JSON.
/// </summary>
public record ProcessLaunchInformation
{
    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("Comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("Executable")]
    public string Executable { get; set; } = string.Empty;

    [JsonPropertyName("Arguments")]
    public string[] Arguments { get; set; } = Array.Empty<string>();

    [JsonPropertyName("ExecutionMode")]
    public ExecutionMode ExecutionMode { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("Hidden")]
    public bool Hidden { get; set; }

    [JsonPropertyName("WorkingDirectory")]
    public string? WorkingDirectory { get; set; }
}
