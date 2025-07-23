// Copyright © Martin Lacina

using ML.ApplicationLauncher.Shell.ViewModels;
using ML.ApplicationLauncher.Shell.Views;

namespace ML.ApplicationLauncher.Shell.Services;

internal class MyDialogService : IMyDialogService
{
    public void ShowAboutDialog()
    {
        ShowDialog(new AboutViewModel());
    }

    public void ShowProcessGroupEditorDialog(System.Collections.Generic.IEnumerable<ML.ApplicationLauncher.Source.Model.ProcessGroup> processGroups)
    {
        var viewModel = new ViewModels.ProcessGroupEditorDialogViewModel(processGroups);
        var dialog = new Views.ProcessGroupEditorDialog
        {
            DataContext = viewModel,
            Owner = App.Current.MainWindow,
            Icon = App.Current.MainWindow.Icon,
            ShowInTaskbar = false
        };
        dialog.ShowDialog();
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
