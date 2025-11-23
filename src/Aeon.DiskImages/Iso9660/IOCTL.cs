using System.Runtime.InteropServices;

namespace Aeon.DiskImages.Iso9660;

internal static class IOCTL
{
    private const uint FILE_ANY_ACCESS = 0;
    private const uint FILE_READ_ACCESS = 1;
    private const uint FILE_WRITE_ACCESS = 2;
    private const uint METHOD_BUFFERED = 0;
    private const uint METHOD_IN_DIRECT = 1;
    private const uint METHOD_OUT_DIRECT = 2;
    private const uint METHOD_NEITHER = 3;

    private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
    {
        return (deviceType << 16) | (access << 14) | (function << 2) | method;
    }

    public static class CDROM
    {
        private const uint IOCTL_CDROM_BASE = 0x00000002;

        public static readonly uint READ_TOC = CTL_CODE(IOCTL_CDROM_BASE, 0x0000, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint SEEK_AUDIO_MSF = CTL_CODE(IOCTL_CDROM_BASE, 0x0001, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint STOP_AUDIO = CTL_CODE(IOCTL_CDROM_BASE, 0x0002, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint PAUSE_AUDIO = CTL_CODE(IOCTL_CDROM_BASE, 0x0003, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint RESUME_AUDIO = CTL_CODE(IOCTL_CDROM_BASE, 0x0004, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint GET_VOLUME = CTL_CODE(IOCTL_CDROM_BASE, 0x0005, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint PLAY_AUDIO_MSF = CTL_CODE(IOCTL_CDROM_BASE, 0x0006, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint SET_VOLUME = CTL_CODE(IOCTL_CDROM_BASE, 0x000A, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint READ_Q_CHANNEL = CTL_CODE(IOCTL_CDROM_BASE, 0x000B, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint GET_CONTROL = CTL_CODE(IOCTL_CDROM_BASE, 0x000D, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint GET_LAST_SESSION = CTL_CODE(IOCTL_CDROM_BASE, 0x000E, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint RAW_READ = CTL_CODE(IOCTL_CDROM_BASE, 0x000F, METHOD_OUT_DIRECT, FILE_READ_ACCESS);
        public static readonly uint DISK_TYPE = CTL_CODE(IOCTL_CDROM_BASE, 0x0010, METHOD_BUFFERED, FILE_ANY_ACCESS);
        public static readonly uint GET_DRIVE_GEOMETRY = CTL_CODE(IOCTL_CDROM_BASE, 0x0013, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint GET_DRIVE_GEOMETRY_EX = CTL_CODE(IOCTL_CDROM_BASE, 0x0014, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint READ_TOC_EX = CTL_CODE(IOCTL_CDROM_BASE, 0x0015, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint GET_CONFIGURATION = CTL_CODE(IOCTL_CDROM_BASE, 0x0016, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint CHECK_VERIFY = CTL_CODE(IOCTL_CDROM_BASE, 0x0200, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint MEDIA_REMOVAL = CTL_CODE(IOCTL_CDROM_BASE, 0x0201, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint EJECT_MEDIA = CTL_CODE(IOCTL_CDROM_BASE, 0x0202, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint LOAD_MEDIA = CTL_CODE(IOCTL_CDROM_BASE, 0x0203, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint RESERVE = CTL_CODE(IOCTL_CDROM_BASE, 0x0204, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint RELEASE = CTL_CODE(IOCTL_CDROM_BASE, 0x0205, METHOD_BUFFERED, FILE_READ_ACCESS);
        public static readonly uint FIND_NEW_DEVICES = CTL_CODE(IOCTL_CDROM_BASE, 0x0206, METHOD_BUFFERED, FILE_READ_ACCESS);

        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY
        {
            public long Cylinders;
            public uint MediaType;
            public uint TracksPerCylinder;
            public uint SectorsPerTrack;
            public uint BytesPerSector;
        }
    }
}
