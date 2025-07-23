using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ML.ApplicationLauncher.Source.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public partial class ProcessGroupEditorDialogViewModel : BindableBase
{
    public ObservableCollection<EditProcessGroupViewModel> RootGroups { get; }
    private object? selectedItem;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddGroupCommand { get; }
    public ICommand AddProcessCommand { get; }
    public ICommand DeleteGroupCommand { get; }
    public ICommand DeleteProcessCommand { get; }

    public ProcessGroupEditorDialogViewModel(IEnumerable<ProcessGroup> groups)
    {
        RootGroups = new ObservableCollection<EditProcessGroupViewModel>(groups.Select(g => new EditProcessGroupViewModel(g)));
        SaveCommand = new DelegateCommand(OnSave);
        CancelCommand = new DelegateCommand(OnCancel);
        AddGroupCommand = new DelegateCommand<EditProcessGroupViewModel?>(OnAddGroup);
        AddProcessCommand = new DelegateCommand<EditProcessGroupViewModel?>(OnAddProcess);
        DeleteGroupCommand = new DelegateCommand<EditProcessGroupViewModel?>(OnDeleteGroup);
        DeleteProcessCommand = new DelegateCommand<ProcessLaunchInformationViewModel?>(OnDeleteProcess);
    }

    private void OnSave()
    {
        // Here you would collect the current state and save it
        System.Windows.MessageBox.Show("Save is not implemented yet.", "Not Implemented", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void OnCancel()
    {
        // Close the dialog via window
        if (System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w.DataContext == this) is System.Windows.Window window)
        {
            window.Close();
        }
    }

    private void OnAddGroup(EditProcessGroupViewModel? parent) { /* Add group logic */ }
    private void OnAddProcess(EditProcessGroupViewModel? parent) { /* Add process logic */ }
    private void OnDeleteGroup(EditProcessGroupViewModel? group) { /* Delete group logic */ }
    private void OnDeleteProcess(ProcessLaunchInformationViewModel? process) { /* Delete process logic */ }
}
