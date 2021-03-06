﻿using System;
using System.Threading;
using TinyAudio;

namespace Aeon.Emulator.Sound
{
    internal static class Audio
    {
        public static AudioPlayer CreatePlayer(bool useCallback = false) => WasapiAudioPlayer.Create(TimeSpan.FromSeconds(0.25), useCallback);

        public static void WriteFullBuffer(AudioPlayer player, ReadOnlySpan<float> buffer)
        {
            var writeBuffer = buffer;

            while (true)
            {
                int count = (int)player.WriteData(writeBuffer);
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
                int count = (int)player.WriteData(writeBuffer);
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
                int count = (int)player.WriteData(writeBuffer);
                writeBuffer = writeBuffer[count..];
                if (writeBuffer.IsEmpty)
                    return;

                Thread.Sleep(1);
            }
        }
    }
}
