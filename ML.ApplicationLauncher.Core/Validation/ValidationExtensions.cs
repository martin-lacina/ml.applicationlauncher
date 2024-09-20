// Copyright © Martin Lacina

using System;
using System.Runtime.CompilerServices;

namespace ML.ApplicationLauncher.Core.Validation;

public static class ValidationExtensions
{
    public static T ShouldNotBeNull<T>(this T? value, [CallerArgumentExpression("value")] string? parameterName = null) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);

        return value;
    }
}
