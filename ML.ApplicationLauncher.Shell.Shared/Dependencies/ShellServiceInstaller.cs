// Copyright © Martin Lacina

using ML.ApplicationLauncher.Core.Dependencies;
using ML.ApplicationLauncher.Shell.Services;
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
            .Register<IConfigurationProvider, ConfigurationProvider>()            
            ;
    }
}
