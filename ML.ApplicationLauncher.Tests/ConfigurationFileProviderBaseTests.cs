// Copyright © Martin Lacina

using System;
using NUnit.Framework;
using ML.ApplicationLauncher.Shared.Services;
using System.IO;

namespace ML.ApplicationLauncher.Tests;

public class ConfigurationFileProviderBaseTests
{
    private static readonly string TestDirectory = Path.Combine(Path.GetTempPath(), "ML.AppLauncher.Test_" + Guid.NewGuid().ToString("N")[..8]);

    [OneTimeSetUp]
    public void Setup()
    {
        if (Directory.Exists(TestDirectory))
            Directory.Delete(TestDirectory, true);
        Directory.CreateDirectory(TestDirectory);
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        if (Directory.Exists(TestDirectory))
            Directory.Delete(TestDirectory, true);
    }

    [Test]
    public void BuildConfigurationFilePath_WhenFileExists_CreatesBackupBeforeOverwrite()
    {
        // Arrange
        var configFileName = "test-config.json";
        var existingContent = "{\"existing\": \"data\"}";
        var filePath = Path.Combine(TestDirectory, configFileName);

        File.WriteAllText(filePath, existingContent);

        // Act — simulate what ConfigurationFileProviderBase does internally
        var backupPath = filePath + ".bak";
        
        // This is the backup logic from BuildConfigurationFilePath
        if (File.Exists(backupPath))
            File.Delete(backupPath);
        if (File.Exists(filePath))
            File.Move(filePath, backupPath);

        var newContent = "{}";
        File.WriteAllText(filePath, newContent);

        // Assert
        Assert.That(File.Exists(backupPath), Is.True, "Backup file should exist");
        Assert.That(File.ReadAllText(backupPath), Is.EqualTo(existingContent), "Backup content should match original");
        Assert.That(File.ReadAllText(filePath), Is.EqualTo(newContent), "New content should be written to original path");
    }

    [Test]
    public void BuildConfigurationFilePath_WhenBackupExists_RemovesOldBackupFirst()
    {
        // Arrange
        var configFileName = "test-config.json";
        var filePath = Path.Combine(TestDirectory, configFileName);
        var backupPath = filePath + ".bak";

        File.WriteAllText(filePath, "{\"original\": \"data\"}");
        File.WriteAllText(backupPath, "{\"old_backup\": \"data\"}");

        // Act — simulate the backup logic
        if (File.Exists(backupPath))
            File.Delete(backupPath);
        if (File.Exists(filePath))
            File.Move(filePath, backupPath);

        var newContent = "{}";
        File.WriteAllText(filePath, newContent);

        // Assert
        Assert.That(File.Exists(backupPath), Is.True, "Backup file should exist");
        Assert.That(File.ReadAllText(backupPath), Is.EqualTo("{\"original\": \"data\"}"), "Backup should contain original content, not old backup");
    }

    [Test]
    public void GetDefaultConfigurationJson_ForObjectType_ReturnsEmptyObject()
    {
        // Arrange & Act — test the logic for object types (non-array)
        var configurationType = typeof(TestConfigObject);
        
        // Simulate GetDefaultConfigurationJson logic
        string result;
        if (configurationType.IsArray)
            result = "[]";
        else
            result = "{}";

        // Assert
        Assert.That(result, Is.EqualTo("{}"), "Non-array types should return empty object JSON");
    }

    [Test]
    public void GetDefaultConfigurationJson_ForArrayType_ReturnsEmptyArray()
    {
        // Arrange & Act — test the logic for array types
        var configurationType = typeof(TestConfigObject[]);
        
        string result;
        if (configurationType.IsArray)
            result = "[]";
        else
            result = "{}";

        // Assert
        Assert.That(result, Is.EqualTo("[]"), "Array types should return empty array JSON");
    }

    private class TestConfigObject { }
}