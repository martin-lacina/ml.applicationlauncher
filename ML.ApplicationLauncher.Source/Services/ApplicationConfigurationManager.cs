// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Configuration;

namespace ML.ApplicationLauncher.Source.Services;

internal class ApplicationConfigurationManager : ConfigurationManagerBase<ApplicationConfiguration>
{
    public ApplicationConfigurationManager(IConfigurationLocationProvider<ApplicationConfiguration> configurationProvider, IMessageService messageService)
        : base(configurationProvider, messageService)
    {
    }

    protected override ApplicationConfiguration DefaultConfiguration => ApplicationConfiguration.Default;
}
