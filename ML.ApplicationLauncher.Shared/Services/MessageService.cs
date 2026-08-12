// Copyright © Martin Lacina

using System;
using System.Windows;
using ML.ApplicationLauncher.Source.Services;

namespace ML.ApplicationLauncher.Shared.Services;

internal class MessageService : IMessageService
{
    private const int MaxErrorMessageLength = 1000;

    public void ShowError(string message)
    {
        MessageBox.Show(TrimMessageLength(message), "Error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowError(string message, Exception ex)
    {
        string formattedMessage = $"{message}\n\n{ex.Message}\n\n{ex.StackTrace}".Trim();
        ShowError(formattedMessage);
    }

    public MessageBoxResult ShowQuestion(string message, string title, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Question)
    {
        return MessageBox.Show(TrimMessageLength(message), title, button, image);
    }

    internal static string? TrimMessageLength(string? message)
    {
        if (message == null || message.Length <= MaxErrorMessageLength)
            return message;

        return message[..MaxErrorMessageLength];
    }
}
