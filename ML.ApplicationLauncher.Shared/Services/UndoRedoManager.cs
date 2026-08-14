// Copyright © Martin lacina

using System.Collections.Generic;
using System.Text.Json;

namespace ML.ApplicationLauncher.Shared.Services;

/// <summary>
/// Generic undo/redo orchestration manager.
/// Owns the collection of snapshots, trim-to-limit policy, and redo invalidation semantics.
/// Handles JSON serialization/deserialization internally so callers work with raw snapshot objects.
/// </summary>
public class UndoRedoManager<TSnapshot>
{
    private readonly List<string> _history = new();
    private int _index; // Index pointing to the current state in _history
    private readonly Stack<string> _redoStack = new();

    /// <summary>
    /// Maximum number of undo history entries. When exceeded, oldest entries (from index 0) are discarded.
    /// </summary>
    public int MaxHistorySize { get; }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public UndoRedoManager(int maxHistorySize)
    {
        if (maxHistorySize < 1) throw new System.ArgumentOutOfRangeException(nameof(maxHistorySize), "Must be at least 1.");
        MaxHistorySize = maxHistorySize;
    }

    /// <summary>
    /// Number of entries currently in the undo history.
    /// </summary>
    public int HistoryCount => _history.Count;

    /// <summary>
    /// Current position within the history (0-based). -1 means no state has been pushed yet.
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// Whether undo is available at the current position.
    /// </summary>
    public bool CanUndo => _index > 0 && _history.Count > 0;

    /// <summary>
    /// Whether redo is available (redo stack has entries).
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Push a snapshot onto the history. Clears the redo stack (standard undo/redo semantics: new action invalidates redo).
    /// Trims oldest entries if the limit is exceeded.
    /// </summary>
    public void Push(TSnapshot snapshot)
    {
        _history.Add(JsonSerializer.Serialize(snapshot, _jsonOptions));

        // Trim oldest entries from the front to stay within limit
        while (_history.Count > MaxHistorySize)
            _history.RemoveAt(0);

        // New action invalidates redo stack (standard editor behavior)
        _redoStack.Clear();

        // Move index to the newly added entry
        _index = _history.Count - 1;
    }

    /// <summary>
    /// Undo one step: move back in history. Returns the previous snapshot, or null if undo is unavailable.
    /// </summary>
    public TSnapshot? Undo()
    {
        if (!CanUndo) return default;

        _index--;
        var json = _history[_index];
        return JsonSerializer.Deserialize<TSnapshot>(json, _jsonOptions);
    }

    /// <summary>
    /// Redo one step: move forward from history. Returns the next snapshot, or null if redo is unavailable.
    /// </summary>
    public TSnapshot? Redo()
    {
        if (!CanRedo) return default;

        // Save current state back into history before moving forward
        _history.Insert(_index + 1, _history[_index]);
        _index++;

        var json = _redoStack.Pop();
        return JsonSerializer.Deserialize<TSnapshot>(json, _jsonOptions);
    }

    /// <summary>
    /// Clear all undo history (useful when loading a fresh configuration).
    /// </summary>
    public void Clear()
    {
        _history.Clear();
        _redoStack.Clear();
        _index = -1;
    }
}
