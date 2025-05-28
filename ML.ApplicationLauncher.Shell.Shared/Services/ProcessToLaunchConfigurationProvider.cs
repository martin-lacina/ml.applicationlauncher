// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ML.ApplicationLauncher.Core.Validation;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.Services;

internal class ProcessToLaunchConfigurationProvider : ConfigurationFileProviderBase<ProcessGroup[]>
{
    private static readonly string[] ConfigurationFileNames = new[]
    {
        @"CommandDefinitions.json",
        @"CommandDefinitions.examples.json"
    };

    public ProcessToLaunchConfigurationProvider(IMessageService messageService)
        : base(messageService, ConfigurationFileNames)
    {
    }
}
