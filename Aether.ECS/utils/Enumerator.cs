using System.Collections;

namespace Aether.Utils;

public struct Enumerator(int count) : IEnumerator<int>
{
    private readonly int _count = count;
    private int _current = -1;

    public readonly int Current => _current;

    readonly object IEnumerator.Current => Current;

    public bool MoveNext() => ++_current < _count;

    public void Reset() => _current = -1;

    public readonly void Dispose() { }
}