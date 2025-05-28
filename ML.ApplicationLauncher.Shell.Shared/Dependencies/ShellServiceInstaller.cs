// Copyright © Martin Lacina

using ML.ApplicationLauncher.Core.Dependencies;
using ML.ApplicationLauncher.Shell.Services;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;
using Prism.Ioc;

namespace ML.ApplicationLauncher.Shell.Dependencies;

public class ShellServiceInstaller : IDependencyInstaller
{
    public void Install(IContainerRegistry registry)
    {
        registry
            .RegisterSingleton<IMessageService, MessageService>()
            .RegisterSingleton<ICommandFactory, RefreshableCommandFactory>()
            .RegisterSingleton<IMyDialogService, MyDialogService>()
            .Register<IConfigurationLocationProvider<ProcessGroup[]>, ProcessToLaunchConfigurationProvider>()
            .Register<IConfigurationLocationProvider<ApplicationConfiguration>, ApplicationConfigurationProvider>()
            ;
    }
}
