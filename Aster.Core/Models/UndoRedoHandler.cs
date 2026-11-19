using System;
using System.Collections.Generic;

namespace Aster.Core.Models;

public sealed class UndoRedoHandler<T>
{
    private readonly Action? _onCollectionChanged;

    private sealed class UndoRedoNode
    {
        internal readonly T? Data;

        internal UndoRedoNode? Previous;

        internal UndoRedoNode? Next;

        internal UndoRedoNode(T? data, UndoRedoNode? previous = null, UndoRedoNode? next = null)
        {
            Data = data;
            Previous = previous;
            Next = next;

            if (Previous is not null)
                Previous.Next = this;

            if (Next is not null)
                Next.Previous = this;
        }
    }

    private readonly UndoRedoNode _head;
    private readonly UndoRedoNode _tail;
    private UndoRedoNode _current;

    public T? Current => _current.Data;

    public UndoRedoHandler(Action? onCollectionChanged = null)
    {
        _onCollectionChanged = onCollectionChanged;
        _head = new UndoRedoNode(default);
        _tail = new UndoRedoNode(default, _head);
        _current = _head;
    }

    public void AddNewNode(T data)
    {
        if (_current == _tail)
            MoveToPrevious();

        _current = new UndoRedoNode(data, _current, _tail);
        _onCollectionChanged?.Invoke();
    }

    public bool IsTail() => _current == _tail;

    public bool IsHead() => _current == _head;

    public T? GetPrevious() => _current.Previous is null ? default : _current.Previous.Data;

    public T? GetNext() => _current.Next is null ? default : _current.Next.Data;

    public void MoveToPrevious()
    {
        if (_current.Previous is null)
            return;

        _current = _current.Previous;
        _onCollectionChanged?.Invoke();
    }

    public void MoveToNext()
    {
        if (_current.Next is null)
            return;

        _current = _current.Next;
        _onCollectionChanged?.Invoke();
    }

    public IEnumerable<T> EnumerateFromCurrentForward()
    {
        UndoRedoNode? current = _current.Next;

        while (current is not null)
        {
            if (current.Data is not null)
                yield return current.Data;

            current = current.Next;
        }
    }

    public IEnumerable<T> EnumerateFromCurrentBack()
    {
        UndoRedoNode? current = _current.Previous;

        while (current is not null)
        {
            if (current.Data is not null)
                yield return current.Data;

            current = current.Previous;
        }
    }

    public void RemoveCurrent(bool goToPrevious)
    {
        if (
            _current.Previous is null
            || _current.Next is null
            || _current == _head
            || _current == _tail
        )
            throw new InvalidOperationException(nameof(_current));

        _current.Previous.Next = _current.Next;
        _current.Next.Previous = _current.Previous;
        _current = goToPrevious ? _current.Previous : _current.Next;
        _onCollectionChanged?.Invoke();
    }
}
