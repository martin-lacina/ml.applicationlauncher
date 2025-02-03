// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

public interface IProcessListProvider
{
    IEnumerable<ProcessGroup> ProcessGroups { get; }
}

internal class ProcessListProvider : IProcessListProvider
{
    private readonly IMessageService _messageService;
    private readonly IConfigurationProvider _configurationProvider;

    public ProcessListProvider(IConfigurationProvider configurationProvider, IMessageService messageService)
    {
        _messageService = messageService.ShouldNotBeNull();
        _configurationProvider = configurationProvider.ShouldNotBeNull();
    }

    public IEnumerable<ProcessGroup> ProcessGroups => LoadConfiguration();

    private IEnumerable<ProcessGroup> LoadConfiguration()
    {
        try
        {
            var fileContent = File.ReadAllText(_configurationProvider.ConfigurationFilePath, Encoding.UTF8);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            IEnumerable<ProcessGroup> result = JsonSerializer.Deserialize<ProcessGroup[]>(fileContent, options) ?? Array.Empty<ProcessGroup>();

            result = Filter(result);
            return result;
        }
        catch (Exception ex)
        {
            _messageService.ShowError("Failed to load process list", ex);
            return Array.Empty<ProcessGroup>();
        }
    }

    private IEnumerable<ProcessGroup> Filter(IEnumerable<ProcessGroup> result)
    {
        foreach (ProcessGroup processGroup in result.Where(processGroup => !processGroup.Disabled))
        {
            var filtered = Filter(processGroup);
            yield return filtered;
        }
    }

    private ProcessGroup Filter(ProcessGroup processGroup)
    {
        return processGroup with
        {
            Groups = Filter(processGroup.Groups ?? Array.Empty<ProcessGroup>()),
            Processes = processGroup.Processes?.Where(process => !process.Disabled) ?? Array.Empty<ProcessLaunchInformation>()
        };
    }
}
