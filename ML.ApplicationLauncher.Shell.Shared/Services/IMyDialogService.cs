// Copyright © Martin Lacina

namespace ML.ApplicationLauncher.Shell.Services;

public interface IMyDialogService
{
    void ShowAboutDialog();
    void ShowProcessGroupEditorDialog(System.Collections.Generic.IEnumerable<ML.ApplicationLauncher.Source.Model.ProcessGroup> processGroups);
}
