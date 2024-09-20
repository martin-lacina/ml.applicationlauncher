// Copyright © Martin Lacina

using System;
using System.Threading;
using System.Windows.Input;
using ML.ApplicationLauncher.Shell.Services;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.ViewModels;

public abstract class ProcessViewModelBase : BindableBase
{
    private readonly Lazy<ICommand> _lazyStartCommand;

    private DateTime? _lastExecuted;

    private bool _canBeStarted = true;

    protected ProcessViewModelBase(ICommandFactory commandFactory)
    {
        _lazyStartCommand = new Lazy<ICommand>(() => commandFactory.CreateCommand(StartSafe, CanStartSafe),
            LazyThreadSafetyMode.PublicationOnly);
    }

    public abstract string DisplayName { get; }

    public ICommand StartCommand => _lazyStartCommand.Value;

    public TimeOnly? LastExecuted
    {
        get
        {
            var lastExecuted = _lastExecuted;
            if (lastExecuted != null)
                return TimeOnly.FromDateTime(lastExecuted.Value.ToLocalTime());
            return null;
        }
    }

    public bool CanBeStarted
    {
        get => _canBeStarted;
        set => SetProperty(ref _canBeStarted, value);
    }

    public void SetLastExecuted()
    {
        _lastExecuted = DateTime.UtcNow;
        RaisePropertyChanged(nameof(LastExecuted));
    }

    public void ClearLastExecuted()
    {
        _lastExecuted = null;
        RaisePropertyChanged(nameof(LastExecuted));
    }

    public void ExpireLastExecuted(TimeSpan expirationTime)
    {
        var lastExecuted = _lastExecuted;
        if (!lastExecuted.HasValue || lastExecuted.Value.Add(expirationTime) > DateTime.UtcNow)
            return;

        ClearLastExecuted();
    }

    protected abstract void StartInternal();
    protected abstract bool CanStartInternal();

    private bool CanStartSafe()
    {
        try
        {
            return CanStartInternal();
        }
        catch
        {
            return false;
        }
    }

    private void StartSafe()
    {
        try
        {
            StartInternal();
        }
        catch
        {
            // Nothing to do
        }
    }

}
