// Copyright © Martin Lacina

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ML.ApplicationLauncher.Source.Model;

/// <summary>
/// Represents a group of processes in the configuration file.
/// This is the serialization model — serialized to/from JSON.
/// </summary>
public record ProcessGroup
{
    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("Comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("CanLaunch")]
    public bool CanLaunch { get; set; }

    [JsonPropertyName("Groups")]
    public IEnumerable<ProcessGroup> Groups { get; set; } = Enumerable.Empty<ProcessGroup>();

    [JsonPropertyName("Processes")]
    public IEnumerable<ProcessLaunchInformation> Processes { get; set; } = Enumerable.Empty<ProcessLaunchInformation>();

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("Hidden")]
    public bool Hidden { get; set; }
}
