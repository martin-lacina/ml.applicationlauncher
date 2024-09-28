// Copyright © Martin Lacina

using System;
using System.IO;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.Services;

internal class ConfigurationProvider : IConfigurationProvider
{
    public ConfigurationProvider()
    {
        ConfigurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"CommandDefinitions.json");
    }

    public string ConfigurationFilePath { get; }
}
