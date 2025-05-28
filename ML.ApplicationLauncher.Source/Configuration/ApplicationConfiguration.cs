// Copyright © Martin Lacina

namespace ML.ApplicationLauncher.Source.Configuration;

public record ApplicationConfiguration(
    WindowsTerminalConfig WindowsTerminal
    )
{
    public static ApplicationConfiguration Default => new ApplicationConfiguration(WindowsTerminalConfig.Default);
}
