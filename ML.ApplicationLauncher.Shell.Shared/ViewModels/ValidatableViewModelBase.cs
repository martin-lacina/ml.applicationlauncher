using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;

namespace ML.ApplicationLauncher.Shell.Shared.ViewModels;

public abstract class ValidatableViewModelBase : BindableBase, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    public bool HasErrors => _errors.Any();

    public IEnumerable GetErrors(string? propertyName)
        => _errors.TryGetValue(propertyName ?? string.Empty, out var errorList) ? errorList : Enumerable.Empty<string>();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected void ValidateProperty(string propertyName, object? value)
    {
        ClearErrors(propertyName);
        if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
        {
            AddError(propertyName, $"{propertyName} cannot be empty.");
        }
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();
        _errors[propertyName].Add(error);
        OnErrorsChanged(propertyName);
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    protected void OnErrorsChanged(string propertyName)
        => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
}
