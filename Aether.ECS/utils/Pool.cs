using System.Runtime.CompilerServices;

namespace Aether.Utils;

public class StructPool<T>(int capacity) where T : struct
{
  private readonly T[] _pool = new T[capacity];
  private int _index = capacity - 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public T Rent()
  {
    if (_index >= 0)
    {
      return _pool[_index--];
    }
    return default;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Return(T item)
  {
    if (_index + 1 < _pool.Length)
    {
      _pool[++_index] = item;
    }
  }

  public void Clear()
  {
    Array.Clear(_pool, 0, _pool.Length);
    _index = _pool.Length - 1;
  }

  public int Count => _index + 1;
}