// Copyright © Martin Lacina

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ML.ApplicationLauncher.Source.Services;

internal abstract class ConfigurationManagerBase<TConfiguration> : IConfigurationProvider<TConfiguration>, IConfigurationManager<TConfiguration>
{
    private readonly IConfigurationLocationProvider<TConfiguration> _configurationProvider;
    private readonly IMessageService _messageService;

    public ConfigurationManagerBase(IConfigurationLocationProvider<TConfiguration> configurationProvider, IMessageService messageService)
    {
        _configurationProvider = configurationProvider;
        _messageService = messageService;
    }

    protected abstract TConfiguration DefaultConfiguration { get; }

    public async Task<TConfiguration> LoadConfigurationAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configurationFilePath = _configurationProvider.ConfigurationFilePath;
        try
        {
            if (!File.Exists(configurationFilePath))
            {
                _messageService.ShowError($"Configuration file does not exist (Path: {configurationFilePath}");
                return DefaultConfiguration;
            }

            var fileContent = await File.ReadAllTextAsync(configurationFilePath, Encoding.UTF8, cancellationToken);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            TConfiguration configuration;
            configuration = JsonSerializer.Deserialize<TConfiguration>(fileContent, options) ?? DefaultConfiguration;

            return configuration;
        }
        catch (Exception ex)
        {
            _messageService.ShowError($"Failed to load configuration file (Path: {configurationFilePath}, Type: {typeof(TConfiguration).Name})", ex);
            return DefaultConfiguration;
        }
    }

    public async Task SaveConfigurationAsync(TConfiguration configuration, CancellationToken cancellationToken)
    {
        try
        {
            using var fileStream = File.OpenWrite(_configurationProvider.ConfigurationFilePath);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                MaxDepth = 512,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            await JsonSerializer.SerializeAsync<TConfiguration>(fileStream, configuration, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _messageService.ShowError($"Failed to save configuration file (Path: {_configurationProvider.ConfigurationFilePath})", ex);
        }
    }
}
