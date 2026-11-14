// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Shell.Services;

public interface IMyDialogService
{
    void ShowAboutDialog();
    void ShowJsonDialog(ProcessGroup[] groups, string configurationFileName);
}
