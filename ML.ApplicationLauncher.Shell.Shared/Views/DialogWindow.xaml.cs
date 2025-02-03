// Copyright © Martin Lacina

using System.Windows;
using ML.ApplicationLauncher.Shell.ViewModels;

namespace ML.ApplicationLauncher.Shell.Views;

/// <summary>
/// Interaction logic for DialogWindow.xaml
/// </summary>
public partial class DialogWindow : Window
{
    public DialogWindow(IDialogViewModel dialogViewModel)
    {
        InitializeComponent();
        SetTitle(dialogViewModel.DialogTitle);
        DataContext = dialogViewModel;
    }

    private void SetTitle(string dialogTitle)
    {
        Title = $"{Title} - {dialogTitle}";
    }
}
