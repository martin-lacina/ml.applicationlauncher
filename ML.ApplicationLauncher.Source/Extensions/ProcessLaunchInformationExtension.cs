// Copyright © Martin Lacina

using ML.ApplicationLauncher.Source.Model;

namespace ML.ApplicationLauncher.Source.Extensions;

public static class ProcessLaunchInformationExtension
{
    public static bool IsVisible(this ProcessLaunchInformation process) => !process.Hidden;
}
