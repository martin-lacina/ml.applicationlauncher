// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.Services;

internal class ApplicationConfigurationProvider : ConfigurationFileProviderBase<ApplicationConfiguration>
{
    private static readonly string[] ConfigurationFileNames = new[]
    {
        @"appsettings.json",
        @"appsettings.template.json"
    };

    public ApplicationConfigurationProvider(IMessageService messageService)
        : base(messageService, ConfigurationFileNames)
    {        
    }
}
