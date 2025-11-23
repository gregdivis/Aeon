using System.Collections.Concurrent;

namespace Aeon.Emulator.Keyboard;

internal static class ConcurrentQueueExtensions
{
    public static T Dequeue<T>(this ConcurrentQueue<T> queue, T defaultValue) => queue.TryDequeue(out var value) ? value : defaultValue;
    public static T Peek<T>(this ConcurrentQueue<T> queue, T defaultValue) => queue.TryPeek(out var value) ? value : defaultValue;
}
