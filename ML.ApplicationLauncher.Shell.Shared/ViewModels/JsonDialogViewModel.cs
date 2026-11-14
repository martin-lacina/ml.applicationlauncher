// Copyright © Martin Lacina

using System;
using System.Collections.ObjectModel;
using System.Linq;
using ML.ApplicationLauncher.Source.Extensions;
using ML.ApplicationLauncher.Source.Model;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public class JsonDialogViewModel : BindableBase, IDialogViewModel
{
    public JsonDialogViewModel(ProcessGroup[] groups, string configurationFileName)
    {
        ConfigurationFileName = configurationFileName;
        Items = new ObservableCollection<TreeItemViewModel>();
        foreach (var group in groups)
        {
            Items.Add(TreeItemViewModel.FromGroup(group));
        }
    }

    public string DialogTitle => $"Command definitions - {ConfigurationFileName}";

    public ObservableCollection<TreeItemViewModel> Items { get; }

    private string ConfigurationFileName { get; }
}

public class TreeItemViewModel
{
    private readonly ProcessGroup _group;

    private TreeItemViewModel(ProcessGroup group)
    {
        _group = group;
        Header = group.DisplayName;
    }

    private TreeItemViewModel(ProcessLaunchInformation process)
    {
        Header = process.DisplayName;
        Details = BuildProcessDetailLines(process);
    }

    public static TreeItemViewModel FromGroup(ProcessGroup group) => new(group);

    public static TreeItemViewModel FromProcess(ProcessLaunchInformation process) => new(process);

    public string Header { get; }

    public string[]? Details { get; }

    public TreeItemViewModel[] Children
    {
        get
        {
            if (_group is null)
            {
                return Array.Empty<TreeItemViewModel>();
            }

            var result = new System.Collections.Generic.List<TreeItemViewModel>();
            foreach (var childGroup in _group.Groups.Where(g => g.IsVisible()))
            {
                result.Add(FromGroup(childGroup));
            }

            foreach (var process in _group.Processes.Where(p => p.IsVisible()))
            {
                result.Add(FromProcess(process));
            }

            return result.ToArray();
        }
    }

    private static string[] BuildProcessDetailLines(ProcessLaunchInformation process)
    {
        var lines = new System.Collections.Generic.List<string>();
        if (string.IsNullOrWhiteSpace(process.Comment))
        {
            lines.Add(process.Executable);
        }
        else
        {
            lines.Add($"{process.Executable} — {process.Comment}");
        }

        if (process.Arguments is { Length: > 0 })
        {
            lines.Add($"Arguments: {string.Join(" ", process.Arguments)}");
        }

        lines.Add($"Execution mode: {process.ExecutionMode}");

        if (string.IsNullOrWhiteSpace(process.WorkingDirectory))
        {
            return lines.ToArray();
        }

        lines.Add($"Working directory: {process.WorkingDirectory}");
        return lines.ToArray();
    }
}
