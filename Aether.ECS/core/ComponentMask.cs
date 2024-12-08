using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Aether.Utils;

namespace Aether.Core;

// TODO: docs
public struct ComponentMask : IDisposable
{
  private const int BitsPerInt = 32;

  private readonly int[] _mask;
  private readonly int _capacity;

  public ComponentMask(int capacity)
  {
    _capacity = (capacity + BitsPerInt - 1) / BitsPerInt;
    _mask = ArrayPool<int>.Shared.Rent(_capacity);
    Array.Clear(_mask, 0, _capacity);
  }

  public readonly void Dispose()
  {
    if (_mask != null)
    {
      ArrayPool<int>.Shared.Return(_mask);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly void SetBit(int index)
  {
    var (arrayIndex, bitIndex) = GetIndices(index);
    _mask[arrayIndex] |= 1 << bitIndex;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly void ClearBit(int index)
  {
    var (arrayIndex, bitIndex) = GetIndices(index);
    _mask[arrayIndex] &= ~(1 << bitIndex);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly bool HasBit(int index)
  {
    var (arrayIndex, bitIndex) = GetIndices(index);
    return (_mask[arrayIndex] & (1 << bitIndex)) != 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly void ClearAll()
  {
    Array.Clear(_mask, 0, _capacity);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (int ArrayIndex, int BitIndex) GetIndices(int index)
  {
    return (index / BitsPerInt, index % BitsPerInt);
  }

  private static class TypeIDs
  {
    internal static int LastID;
  }

  private static class TypeIDs<T>
  {
    public static readonly int ID = Interlocked.Increment(ref TypeIDs.LastID);
  }

  public static int GetBitIndex<T>() where T : struct
  {
    int id = TypeIDs<T>.ID;
    if (id >= 1024)
    {
      throw new InvalidOperationException("Exceeded the maximum limit of 1024 unique bit indices.");
    }
    return id;
  }

  public override readonly int GetHashCode()
  {
    int hash = 17;
    for (int i = 0; i < _capacity; i++)
    {
      hash = (hash * 31) ^ _mask[i];
    }
    return hash;
  }

  public override readonly bool Equals(object? obj)
  {
    if (obj is not ComponentMask other || _capacity != other._capacity)
      return false;

    for (int i = 0; i < _capacity; i++)
    {
      if (_mask[i] != other._mask[i])
        return false;
    }
    return true;
  }

  public override readonly string ToString()
  {
    var sb = new StringBuilder();
    for (int i = 0; i < _capacity; i++)
    {
      sb.Append(Convert.ToString(_mask[i], 2).PadLeft(BitsPerInt, '0')).Append(",");
    }
    return sb.ToString().TrimEnd(',');
  }

  public static bool operator ==(ComponentMask left, ComponentMask right) => left.Equals(right);
  public static bool operator !=(ComponentMask left, ComponentMask right) => !(left == right);
}