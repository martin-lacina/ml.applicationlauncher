// Copyright © Martin Lacina

using System.Text;

namespace ML.ApplicationLauncher.Source.ExecutorResolvers.PowerShell;

internal record PowerShellScriptExecutionContext(string Title, string WorkingDirectory, string[] Commands)
{
    public string EncodeToBase64()
    {
        var serialized = System.Text.Json.JsonSerializer.Serialize(this);
        var base64encoded = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));
        return base64encoded;
    }
}
