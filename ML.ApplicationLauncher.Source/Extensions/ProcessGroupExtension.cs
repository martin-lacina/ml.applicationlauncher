// Copyright © Martin Lacina

using System.Linq;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Extensions;

public static class ProcessGroupExtension
{
    public static bool IsVisible(this ProcessGroup group) => !group.Hidden && (group.Groups.Any(g => g.IsVisible()) || group.Processes.Any(p => p.IsVisible()));
}
