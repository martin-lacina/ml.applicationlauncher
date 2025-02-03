// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Text;
using ML.ApplicationLauncher.Shell.ViewModels;
using ML.ApplicationLauncher.Shell.Views;

namespace ML.ApplicationLauncher.Shell.Services;

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
            Owner = App.Current.MainWindow,
            Icon = App.Current.MainWindow.Icon,
            ShowInTaskbar = false
        };

        window.ShowDialog();
    }
}
