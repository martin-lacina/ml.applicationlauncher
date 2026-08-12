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

    /// <summary>
    /// Validates that a nullable value type has an assigned value. Throws <see cref="ArgumentNullException"/> if the value is null.
    /// Use this method for nullable structs like <c>int?</c>, <c>Guid?</c>, etc.
    /// </summary>
    /// <typeparam name="T">The underlying struct type.</typeparam>
    /// <param name="value">The nullable value to validate.</param>
    /// <param name="parameterName">The parameter name to use in the exception message (automatically populated).</param>
    /// <returns>The unwrapped value if not null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static T ShouldHaveValue<T>(this T? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
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
