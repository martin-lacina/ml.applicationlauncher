// Copyright © Martin Lacina

using System.IO;
using System.Reflection;

namespace ML.ApplicationLauncher.Source.Services;

public interface IConfigurationProvider
{
    string ConfigurationFilePath { get; }
}

internal class ConfigurationProvider : IConfigurationProvider
{
    public ConfigurationProvider()
    {
        ConfigurationFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, @"DefaultEnvironment.json");
    }

    public string ConfigurationFilePath { get; }
}
