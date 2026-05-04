// Copyright © Martin Lacina

using System;
using System.Windows.Input;

namespace ML.ApplicationLauncher.Shared.Services;

public interface ICommandFactory
{
    ICommand CreateCommand(Action executeMethod, Func<bool> canExecuteMethod);
}
