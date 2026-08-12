// Copyright © Martin Lacina

using System;
using System.Linq;
using ML.ApplicationLauncher.Core;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

/// <summary>
/// Bidirectional mapping between configuration models (ProcessGroup/ProcessLaunchInformation)
/// and persistence DTOs (CommandGroup/CommandProcess).
/// The mapper is dumb — no filtering, no validation, just pure conversion.
/// Bulk collection operations are provided as extension methods on IProcessModelMapper.
/// </summary>
public class ProcessGroupMapper : IProcessModelMapper
{
    // ---------------------------------------------------------------------
    // Forward: ProcessGroup → CommandGroup
    // ---------------------------------------------------------------------

    public CommandGroup MapToCommandGroup(ProcessGroup source)
    {
        var cg = new CommandGroup
        {
            Id = Guid.CreateVersion7(),
            Name = source.DisplayName ?? string.Empty,
            Comment = source.Comment ?? string.Empty,
            CanLaunch = source.CanLaunch,
            Disabled = source.Disabled,
            Hidden = source.Hidden,
        };

        if (source.Groups != null)
        {
            foreach (var child in source.Groups)
                cg.ChildGroups.Add(MapToCommandGroup(child));
        }

        if (source.Processes != null)
        {
            foreach (var p in source.Processes)
            {
                cg.Processes.Add(new CommandProcess
                {
                    Id = Guid.CreateVersion7(),
                    Name = p.DisplayName ?? string.Empty,
                    Path = p.Executable ?? string.Empty,
                    Arguments = ArgumentExtensions.FormatArguments(p.Arguments),
                    Comment = p.Comment ?? string.Empty,
                    ExecutionMode = p.ExecutionMode,
                    Disabled = p.Disabled,
                    Hidden = p.Hidden,
                    WorkingDirectory = p.WorkingDirectory ?? string.Empty
                });
            }
        }

        return cg;
    }

    // ---------------------------------------------------------------------
    // Reverse: CommandGroup → ProcessGroup
    // ---------------------------------------------------------------------

    public ProcessGroup MapToProcessGroup(CommandGroup source)
    {
        var childGroups = source.ChildGroups.Select(MapToProcessGroup).ToArray();
        var processes = source.Processes.Select(MapToProcessLaunchInformation).ToArray();

        return new ProcessGroup
        {
            DisplayName = source.Name ?? string.Empty,
            Comment = source.Comment ?? string.Empty,
            CanLaunch = source.CanLaunch,
            Groups = childGroups,
            Processes = processes,
            Disabled = source.Disabled,
            Hidden = source.Hidden
        };
    }

    private static ProcessLaunchInformation MapToProcessLaunchInformation(CommandProcess p)
    {
        return new ProcessLaunchInformation
        {
            DisplayName = p.Name ?? string.Empty,
            Comment = p.Comment ?? string.Empty,
            Executable = p.Path ?? string.Empty,
            Arguments = ArgumentExtensions.ParseArguments(p.Arguments),
            ExecutionMode = p.ExecutionMode,
            Disabled = p.Disabled,
            Hidden = p.Hidden,
            WorkingDirectory = string.IsNullOrWhiteSpace(p.WorkingDirectory) ? null : p.WorkingDirectory
        };
    }
}
