// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

/// <summary>
/// Bidirectional mapping between configuration models (ProcessGroup/ProcessLaunchInformation)
/// and persistence DTOs (CommandGroup/CommandProcess).
/// Lives in the Source project so it can access both model layers.
/// The mapper is dumb — no filtering, no validation, just pure conversion.
/// Collection methods are extension methods on this interface.
/// </summary>
public interface IProcessModelMapper
{
    /// <summary>
    /// Maps a single ProcessGroup to CommandGroup (recursive).
    /// </summary>
    CommandGroup MapToCommandGroup(ProcessGroup source);

    /// <summary>
    /// Maps a single CommandGroup to ProcessGroup (recursive, reverse direction).
    /// </summary>
    ProcessGroup MapToProcessGroup(CommandGroup source);
}
