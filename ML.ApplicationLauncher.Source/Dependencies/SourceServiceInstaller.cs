// Copyright © Martin Lacina

using System.Threading;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Core.Dependencies;
using ML.ApplicationLauncher.Source.Configuration;
using ML.ApplicationLauncher.Source.ExecutorResolvers;
using ML.ApplicationLauncher.Source.Model;
using ML.ApplicationLauncher.Source.Services;
using Prism.Ioc;

namespace ML.ApplicationLauncher.Source.Dependencies;

public class SourceServiceInstaller : IDependencyInstaller
{
    public void Install(IContainerRegistry registry)
    {
        registry
            .Register<IProcessListProvider, ProcessListProvider>()
            .Register<IProcessLauncher, ProcessLauncher>()
            .Register<IProcessStartInfoResolverSelector, ProcessStartInfoResolverSelector>()
            .RegisterSingleton<ProcessToLaunchConfigurationManager>()
            .Register<IConfigurationProvider<ProcessGroup[]>, ProcessToLaunchConfigurationManager>()
            .Register<IConfigurationManager<ProcessGroup[]>, ProcessToLaunchConfigurationManager>()
            .RegisterSingleton<ApplicationConfigurationManager>()
            .Register<IConfigurationProvider<ApplicationConfiguration>, ApplicationConfigurationManager>()
            .Register<IConfigurationManager<ApplicationConfiguration>, ApplicationConfigurationManager>()
            .Register<WindowsTerminalConfig>(cp =>
                {
                    var provider = cp.Resolve<IConfigurationProvider<ApplicationConfiguration>>();

                    // Ensure we do not block UI thread by execution
                    var config = Task.Run(async () => await provider.LoadConfigurationAsync(CancellationToken.None)).GetAwaiter().GetResult();

                    return config.WindowsTerminal;
                })
            ;
    }
}
