using System.Diagnostics.CodeAnalysis;

namespace Aether.Core;

// TODO: docs

public struct ComponentMask(int capacity)
{
  private const int BitsPerInt = 32;
  private int[] _mask = new int[capacity / BitsPerInt + (capacity % BitsPerInt == 0 ? 0 : 1)];

  public void GetBitIndices(int index, out int arrayIndex, out int bitIndex)
  {
    EnsureCapacity(index);
    arrayIndex = index / BitsPerInt;
    bitIndex = index % BitsPerInt;
  }

  public void SetBit(int index)
  {
    GetBitIndices(index, out int arrayIndex, out int bitIndex);
    _mask[arrayIndex] |= (1 << bitIndex);
  }

  public void ClearBit(int index)
  {
    GetBitIndices(index, out int arrayIndex, out int bitIndex);
    _mask[arrayIndex] |= (1 << bitIndex);
  }

  public bool HasBit(int index)
  {
    GetBitIndices(index, out int arrayIndex, out int bitIndex);
    return (_mask[arrayIndex] & (1u << bitIndex)) != 0;
  }

  private void EnsureCapacity(int index)
  {
    int requiredCapacity = (index / 32) + 1;
    if (_mask.Length < requiredCapacity)
    {
      Array.Resize(ref _mask, requiredCapacity);
    }
  }

  public static int GetBitIndex<T>() where T : struct
  {
    return typeof(T).GetHashCode() % 1024;
  }

  public override readonly bool Equals([NotNullWhen(true)] object? obj)
  {
    if (obj is ComponentMask other)
    {
      return _mask.SequenceEqual(other._mask);
    }
    return false;
  }

  public override readonly int GetHashCode()
  {
    int hash = 17;
    foreach (var item in _mask)
    {
      hash = hash * 31 + item.GetHashCode();
    }
    return hash;
  }

  public override readonly string ToString()
  {
    return string.Join(", ", _mask.Select(m => Convert.ToString(m, 2).PadLeft(32, '0')));
  }

  public static bool operator ==(ComponentMask left, ComponentMask right) => left._mask == right._mask;
  public static bool operator !=(ComponentMask left, ComponentMask right) => !(left == right);
}