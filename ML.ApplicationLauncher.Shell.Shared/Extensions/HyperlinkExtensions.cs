// Copyright © Martin Lacina

using System.Diagnostics;
using System.Windows.Documents;
using System.Windows;
using ML.ApplicationLauncher.Core.Validation;

namespace ML.ApplicationLauncher.Shell.Shared.Extensions;

/// <summary>
/// From https://stackoverflow.com/questions/10238694/example-using-hyperlink-in-wpf
/// </summary>
public static class HyperlinkExtensions
{
    public static bool GetIsExternal(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsExternalProperty);
    }

    public static void SetIsExternal(DependencyObject obj, bool value)
    {
        obj.SetValue(IsExternalProperty, value);
    }
    public static readonly DependencyProperty IsExternalProperty =
        DependencyProperty.RegisterAttached("IsExternal", typeof(bool), typeof(HyperlinkExtensions), new UIPropertyMetadata(false, OnIsExternalChanged));

    private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        var hyperlink = (sender as Hyperlink).ShouldNotBeNull();

        if (args.NewValue is true)
            hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
        else
            hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
    }

    private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) {
            UseShellExecute = true
        });
        e.Handled = true;
    }
}
