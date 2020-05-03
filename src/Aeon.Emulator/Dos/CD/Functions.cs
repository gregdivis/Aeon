namespace Aeon.Emulator.Dos.CD
{
    internal static class Functions
    {
        public const byte GetNumberOfCDRomDrives = 0x00;
        public const byte GetCDRomDeviceList = 0x01;
        public const byte GetCopyrightFileName = 0x02;
        public const byte GetAbstractFileName = 0x03;
        public const byte GetBibliographicDocFileName = 0x04;
        public const byte ReadVTOC = 0x05;
        public const byte DebuggingOn = 0x06;
        public const byte DebuggingOff = 0x07;
        public const byte AbsoluteDiskRead = 0x08;
        public const byte AbsoluteDiskWrite = 0x09;
        public const byte CDRomDriveCheck = 0x0B;
        public const byte MSCDEXVersion = 0x0C;
        public const byte GetCDRomDriveLetters = 0x0D;
        public const byte GetSetVolumeDescriptorPreference = 0x0E;
        public const byte GetDirectoryEntry = 0x0F;
        public const byte SendDeviceRequest = 0x10;
    }
}
