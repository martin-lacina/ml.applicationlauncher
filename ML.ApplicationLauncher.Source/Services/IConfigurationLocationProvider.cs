// Copyright © Martin Lacina

namespace ML.ApplicationLauncher.Source.Services;

public interface IConfigurationLocationProvider<TConfig>
{
    string ConfigurationFilePath { get; }

    string ConfigurationFileName { get; }
}
