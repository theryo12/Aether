using System.Collections;

namespace Aether.Utils;

public readonly struct Enumerator : IEnumerator<int>
{
    private readonly IEnumerator<int> _rangeEnumerator;

    public Enumerator(int count)
    {
        _rangeEnumerator = Enumerable.Range(0, count).GetEnumerator();
    }

    public int Current => _rangeEnumerator.Current;

    object IEnumerator.Current => Current;

    public bool MoveNext() => _rangeEnumerator.MoveNext();

    public void Reset() => throw new NotSupportedException("Reset is not supported for this Enumerator.");

    public void Dispose() => _rangeEnumerator.Dispose();
}