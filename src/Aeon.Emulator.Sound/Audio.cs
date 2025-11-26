using TinyAudio;

namespace Aeon.Emulator.Sound;

internal static class Audio
{
    public static AudioPlayer CreatePlayer(bool useCallback = false) => AudioPlayer.CreateDefault(TimeSpan.FromSeconds(0.25), useCallback);

    public static void WriteFullBuffer(AudioPlayer player, ReadOnlySpan<float> buffer)
    {
        var writeBuffer = buffer;

        while (true)
        {
            int count = player.WriteData(writeBuffer);
            writeBuffer = writeBuffer[count..];
            if (writeBuffer.IsEmpty)
                return;

            Thread.Sleep(1);
        }
    }
    public static void WriteFullBuffer(AudioPlayer player, ReadOnlySpan<short> buffer)
    {
        var writeBuffer = buffer;

        while (true)
        {
            int count = player.WriteData(writeBuffer);
            writeBuffer = writeBuffer[count..];
            if (writeBuffer.IsEmpty)
                return;

            Thread.Sleep(1);
        }
    }
    public static void WriteFullBuffer(AudioPlayer player, ReadOnlySpan<byte> buffer)
    {
        var writeBuffer = buffer;

        while (true)
        {
            int count = player.WriteData(writeBuffer);
            writeBuffer = writeBuffer[count..];
            if (writeBuffer.IsEmpty)
                return;

            Thread.Sleep(1);
        }
    }
}
