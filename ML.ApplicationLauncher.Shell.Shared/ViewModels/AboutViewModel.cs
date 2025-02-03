// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Text;
using ML.ApplicationLauncher.Shell.Assets;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

internal class AboutViewModel : BindableBase, IDialogViewModel
{
    public AboutViewModel()
    {
        License = EmbeddedResources.License;
    }

    public string DialogTitle { get; } = "About";

    public string License { get; }
}
