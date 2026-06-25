// Copyright © Martin Lacina

using System;
using System.Linq;

namespace ML.ApplicationLauncher.Core
{
    /// <summary>
    /// Shared helpers for argument serialization/deserialization.
    /// Single source of truth: arguments are stored as newline-separated strings,
    /// each line representing one argument to preserve spaces within each argument.
    /// </summary>
    public static class ArgumentExtensions
    {
        /// <summary>
        /// Splits a newline-separated arguments string into individual argument tokens.
        /// </summary>
        public static string[] ParseArguments(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return Array.Empty<string>();

            return args.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        /// <summary>
        /// Joins individual argument tokens into a newline-separated string.
        /// </summary>
        public static string FormatArguments(string[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                return string.Empty;

            return string.Join("\n", arguments);
        }
    }
}
