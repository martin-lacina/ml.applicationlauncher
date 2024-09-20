// Copyright © Martin Lacina

using System;
using System.Windows;
using ML.ApplicationLauncher.Core.Dependencies;
using ML.ApplicationLauncher.Shell.Dependencies;
using ML.ApplicationLauncher.Shell.Views;
using ML.ApplicationLauncher.Source.Dependencies;
using ML.ApplicationLauncher.Source.Services;
using Prism.Ioc;
using Prism.Unity;

namespace ML.ApplicationLauncher.Shell;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Install<SourceServiceInstaller>();
        containerRegistry.Install<ShellServiceInstaller>();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }

    protected override Window CreateShell()
    {
        var w = Container.Resolve<MainWindow>();
        return w;
    }

    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var message = $"Unhandled error: {e.ExceptionObject}";
        Container.Resolve<IMessageService>().ShowError(message);
    }
}
