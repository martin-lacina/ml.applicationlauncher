// Copyright © Martin Lacina

using System.Security.Principal;
using System.Windows;

namespace ML.ApplicationLauncher.Shell.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetTitle();
    }

    private void SetTitle()
    {
        var title = Title;

        if (IsAdministrator(out var userName))
        {
            title = $"{title} (Administrator: userName)";
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
