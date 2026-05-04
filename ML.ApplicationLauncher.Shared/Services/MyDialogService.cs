// Copyright © Martin Lacina

using System.Windows;
using ML.ApplicationLauncher.Shared.ViewModels;
using ML.ApplicationLauncher.Shared.Views;
using DialogWindow = ML.ApplicationLauncher.Shared.Views.DialogWindow;

namespace ML.ApplicationLauncher.Shared.Services;

internal class MyDialogService : IMyDialogService
{
    public void ShowAboutDialog()
    {
        ShowDialog(new AboutViewModel());
    }

    private void ShowDialog(IDialogViewModel viewModel)
    {
        var window = new DialogWindow(viewModel)
        {
            Owner = Application.Current.MainWindow,
            Icon = Application.Current.MainWindow.Icon,
            ShowInTaskbar = false
        };

        window.ShowDialog();
    }
}
