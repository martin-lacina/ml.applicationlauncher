// Copyright © Martin Lacina

using System.Collections.Generic;
using System.Linq;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

/// <summary>
/// Extension methods on IProcessModelMapper for bulk collection operations.
/// These delegate to the single-item converter, so there is no duplication.
/// </summary>
public static class ProcessModelMapperExtensions
{
    /// <summary>
    /// Maps an enumerable of ProcessGroups to CommandGroups.
    /// </summary>
    public static IEnumerable<CommandGroup> MapToCommandGroups(this IProcessModelMapper mapper, IEnumerable<ProcessGroup> sources)
    {
        return (sources ?? System.Array.Empty<ProcessGroup>())
            .Select(mapper.MapToCommandGroup);
    }

    /// <summary>
    /// Maps an enumerable of CommandGroups to ProcessGroups.
    /// </summary>
    public static IEnumerable<ProcessGroup> MapToProcessGroups(this IProcessModelMapper mapper, IEnumerable<CommandGroup> sources)
    {
        return (sources ?? System.Array.Empty<CommandGroup>())
            .Select(mapper.MapToProcessGroup);
    }
}
