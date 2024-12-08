
using System.Buffers;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aether.Utils;

namespace Aether.Core;

/// <summary>
///     Represents a memory chunk designed for stroing entities and their
///     associated components.
/// </summary>
[StructLayout(LayoutKind.Auto, Pack = 1)]
public struct Chunk : IEnumerable<int>
{
    private const int MaxEntities = 1024; // chunk capacity
    private const int MaxComponents = 64; // max components per entity

    private readonly int _entityCount;
    private readonly ComponentMask[] _masks;
    private readonly int[] _dataOffsets;

    public Chunk(int entityCount)
    {
        if ((uint)entityCount > MaxEntities || (uint)entityCount < 0)
            throw new ArgumentOutOfRangeException(nameof(entityCount), $"Entity count must be in range 1-{MaxEntities}.");

        _entityCount = entityCount;
        // we explictily initialize ComponentMask array to avoid NREs
        _masks = new ComponentMask[entityCount];
        for (int i = 0; i < entityCount; i++)
            _masks[i] = new ComponentMask(MaxComponents);

        _dataOffsets = new int[entityCount * MaxComponents];
    }

    /// <summary>
    ///     Gets the number of entities stored in the chunk.
    /// </summary>
    public readonly int Count => _entityCount;

    /// <summary>
    ///     Checks if the specified entity has a given component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        return _masks[entityIndex].HasBit(ComponentMask.GetBitIndex<T>());
    }

    /// <summary>
    ///     Adds a component to the specified entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Add<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        int index = ComponentMask.GetBitIndex<T>();
        _masks[entityIndex].SetBit(index);
        _dataOffsets[FlatIndex(entityIndex, index)] = CalculateOffset<T>(entityIndex);
    }

    /// <summary>
    ///     Removes a component from specified entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Remove<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        _masks[entityIndex].ClearBit(ComponentMask.GetBitIndex<T>());
    }

    /// <summary>
    ///     Copies all data from another chunk into this one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyFrom(ref Chunk source)
    {
        if (source._entityCount != _entityCount)
            throw new ArgumentException("Chunk sizes must match.", nameof(source));

        Array.Copy(source._masks, _masks, _entityCount);
        Array.Copy(source._dataOffsets, _dataOffsets, _dataOffsets.Length);
    }

    /// <summary>
    ///     Provides a span over the memory offsets for a specific component across all entities in the chunk.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<int> GetSpan<T>() where T : struct
    {
        int componentIndex = ComponentMask.GetBitIndex<T>();
        int start = componentIndex * _entityCount;
        return new Span<int>(_dataOffsets, start, _entityCount);
    }

    public readonly Enumerator GetEnumerator() => new(_entityCount);

    readonly IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Validates that the entity index is within the valid range for the chunk.
    /// </summary>
    /// <param name="entityIndex"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void ValidateEntityIndex(int entityIndex)
    {
        if ((uint)entityIndex >= (uint)_entityCount)
            throw new ArgumentOutOfRangeException(nameof(entityIndex), $"Index {entityIndex} is out of range.");
    }

    /// <summary>
    ///     Calculates the memory offset for a component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int CalculateOffset<T>(int entityIndex) where T : struct
        => entityIndex * Unsafe.SizeOf<T>();

    /// <summary>
    ///     Calculates the flat index for an entity and component pair.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int FlatIndex(int entityIndex, int componentIndex)
        => entityIndex * MaxComponents + componentIndex;
}