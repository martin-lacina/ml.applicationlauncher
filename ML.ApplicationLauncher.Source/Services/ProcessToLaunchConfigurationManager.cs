// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Services;

internal class ProcessToLaunchConfigurationManager : ConfigurationManagerBase<ProcessGroup[]>
{
    public ProcessToLaunchConfigurationManager(IConfigurationLocationProvider<ProcessGroup[]> configurationProvider, IMessageService messageService)
        : base(configurationProvider, messageService)
    {
    }

    protected override ProcessGroup[] DefaultConfiguration => [];
}
