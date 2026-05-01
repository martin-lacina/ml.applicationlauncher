using System;
using System.Collections.Generic;

namespace ML.ApplicationLauncher.Source
{
    /// <summary>
    /// Represents a launchable process.
    /// </summary>
    public class CommandProcess
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        // Free-form arguments string for editor compatibility. Persisted configuration uses ProcessLaunchInformation.Arguments (string[]).
        public string Arguments { get; set; } = string.Empty;

        // Extended fields to mirror ProcessLaunchInformation
        public string Comment { get; set; } = string.Empty;
        public Model.ExecutionMode ExecutionMode { get; set; } = Model.ExecutionMode.Default;
        public bool Disabled { get; set; } = false;
        public bool Hidden { get; set; } = false;
        public string WorkingDirectory { get; set; } = string.Empty;
    }
}
