using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator;

internal readonly struct SparseArray<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : struct, IBinaryInteger<TKey>
    where TValue : class
{
    private readonly TValue?[] items;
    private readonly TKey minKey;

    public SparseArray(IReadOnlyDictionary<TKey, TValue> source)
    {
        TKey min = default;
        TKey max = default;

        foreach (var key in source.Keys)
        {
            if (key < min)
                min = key;
            if (key > max)
                max = key;
        }

        TKey range = max - min;
        if (long.CreateSaturating(range) >= int.MaxValue)
            throw new ArgumentException("Source collection is too large.");

        int intRange = int.CreateTruncating(range) + 1;

        this.items = new TValue?[intRange];

        foreach (var item in source)
        {
            int index = int.CreateTruncating(item.Key - min);
            this.items[index] = item.Value;
        }

        this.Count = source.Count;
        this.minKey = min;
    }

    public TValue this[TKey key] => this.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

    public int Count { get; }
    public IEnumerable<TKey> Keys => this.Select(i => i.Key);
    public IEnumerable<TValue> Values => this.Select(i => i.Value);

    public bool ContainsKey(TKey key) => this.TryGetValue(key, out _);
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < this.items.Length; i++)
        {
            if (this.items[i] is not null)
                yield return KeyValuePair.Create(TKey.CreateTruncating(i) + this.minKey, this.items[i]!);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        int index = int.CreateTruncating(key - this.minKey);
        if (index >= 0 && index < this.items.Length)
        {
            value = Unsafe.Add(ref MemoryMarshal.GetReference(this.items), index);
            return value is not null;
        }

        value = null;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
