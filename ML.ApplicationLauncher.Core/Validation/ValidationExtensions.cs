// Copyright © Martin Lacina

using System;
using System.Runtime.CompilerServices;

namespace ML.ApplicationLauncher.Core.Validation;

public static class ValidationExtensions
{
    public static T ShouldNotBeNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);

        return value;
    }

    public static T ShouldNotBeNull<T>(this T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
        where T : struct
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);

        return value.Value;
    }

    public static T[] ShouldNotBeNullOrEmpty<T>(this T[] value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        value.ShouldNotBeNull(parameterName);

        if (value.LongLength == 0)
            throw new ArgumentException("Array cannot be empty", parameterName);

        return value;
    }
}
