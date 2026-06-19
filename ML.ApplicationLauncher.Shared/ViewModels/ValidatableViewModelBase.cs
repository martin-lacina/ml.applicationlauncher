using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ML.ApplicationLauncher.Shared.ViewModels
{
    /// <summary>
    /// Base class that combines CommunityToolkit.Mvvm property notification with manual <see cref="INotifyDataErrorInfo"/> support.
    /// </summary>
    public abstract class ValidatableViewModelBase : ObservableObject, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

        public bool HasErrors => _errors.Any();

        public IEnumerable GetErrors(string? propertyName)
            => _errors.TryGetValue(propertyName ?? string.Empty, out var errorList) ? errorList : Enumerable.Empty<string>();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Adds an error or clears all errors for the specified property and raises <see cref="ErrorsChanged"/>.
        /// </summary>
        protected void SetValidation(string propertyName, string? error)
        {
            if (error is not null)
            {
                if (!_errors.ContainsKey(propertyName))
                    _errors[propertyName] = new List<string>();
                _errors[propertyName].Add(error);
            }
            else
            {
                _errors.Remove(propertyName);
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
