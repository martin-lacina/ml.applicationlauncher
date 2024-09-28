// Copyright © Martin Lacina

using ML.ApplicationLauncher.Core.Dependencies;
using ML.ApplicationLauncher.Source.Services;
using Prism.Ioc;

namespace ML.ApplicationLauncher.Source.Dependencies;

public class ServiceInstaller : IDependencyInstaller
{
    public void Install(IContainerRegistry registry)
    {
        registry
            .Register<IProcessListProvider, ProcessListProvider>()
            .Register<IProcessLauncher, ProcessLauncher>()
            ;
    }
}
