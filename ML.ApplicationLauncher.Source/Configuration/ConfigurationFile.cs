// Copyright © Martin Lacina

using System;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Configuration;

public record ConfigurationFile(
    ApplicationConfiguration ApplicationConfig,
    ProcessGroup[] Groups
    )
{
    public static ConfigurationFile Default => new ConfigurationFile(ApplicationConfiguration.Default, Array.Empty<ProcessGroup>());
}
