// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

internal class ProcessListProvider : IProcessListProvider
{
    private readonly IConfigurationProvider<ProcessGroup[]> _configurationProvider;

    public ProcessListProvider(IConfigurationProvider<ProcessGroup[]> configurationProvider)
    {
        _configurationProvider = configurationProvider.ShouldNotBeNull();
    }

    public async IAsyncEnumerable<ProcessGroup> LoadProcessGroupsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var config = await _configurationProvider.LoadConfigurationAsync(cancellationToken);

        foreach (var group in config)
        {
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return group;
            }
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
            DisplayName = processGroup.DisplayName ?? string.Empty,
            Comment = processGroup.Comment ?? string.Empty,
            Groups = Filter(processGroup.Groups ?? Array.Empty<ProcessGroup>()),
            Processes = Filter(processGroup.Processes),
        };
    }

    private IEnumerable<ProcessLaunchInformation> Filter(IEnumerable<ProcessLaunchInformation> processLaunchInformation)
    {
        if (processLaunchInformation == null)
            return Array.Empty<ProcessLaunchInformation>();

        return processLaunchInformation.Where(process => !process.Disabled).Select(process => process with
        {
            DisplayName = process.DisplayName ?? string.Empty,
            Comment = process.Comment ?? string.Empty,
            WorkingDirectory = process.WorkingDirectory ?? Environment.CurrentDirectory,
            Executable = process.Executable ?? string.Empty,
            Arguments = process.Arguments ?? Array.Empty<string>(),
        });
    }
}
