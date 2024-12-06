
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aether.Utils;

namespace Aether.Core;

/// <summary>
///     Represents a memory chunk designed for stroing entities and their
///     associated components.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Chunk : IEnumerable<int>
{
    private const int MaxEntities = 1024; // chunk capacity
    private const int BitsPerInt = 32; // number of bits in an integer, for mask calculations
    private const int MaxComponents = 64; // maximum components per entity, constrained by mask size

    private int _entityCount;
    private ComponentMask[] _masks;
    private int[,] _dataOffsets;

    /// <summary>
    ///     Initializes a new chunk for storing entities and their components.
    /// </summary>
    /// <param name="entityCount">The number of entities (cannot exceed <see cref="MaxEntities"/>).</param>
    public Chunk(int entityCount)
    {
        if (entityCount > MaxEntities)
            throw new ArgumentOutOfRangeException(nameof(entityCount), $"Entity count cannot exceed {MaxEntities}");
        ArgumentOutOfRangeException.ThrowIfGreaterThan(MaxEntities, entityCount);

        _entityCount = entityCount;
        _masks = new ComponentMask[entityCount];
        _dataOffsets = new int[entityCount, MaxComponents];
    }

    /// <summary>
    ///     Gets the number of entities stored in the chunk.
    /// </summary>
    public readonly int Count => _entityCount;

    /// <summary>
    ///     Checks if specified entity has a given component.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <param name="entityIndex">The index of entity in the chunk.</param>
    /// <returnsTrue if component exists; otherwise, false.></returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Has<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        return _masks[entityIndex].HasBit(ComponentMask.GetBitIndex<T>());
    }

    /// <summary>
    ///     Adds a component to the specified entity.
    /// </summary>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    /// <param name="entityIndex">The index of the entity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Add<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        int index = ComponentMask.GetBitIndex<T>();
        _masks[entityIndex].SetBit(index);

        _dataOffsets[entityIndex, index] = CalculateOffset<T>(entityIndex);
    }

    /// <summary>
    ///     Removes a component from the specified entity.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    /// <param name="entityIndex">The index of the entity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Remove<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        _masks[entityIndex].ClearBit(ComponentMask.GetBitIndex<T>());
    }

    /// <summary>
    /// Copies all data from another chunk into this one.
    /// </summary>
    /// <param name="source">The source chunk to copy from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyFrom(ref Chunk source)
    {
        if (source.Count != _entityCount)
            throw new ArgumentException("Chunk sizes must match.", nameof(source));

        Array.Copy(source._masks, _masks, _entityCount);
        Array.Copy(source._dataOffsets, _dataOffsets, _dataOffsets.Length);
    }

    /// <summary>
    /// Provides a span over the memory offsets for a specific component of an entity.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    /// <param name="entityIndex">The index of the entity in the chunk.</param>
    /// <returns>A <see cref="Span{T}"/> over the memory offsets for the component.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<int> GetSpan<T>(int entityIndex) where T : struct
    {
        ValidateEntityIndex(entityIndex);
        ref int offset = ref _dataOffsets[entityIndex, ComponentMask.GetBitIndex<T>()];
        return MemoryMarshal.CreateSpan(ref offset, 1);
    }

    [Pure]
    public readonly Enumerator GetEnumerator() => new(_entityCount);

    readonly IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Validates that the entity index is within the valid range for the chunk.
    /// </summary>
    private readonly void ValidateEntityIndex(int entityIndex)
    {
        if ((uint)entityIndex >= (uint)_entityCount)
            throw new IndexOutOfRangeException($"Entity {entityIndex} is out of range.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int CalculateOffset<T>(int entityIndex) where T : struct
    {
        return entityIndex * Unsafe.SizeOf<T>();
    }
}