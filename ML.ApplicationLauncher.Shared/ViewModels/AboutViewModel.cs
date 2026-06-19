// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using ML.ApplicationLauncher.Shell.Assets;

namespace ML.ApplicationLauncher.Shared.ViewModels;

/// <summary>
/// ViewModel for the About dialog.
/// </summary>
internal partial class AboutViewModel : ObservableObject, IDialogViewModel
{
    public AboutViewModel()
    {
        License = EmbeddedResources.License;
    }

    public string DialogTitle { get; } = "About application";

    public string VersionInformation { get; } = BuildVersionInformation();

    public string License { get; }

    private static string BuildVersionInformation()
        => $"{ThisAssembly.AssemblyInformationalVersion}, {ThisAssembly.GitCommitDate:s}, {ThisAssembly.GitCommitId}";
}
