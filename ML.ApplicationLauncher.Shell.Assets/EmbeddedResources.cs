// Copyright © Martin Lacina

using System.Reflection;
using System.Text;

namespace ML.ApplicationLauncher.Shell.Assets;

public static class EmbeddedResources
{
    public static IEnumerable<string> ResourceNames
    {
        get
        {
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            return names;
        }
    }

    public static string License => ReadEmbeddedResource("LICENSE");

    private static string ReadEmbeddedResource(string resourceName)
    {
        var info = Assembly.GetExecutingAssembly().GetName();
        var name = info.Name;
        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"{name}.EmbeddedResources.{resourceName}")!;
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}
