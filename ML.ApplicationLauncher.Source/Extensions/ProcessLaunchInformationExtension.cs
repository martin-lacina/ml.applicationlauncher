// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Extensions;

public static class ProcessLaunchInformationExtension
{
    public static bool IsVisible(this ProcessLaunchInformation process) => !process.Hidden;

    public static string GetArgumentsForProcessLaunch(this ProcessLaunchInformation process)
    {
        return process.Arguments.GetArgumentsForProcessLaunch();
    }

    public static string GetArgumentsForProcessLaunch(this IEnumerable<string> arguments)
    {
        return string.Join(" ", arguments?.Select(a => $"\"{a}\"") ?? Array.Empty<string>());
    }
}
