// Copyright © Martin Lacina

using ML.ApplicationLauncher.Shell.ViewModels;
using ML.ApplicationLauncher.Shell.Views;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shell.Services;

internal class MyDialogService : IMyDialogService
{
    public void ShowAboutDialog()
    {
        ShowDialog(new AboutViewModel());
    }

    public void ShowJsonDialog(ProcessGroup[] groups, string configurationFileName)
    {
        ShowDialog(new JsonDialogViewModel(groups, configurationFileName));
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
