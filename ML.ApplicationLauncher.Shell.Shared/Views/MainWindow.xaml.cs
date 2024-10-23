// Copyright © Martin Lacina

using System.Security.Principal;
using System.Windows;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(IConfigurationProvider configurationProvider)
    {
        InitializeComponent();
        SetTitle(configurationProvider.ConfigurationFileName);
    }

    private void SetTitle(string configurationFileName)
    {
        string title;
        if (IsAdministrator(out var userName))
        {
            title = $"{Title} ({configurationFileName}, Administrator: userName)";
        }
        else
        {
            title = $"{Title} ({configurationFileName})";
        }
        Title = title;
    }

    private static bool IsAdministrator(out string userName)
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        userName = identity.Name;
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
