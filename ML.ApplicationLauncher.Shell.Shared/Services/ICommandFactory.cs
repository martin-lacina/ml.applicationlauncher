// Copyright © Martin Lacina

using System;
using System.Windows.Input;

namespace ML.ApplicationLauncher.Shell.Services;

public interface ICommandFactory
{
    ICommand CreateCommand(Action executeMethod, Func<bool> canExecuteMethod);
}
