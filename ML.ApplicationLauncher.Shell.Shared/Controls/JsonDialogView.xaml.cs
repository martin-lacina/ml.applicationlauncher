// Copyright © Martin Lacina

using System.Windows;
using System.Windows.Controls;

namespace ML.ApplicationLauncher.Shell.Controls;

public partial class JsonDialogView : ContentControl
{
    public JsonDialogView()
    {
        InitializeComponent();
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
