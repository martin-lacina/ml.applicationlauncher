// Copyright © Martin lacina

using System;
using System.Collections.Generic;
using System.Text.Json;
using ML.ApplicationLauncher.Shared.Services;
using ML.ApplicationLauncher.Shared.ViewModels;
using NUnit.Framework;

namespace ML.ApplicationLauncher.Tests;

/// <summary>
/// Tests for the core UndoRedoManager orchestration logic.
/// Exercises real production code via a generic manager, not simulated behavior in test land.
/// </summary>
public class EditViewModelStackTests
{
    private UndoRedoManager<string> _manager = null!;

    [SetUp]
    public void Setup()
    {
        _manager = new UndoRedoManager<string>(10); // small limit for trim tests
    }

    #region Push behavior

    [Test]
    public void Push_WhenUnderLimit_DoesNotTrimOldEntries()
    {
        // Act — push 5 entries (under limit of 10)
        _manager.Push("a");
        _manager.Push("b");
        _manager.Push("c");
        _manager.Push("d");
        _manager.Push("e");

        // Assert
        Assert.That(_manager.HistoryCount, Is.EqualTo(5), "Should not trim when under limit");
    }

    [Test]
    public void Push_WhenExceedsLimit_TrimsOldestEntries()
    {
        // Act — push 15 entries (exceeds limit of 10)
        for (int i = 0; i < 15; i++)
            _manager.Push($"snap_{i}");

        // Assert
        Assert.That(_manager.HistoryCount, Is.EqualTo(10), "Should trim to exactly the limit when exceeded");
        Assert.That(_manager.Undo(), Is.EqualTo("snap_5"), "Oldest surviving entry should be snap_5 (removes 0-4)");
    }

    [Test]
    public void Push_WhenAtExactLimit_DoesNotTrim()
    {
        // Act — push exactly 10 entries (at limit), then one more triggers trim
        for (int i = 0; i < 10; i++)
            _manager.Push($"snap_{i}");

        Assert.That(_manager.HistoryCount, Is.EqualTo(10), "Should be at exact limit");

        // Push one more — should trigger trim but history stays at limit
        _manager.Push("overflow_entry");

        Assert.That(_manager.HistoryCount, Is.EqualTo(10),
            "Should be at limit after trim (one added, one removed)");
    }

    [Test]
    public void Push_WhenExceedsLimitByOne_TrimsExactlyOne()
    {
        // Act — push 11 entries (limit + 1)
        for (int i = 0; i < 11; i++)
            _manager.Push($"snap_{i}");

        Assert.That(_manager.HistoryCount, Is.EqualTo(10),
            "Should be trimmed to exactly the limit");
    }

    [Test]
    public void Push_ClearsRedoStackOnNewPush()
    {
        // Arrange — exercise undo/redo first so redo stack has entries
        _manager.Push("snap_0");
        _manager.Push("snap_1");
        string? undone = _manager.Undo();
        Assert.That(undone, Is.EqualTo("snap_0"), "Undo should return previous state");

        // Act — push a new entry (should clear redo stack)
        _manager.Push("new_snap");

        // Assert — no redo available after new action
        Assert.That(_manager.CanRedo, Is.False, "Redo stack should be cleared after new push");
    }

    #endregion

    #region Undo behavior

    [Test]
    public void Undo_WhenNoHistory_ReturnsNull()
    {
        string? result = _manager.Undo();
        Assert.That(result, Is.Null, "Should return null when no history exists");
    }

    [Test]
    public void Undo_AfterSinglePush_ReturnsNull()
    {
        _manager.Push("snap_0");
        string? result = _manager.Undo();
        Assert.That(result, Is.Null, "Cannot undo a single push (at beginning of history)");
    }

    [Test]
    public void Undo_ReturnsPreviousState()
    {
        _manager.Push("snap_0");
        _manager.Push("snap_1");
        _manager.Push("snap_2");

        string? result = _manager.Undo();
        Assert.That(result, Is.EqualTo("snap_1"), "Undo should return the previous state");
    }

    [Test]
    public void Undo_CanReplayAllEntries()
    {
        // Push 5 entries, then undo all of them one by one
        for (int i = 0; i < 5; i++)
            _manager.Push($"snap_{i}");

        var history = new List<string>();
        while (_manager.Undo() is string state)
            history.Add(state);

        Assert.That(history, Is.EqualTo(new[] { "snap_4", "snap_3", "snap_2", "snap_1" }),
            "Should be able to undo back to the first pushed entry");
    }

    #endregion

    #region Redo behavior

    [Test]
    public void Redo_AfterUndo_ReturnsNextState()
    {
        _manager.Push("snap_0");
        _manager.Push("snap_1");
        _manager.Undo(); // at snap_0

        string? result = _manager.Redo();
        Assert.That(result, Is.EqualTo("snap_1"), "Redo should return the next state from history");
    }

    [Test]
    public void Redo_AfterUndoThenNewPush_ReturnsNull()
    {
        // Undo then push new content (standard editor: new action clears redo)
        _manager.Push("snap_0");
        _manager.Push("snap_1");
        _manager.Undo();
        _manager.Push("new_snap");

        Assert.That(_manager.CanRedo, Is.False, "New push after undo should clear redo stack");
    }

    [Test]
    public void Redo_CanReplayAllUndoneEntries()
    {
        // Push 5 entries, undo all, then redo all back
        for (int i = 0; i < 5; i++)
            _manager.Push($"snap_{i}");

        while (_manager.Undo() is not null) { }

        var history = new List<string>();
        while (_manager.Redo() is string state)
            history.Add(state);

        Assert.That(history, Is.EqualTo(new[] { "snap_1", "snap_2", "snap_3", "snap_4" }),
            "Should be able to redo all undone entries");
    }

    #endregion

    #region UndoRedoManager with real UndoState serialization

    [Test]
    public void Push_Undo_Redo_WithRealUndoState_SerializationRoundTrip()
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };

        // Act — push a state containing an UndoState record
        var state1 = new EditViewModel.UndoState(
            GroupsJson: "[{\"id\":\"g1\",\"name\":\"Group 1\"}]",
            SelectedGroupId: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            SelectedProcessId: Guid.Empty);
        _manager.Push(JsonSerializer.Serialize(state1, jsonOptions));

        var state2 = new EditViewModel.UndoState(
            GroupsJson: "[{\"id\":\"g1\",\"name\":\"Group 2\"}]",
            SelectedGroupId: Guid.Parse("00000000-0000-0000-0000-000000000002"),
            SelectedProcessId: Guid.Empty);
        _manager.Push(JsonSerializer.Serialize(state2, jsonOptions));

        // Undo
        string? undone = _manager.Undo();
        Assert.That(undone, Is.Not.Null);
        var restoredState1 = JsonSerializer.Deserialize<EditViewModel.UndoState>(undone!, jsonOptions)!;
        Assert.That(restoredState1.GroupsJson, Does.Contain("Group 1"));

        // Redo
        string? redone = _manager.Redo();
        Assert.That(redone, Is.Not.Null);
        var restoredState2 = JsonSerializer.Deserialize<EditViewModel.UndoState>(redone!, jsonOptions)!;
        Assert.That(restoredState2.GroupsJson, Does.Contain("Group 2"));

        // Verify selection IDs survived round-trip
        Assert.That(restoredState1.SelectedGroupId.ToString(), Is.EqualTo("00000000-0000-0000-0000-000000000001"));
        Assert.That(restoredState2.SelectedGroupId.ToString(), Is.EqualTo("00000000-0000-0000-0000-000000000002"));
    }

    #endregion

    #region Clear behavior

    [Test]
    public void Clear_ResetsHistoryAndRedoStack()
    {
        _manager.Push("snap_0");
        _manager.Push("snap_1");
        _manager.Undo(); // creates redo entry

        Assert.That(_manager.CanUndo, Is.True);
        Assert.That(_manager.CanRedo, Is.True);

        _manager.Clear();

        Assert.That(_manager.HistoryCount, Is.EqualTo(0), "History should be empty after clear");
        Assert.That(_manager.CanUndo, Is.False, "Cannot undo after clear");
        Assert.That(_manager.CanRedo, Is.False, "Cannot redo after clear");
    }

    #endregion
}
