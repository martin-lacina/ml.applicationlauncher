// Copyright © Martin Lacina

using System;
using System.Windows;

namespace ML.ApplicationLauncher.Source.Services;

public interface IMessageService
{
    void ShowError(string message);
    void ShowError(string message, Exception ex);
    MessageBoxResult ShowQuestion(string message, string title, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Question);
}
