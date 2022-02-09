namespace Aeon.Emulator.Dos.CD
{
    internal static class CommandCodes
    {
        public const byte Init = 0;
        public const byte Read = 3;
        public const byte Write = 12;
        public const byte ReadLong = 128;
        public const byte ReadLongPrefetch = 130;
        public const byte Seek = 131;
        public const byte PlayAudio = 132;
        public const byte StopAudio = 133;
        public const byte ResumeAudio = 136;
        public const byte WriteLong = 134;
        public const byte WriteLongVerify = 136;
        public const byte InputFlush = 7;
        public const byte OutputFlush = 11;
        public const byte DeviceOpen = 13;
        public const byte DeviceClose = 14;
    }
}
