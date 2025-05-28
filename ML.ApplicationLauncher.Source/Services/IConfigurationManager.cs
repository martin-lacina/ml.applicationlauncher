// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;

namespace ML.ApplicationLauncher.Source.Services;

public interface IConfigurationManager<TConfiguration> : IConfigurationProvider<TConfiguration>
{
    Task SaveConfigurationAsync(TConfiguration configuration, CancellationToken cancellationToken);
}
