// Copyright © Martin Lacina

namespace ML.ApplicationLauncher.Source.Configuration;

public record WindowsTerminalConfig(
    string WindowId = "ML.ApplicationLauncher",
    bool Enable = true
    )
{
    public static WindowsTerminalConfig Default => new WindowsTerminalConfig();
}
