// Copyright © Martin Lacina

using Prism.Ioc;

namespace ML.ApplicationLauncher.Core.Dependencies;

public static class ContainerRegistryExtensions
{
    public static void Install<T>(this IContainerRegistry registry) where T : IDependencyInstaller, new()
    {
        new T().Install(registry);
    }
}
