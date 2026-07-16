// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.Services;

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
            var defaultContent = GetDefaultConfigurationJson(typeof(T));

            messageService.ShowError($"No config file present in application directory ({string.Join(", ", options.Select(f => f.FileName))}), creating default {toCreate.FileName}.");
            
            // Backup any existing file before overwriting (safety net for partially corrupted files)
            var backupPath = toCreate.Path + ".bak";
            if (File.Exists(backupPath))
                File.Delete(backupPath);
            if (File.Exists(toCreate.Path))
                File.Move(toCreate.Path, backupPath);

            // Validate the default content is valid JSON before writing it
            try
            {
                JsonDocument.Parse(defaultContent);
            }
            catch (JsonException ex)
            {
                messageService.ShowError($"Invalid default configuration template for type {typeof(T).Name}: {ex.Message}");
            }

            File.WriteAllText(toCreate.Path, defaultContent);
            firstExisting = toCreate;
        }

        return firstExisting;
    }

    private static string GetDefaultConfigurationJson(Type configurationType)
    {
        // Check if TConfiguration is an array type (e.g., ProcessGroup[])
        if (configurationType.IsArray)
            return "[]";

        // For object types, use empty JSON object which deserializes to default values
        return "{}";
    }

    private record ConfigurationFile(string FileName, string Path);
}
