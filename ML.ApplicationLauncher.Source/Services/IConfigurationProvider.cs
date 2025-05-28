// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;

namespace ML.ApplicationLauncher.Source.Services;

public interface IConfigurationProvider<TConfiguration>
{
    Task<TConfiguration> LoadConfigurationAsync(CancellationToken cancellationToken);
}
