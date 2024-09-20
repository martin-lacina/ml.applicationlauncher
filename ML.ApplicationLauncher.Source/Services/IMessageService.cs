// Copyright © Martin Lacina

using System;

namespace ML.ApplicationLauncher.Source.Services;

public interface IMessageService
{
    void ShowError(string message);
    void ShowError(string message, Exception ex);
}
