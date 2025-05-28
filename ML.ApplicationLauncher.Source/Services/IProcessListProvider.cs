// Copyright © Martin Lacina

using System.Collections.Generic;
using System.Threading;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

public interface IProcessListProvider
{
    IAsyncEnumerable<ProcessGroup> LoadProcessGroupsAsync(CancellationToken cancellationToken);
}
