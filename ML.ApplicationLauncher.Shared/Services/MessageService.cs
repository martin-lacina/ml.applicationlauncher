// Copyright © Martin Lacina

using System;
using System.Windows;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.Services;

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

    internal static string? TrimMessageLength(string? message)
    {
        if (message == null || message.Length <= 1000)
            return message;

        return message[..1000];
    }
}
