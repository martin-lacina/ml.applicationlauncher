// Copyright © Martin Lacina

using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ML.ApplicationLauncher.Shared.ViewModels;

/// <summary>
/// Shared tracker for last-execution time, composed into ViewModels.
/// Provides time tracking with optional expiration via composition pattern.
/// </summary>
public class LastExecutedTracker : ObservableObject
{
    private DateTime? _lastExecuted;

    /// <summary>
    /// The last execution time in local time, or null if never executed.
    /// </summary>
    public TimeOnly? LastExecuted => _lastExecuted != null ? TimeOnly.FromDateTime(_lastExecuted.Value.ToLocalTime()) : null;

    /// <summary>
    /// Records the current UTC time as the last execution time.
    /// </summary>
    public void SetLastExecuted()
    {
        _lastExecuted = DateTime.UtcNow;
        OnPropertyChanged(nameof(LastExecuted));
    }

    /// <summary>
    /// Clears the last execution time.
    /// </summary>
    public void ClearLastExecuted()
    {
        if (_lastExecuted != null)
        {
            _lastExecuted = null;
            OnPropertyChanged(nameof(LastExecuted));
        }
    }

    /// <summary>
    /// Expires the last execution time after the specified delay has elapsed.
    /// Returns true if the time was expired (was set and now past the threshold).
    /// </summary>
    public bool ExpireLastExecuted(TimeSpan delay)
    {
        if (_lastExecuted == null)
            return false;

        var elapsed = DateTime.UtcNow - _lastExecuted.Value;
        if (elapsed >= delay)
        {
            ClearLastExecuted();
            return true;
        }

        return false;
    }
}
