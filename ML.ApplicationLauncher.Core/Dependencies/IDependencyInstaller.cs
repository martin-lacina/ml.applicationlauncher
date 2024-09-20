// Copyright © Martin Lacina

using Prism.Ioc;

namespace ML.ApplicationLauncher.Core.Dependencies;

public interface IDependencyInstaller
{
    public void Install(IContainerRegistry registry);
}
