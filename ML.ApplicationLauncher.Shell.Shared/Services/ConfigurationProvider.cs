// Copyright © Martin Lacina

using System;
using System.IO;
using System.Linq;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.Services;

internal class ConfigurationProvider : IConfigurationProvider
{
    private static readonly string[] ConfigurationFileNames = new[]
    {
        @"CommandDefinitions.json",
        @"CommandDefinitions.examples.json"
    };

    private readonly ConfigurationFile _configurationFile;

    public ConfigurationProvider(IMessageService messageService)
    {
        _configurationFile = BuildConfigurationFilePath(messageService);
    }

    public string ConfigurationFilePath => _configurationFile.Path;

    public string ConfigurationFileName => _configurationFile.FileName;


    private static ConfigurationFile BuildConfigurationFilePath(IMessageService messageService)
    {
        var location = AppDomain.CurrentDomain.BaseDirectory;

        var options = ConfigurationFileNames
            .Select(fileName => new ConfigurationFile(fileName, Path.Combine(location, fileName)))
            .ToArray();

        var firstExisting = options.FirstOrDefault(file => File.Exists(file.Path));

        if (firstExisting is null)
        {
            var toCreate = options.First();
            messageService.ShowError($"No config file present in application directory ({string.Join(", ", options.Select(f => f.FileName))}), creating default {toCreate.FileName}.");
            File.WriteAllText(toCreate.Path, "[]");
            firstExisting = toCreate;
        }

        return firstExisting;
    }

    private record ConfigurationFile(string FileName, string Path);
}
