// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Input;
using Prism.Commands;

namespace ML.ApplicationLauncher.Shell.Services;

public class RefreshableCommandFactory : ICommandFactory, IDisposable
{
    private readonly Timer _timer;
    private List<WeakReference<RefreshedCommandWrapper>> _commands = new();

    public RefreshableCommandFactory()
    {
        _timer = new Timer(250);
        _timer.Elapsed += RefreshCanExecute;
    }

    public ICommand CreateCommand(Action executeMethod, Func<bool> canExecuteMethod)
    {
        var command = new RefreshedCommandWrapper(executeMethod, canExecuteMethod);

        lock (_commands)
        {
            _commands.Add(new WeakReference<RefreshedCommandWrapper>(command));
            UpdateTimer();
        }

        return command.Command;
    }

    private void RefreshCanExecute(object? sender, ElapsedEventArgs e)
    {
        lock (_commands)
        {
            var aliveCommands = new List<WeakReference<RefreshedCommandWrapper>>();
            foreach (var command in _commands)
            {
                if (!command.TryGetTarget(out var aliveCommand))
                {
                    continue;
                }

                aliveCommands.Add(command);
                aliveCommand.CheckCanExecuteChanged();
            }
            _commands = aliveCommands;
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        lock (_commands)
        {
            switch (_commands.Count)
            {
                case 1:
                    _timer.Start();
                    break;
                case 0:
                    _timer.Stop();
                    break;
            }
        }
    }

    public void Dispose() => _timer.Dispose();

    private class RefreshedCommandWrapper
    {
        private readonly Func<bool> _canExecuteMethod;
        private readonly DelegateCommand _command;
        private bool _lastCanExecute;

        public RefreshedCommandWrapper(Action executeMethod, Func<bool> canExecuteMethod)
        {
            _canExecuteMethod = canExecuteMethod;
            _command = new DelegateCommand(executeMethod, GetLastCanExecute);
            CheckCanExecuteChanged();
        }

        private bool GetLastCanExecute()
        {
            return _lastCanExecute;
        }

        public ICommand Command => _command;

        public void CheckCanExecuteChanged()
        {
            bool canExecute = _canExecuteMethod();
            if (canExecute == _lastCanExecute)
            {
                return;
            }

            _lastCanExecute = canExecute;
            _command.RaiseCanExecuteChanged();
        }
    }
}
