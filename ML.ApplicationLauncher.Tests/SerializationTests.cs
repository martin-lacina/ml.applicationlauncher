// Copyright © Martin Lacina

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ML.ApplicationLauncher.Source.Model;
using NUnit.Framework;

namespace ML.ApplicationLauncher.Tests;

public class SerializationTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
    };

    #region ProcessGroup Tests

    [Test]
    public void ProcessGroup_Serialize_Deserialize_RoundTripsCorrectly()
    {
        // Arrange
        var original = new ProcessGroup
        {
            DisplayName = "Test Group",
            Comment = "A test group",
            CanLaunch = true,
            Disabled = false,
            Hidden = false,
            Groups = new[]
            {
                new ProcessGroup
                {
                    DisplayName = "Nested Group",
                    Comment = "",
                    CanLaunch = false,
                    Groups = Array.Empty<ProcessGroup>(),
                    Processes = Array.Empty<ProcessLaunchInformation>()
                }
            },
            Processes = new[]
            {
                new ProcessLaunchInformation
                {
                    DisplayName = "Notepad",
                    Comment = "Edit text files",
                    Executable = "notepad.exe",
                    Arguments = new[] { "test.txt" },
                    ExecutionMode = ExecutionMode.Default,
                    Disabled = false,
                    Hidden = false,
                    WorkingDirectory = null
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ProcessGroup>(json, _jsonOptions);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.DisplayName, deserialized.DisplayName);
        Assert.AreEqual(original.Comment, deserialized.Comment);
        Assert.AreEqual(original.CanLaunch, deserialized.CanLaunch);
        Assert.AreEqual(original.Disabled, deserialized.Disabled);
        Assert.AreEqual(original.Hidden, deserialized.Hidden);

        var childGroups = deserialized.Groups.ToArray();
        Assert.AreEqual(1, childGroups.Length);
        Assert.AreEqual("Nested Group", childGroups[0].DisplayName);

        var processes = deserialized.Processes.ToArray();
        Assert.AreEqual(1, processes.Length);
        Assert.AreEqual("Notepad", processes[0].DisplayName);
        Assert.AreEqual("notepad.exe", processes[0].Executable);
    }

    [Test]
    public void ProcessGroup_Serialize_UsesExpectedPropertyNames()
    {
        // Arrange
        var group = new ProcessGroup
        {
            DisplayName = "Tools",
            Comment = "",
            CanLaunch = true,
            Groups = Array.Empty<ProcessGroup>(),
            Processes = Array.Empty<ProcessLaunchInformation>()
        };

        // Act
        var json = JsonSerializer.Serialize(group, _jsonOptions);

        // Assert — verify JSON uses the exact property names from config files
        StringAssert.Contains("\"DisplayName\"", json);
        StringAssert.Contains("\"Comment\"", json);
        StringAssert.Contains("\"CanLaunch\"", json);
        StringAssert.Contains("\"Groups\"", json);
        StringAssert.Contains("\"Processes\"", json);
    }

    #endregion

    #region ProcessLaunchInformation Tests

    [Test]
    public void ProcessLaunchInformation_Serialize_Deserialize_RoundTripsCorrectly()
    {
        // Arrange
        var original = new ProcessLaunchInformation
        {
            DisplayName = "PowerShell",
            Comment = "Run as PowerShell script",
            Executable = "Scripts\\run.ps1",
            Arguments = new[] { "Write-Host 'hello'", "hostname" },
            ExecutionMode = ExecutionMode.PowerShellScript,
            Disabled = true,
            Hidden = false,
            WorkingDirectory = "C:\\Scripts"
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<ProcessLaunchInformation>(json, _jsonOptions);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.DisplayName, deserialized.DisplayName);
        Assert.AreEqual(original.Comment, deserialized.Comment);
        Assert.AreEqual(original.Executable, deserialized.Executable);
        CollectionAssert.AreEquivalent(original.Arguments, deserialized.Arguments?.ToArray());
        Assert.AreEqual(original.ExecutionMode, deserialized.ExecutionMode);
        Assert.AreEqual(original.Disabled, deserialized.Disabled);
        Assert.AreEqual(original.Hidden, deserialized.Hidden);
        Assert.AreEqual(original.WorkingDirectory, deserialized.WorkingDirectory);
    }

    [Test]
    public void ProcessLaunchInformation_Serialize_UsesExpectedPropertyNames()
    {
        // Arrange
        var info = new ProcessLaunchInformation
        {
            DisplayName = "Calc",
            Comment = "",
            Executable = "calc.exe",
            Arguments = Array.Empty<string>(),
            ExecutionMode = ExecutionMode.Standalone,
            WorkingDirectory = null
        };

        // Act
        var json = JsonSerializer.Serialize(info, _jsonOptions);

        // Assert — verify JSON uses the exact property names from config files
        StringAssert.Contains("\"DisplayName\"", json);
        StringAssert.Contains("\"Executable\"", json);
        StringAssert.Contains("\"Arguments\"", json);
        StringAssert.Contains("\"ExecutionMode\"", json);
        StringAssert.Contains("\"WorkingDirectory\"", json);
    }

    [Test]
    public void ProcessLaunchInformation_Deserialize_MissingOptionalFields_UsesDefaults()
    {
        // Arrange — minimal JSON matching what old configs might have
        var json = @"{
            ""DisplayName"": ""Minimal"",
            ""Executable"": ""app.exe"",
            ""Arguments"": []
        }";

        // Act
        var deserialized = JsonSerializer.Deserialize<ProcessLaunchInformation>(json, _jsonOptions);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("Minimal", deserialized.DisplayName);
        Assert.AreEqual("app.exe", deserialized.Executable);
        Assert.AreEqual(string.Empty, deserialized.Comment);
        Assert.AreEqual(ExecutionMode.Default, deserialized.ExecutionMode);
        Assert.IsFalse(deserialized.Disabled);
        Assert.IsFalse(deserialized.Hidden);
        Assert.IsNull(deserialized.WorkingDirectory);
    }

    #endregion
}
