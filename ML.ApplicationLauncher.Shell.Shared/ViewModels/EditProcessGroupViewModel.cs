using System.Collections.ObjectModel;
using System.Linq;
using ML.ApplicationLauncher.Source.Model;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public partial class EditProcessGroupViewModel : BindableBase
{
    public ObservableCollection<EditProcessGroupViewModel> Groups { get; }
    public ObservableCollection<ProcessLaunchInformationViewModel> Processes { get; }
    public string DisplayName { get; set; }
    public string Comment { get; set; }
    public bool CanLaunch { get; set; }
    public bool Disabled { get; set; }
    public bool Hidden { get; set; }

    public EditProcessGroupViewModel(ProcessGroup group)
    {
        DisplayName = group.DisplayName;
        Comment = group.Comment;
        CanLaunch = group.CanLaunch;
        Disabled = group.Disabled;
        Hidden = group.Hidden;
        Groups = new ObservableCollection<EditProcessGroupViewModel>(group.Groups.Select(g => new EditProcessGroupViewModel(g)));
        Processes = new ObservableCollection<ProcessLaunchInformationViewModel>(group.Processes.Select(p => new ProcessLaunchInformationViewModel(p)));
    }
}
