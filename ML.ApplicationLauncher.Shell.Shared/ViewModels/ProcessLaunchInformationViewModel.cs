using ML.ApplicationLauncher.Source.Model;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public partial class ProcessLaunchInformationViewModel : BindableBase
{
    public string DisplayName { get; set; }
    // Add other properties as needed

    public ProcessLaunchInformationViewModel(ProcessLaunchInformation info)
    {
        DisplayName = info.DisplayName;
        // Map other properties
    }
}
