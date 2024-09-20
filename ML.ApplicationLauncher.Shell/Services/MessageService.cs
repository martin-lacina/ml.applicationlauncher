// Copyright © Martin Lacina

using System;
using System.Windows;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shell.Services;

internal class MessageService : IMessageService
{
    public void ShowError(string message)
    {
        MessageBox.Show(TrimMessageLength(message), "Error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowError(string message, Exception ex)
    {
        string formattedMessage = $"{message}\n\n{ex.Message}\n\n{ex.StackTrace}".Trim();
        ShowError(formattedMessage);
    }

    private static string? TrimMessageLength(string? message)
    {
        return message?.Substring(0, Math.Min(1000, message.Length));
    }
}
