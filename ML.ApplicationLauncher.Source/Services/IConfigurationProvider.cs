// Copyright © Martin Lacina

namespace ML.ApplicationLauncher.Source.Services;

public interface IConfigurationProvider
{
    string ConfigurationFilePath { get; }

    string ConfigurationFileName { get; }
}
