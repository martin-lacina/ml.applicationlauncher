// Copyright © Martin Lacina

namespace ML.ApplicationLauncher.Shell.Admin;

public static class Launcher
{
    [System.STAThreadAttribute()]
    public static void Main(string[] args) => ML.ApplicationLauncher.Shell.App.Main();
}
