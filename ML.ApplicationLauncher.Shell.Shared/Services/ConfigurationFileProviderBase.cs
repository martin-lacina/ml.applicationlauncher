// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.Services;

internal abstract class ConfigurationFileProviderBase<T> : IConfigurationLocationProvider<T>
{
    private readonly ConfigurationFile _configurationFile;

    protected ConfigurationFileProviderBase(IMessageService messageService, params string[] configurationFileNames)
    {
        messageService.ShouldNotBeNull();
        configurationFileNames.ShouldNotBeNullOrEmpty();

        _configurationFile = BuildConfigurationFilePath(messageService, configurationFileNames);
    }

    public string ConfigurationFilePath => _configurationFile.Path;

    public string ConfigurationFileName => _configurationFile.FileName;

    private static ConfigurationFile BuildConfigurationFilePath(IMessageService messageService, IEnumerable<string> configurationFileNames)
    {
        var location = AppDomain.CurrentDomain.BaseDirectory;

        var options = configurationFileNames
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
