using Aether.Utils;

namespace Aether.Core;

public struct Slot(int index, int chunkIndex)
{
    public int Index = index;
    public int ChunkIndex = chunkIndex;

    public static Slot operator +(Slot first, Slot second) =>
        new(first.Index + second.Index, first.ChunkIndex + second.ChunkIndex);

    public void Wrap(int capacity)
    {
        if (Index < capacity) return;
        ChunkIndex += Index / capacity;
        Index %= capacity;
    }

    public static Slot Shift(ref Slot slot, int capacity)
    {
        slot.Index++;
        slot.Wrap(capacity);
        return slot;
    }
}

public partial class Archetype : IEnumerable<(int chunkIndex, int entityIndex)>
{
    private readonly ComponentMask _mask;
    private readonly List<Chunk> _chunks;
    private readonly StructPool<Chunk> _chunkPool;
    private readonly int _chunkSize;

    public Archetype(ComponentMask mask, int chunkSize = 1024, int initialPoolSize = 10, int maxPoolSize = 100)
    {
        _mask = mask;
        _chunkSize = chunkSize;
        _chunks = [];
        _chunkPool = new StructPool<Chunk>(maxPoolSize);
        for (int i = 0; i < initialPoolSize; i++)
        {
            _chunkPool.Return(new Chunk(_chunkSize));
        }
        _chunks.Add(_chunkPool.Rent());
    }

    public void CopyComponents<T>(int chunkSourceIndex, int sourceEntityIndex, int destChunkIndex, int destEntityIndex)
    {
        ref var source = ref _chunks[sourceChunkIndex].GetSpan<T>(sourceEntityIndex);
        ref var target = ref _chunks[targetChunkIndex].GetSpan<T>(targetEntityIndex);
        target = source;
    }
}