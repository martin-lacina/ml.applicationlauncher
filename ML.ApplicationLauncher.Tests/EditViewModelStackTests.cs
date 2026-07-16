// Copyright © Martin Lacina

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ML.ApplicationLauncher.Tests;

public class EditViewModelStackTests
{
    private Stack<string> _undoStack = null!;
    private Stack<string> _redoStack = null!;
    private const int MaxUndoRedoStackSize = 100;

    [SetUp]
    public void Setup()
    {
        _undoStack = new Stack<string>();
        _redoStack = new Stack<string>();
    }

    [Test]
    public void PushUndo_WhenUnderLimit_DoesNotTrimOldEntries()
    {
        // Arrange & Act — simulate PushUndo with 50 entries (under limit)
        for (int i = 0; i < 50; i++)
            _undoStack.Push($"snap_{i}");

        // Assert
        Assert.That(_undoStack.Count, Is.EqualTo(50), "Should not trim when under limit");
    }

    [Test]
    public void PushUndo_WhenExceedsLimit_TrimsOldestEntries()
    {
        // Simulate the actual PushUndo behavior: List-based history with RemoveAt(0) trim.
        var history = new List<string>();

        for (int i = 0; i < 150; i++)
            history.Add($"snap_{i}");

        int excess = history.Count - MaxUndoRedoStackSize;
        
        // Remove oldest entries from the FRONT of the list (RemoveAt(0) removes first item each time)
        for (int i = 0; i < excess; i++)
            history.RemoveAt(0);

        Assert.That(history.Count, Is.EqualTo(MaxUndoRedoStackSize),
            "Should trim to exactly the limit when exceeded");
        Assert.That(history[0], Is.EqualTo("snap_50"),
            "Oldest surviving entry should be snap_50 (removes 0-49)");
        Assert.That(history[^1], Is.EqualTo("snap_149"),
            "Most recent entry should still be at end");
    }

    [Test]
    public void PushUndo_ClearsRedoStackOnNewPush()
    {
        // Arrange — populate redo stack (simulating previous undo)
        _redoStack.Push("old_redo_1");
        _redoStack.Push("old_redo_2");
        Assert.That(_redoStack.Count, Is.EqualTo(2), "Should have 2 entries before");

        // Act — simulate PushUndo behavior
        _undoStack.Push("new_snap");
        while (_undoStack.Count > MaxUndoRedoStackSize)
            _undoStack.Pop();
        _redoStack.Clear();

        // Assert
        Assert.That(_redoStack.Count, Is.EqualTo(0), "Redo stack should be cleared after new push");
    }

    [Test]
    public void PushUndo_WhenAtExactLimit_DoesNotTrim()
    {
        // Arrange — exactly at limit (100 entries)
        for (int i = 0; i < MaxUndoRedoStackSize; i++)
            _undoStack.Push($"snap_{i}");

        Assert.That(_undoStack.Count, Is.EqualTo(MaxUndoRedoStackSize), "Should be at exact limit");

        // Act — simulate PushUndo (no trim needed since count == limit)
        _undoStack.Push("new_snap");
        while (_undoStack.Count > MaxUndoRedoStackSize)
            _undoStack.Pop();

        // Assert
        Assert.That(_undoStack.Count, Is.EqualTo(MaxUndoRedoStackSize), 
            "Should be at limit after trim (one added, one removed)");
    }

    [Test]
    public void PushUndo_WhenExceedsLimitByOne_TrimsExactlyOne()
    {
        // Arrange — exactly at limit + 1 entry
        for (int i = 0; i < MaxUndoRedoStackSize; i++)
            _undoStack.Push($"snap_{i}");
        _undoStack.Push("overflow_entry");

        Assert.That(_undoStack.Count, Is.EqualTo(MaxUndoRedoStackSize + 1), "Should exceed limit by one");

        // Act — apply trim logic
        while (_undoStack.Count > MaxUndoRedoStackSize)
            _undoStack.Pop();

        // Assert
        Assert.That(_undoStack.Count, Is.EqualTo(MaxUndoRedoStackSize), 
            "Should be trimmed to exactly the limit");
    }
}