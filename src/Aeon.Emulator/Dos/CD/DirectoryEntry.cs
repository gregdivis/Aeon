using System.Runtime.InteropServices;

namespace Aeon.Emulator.Dos.CD
{
    /// <summary>
    /// ISO-9660 directory entry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DirectoryEntry
    {
        /// <summary>
        /// Length of this directory entry.
        /// </summary>
        public byte len_dr;
        /// <summary>
        /// Length of XAR in LBN's.
        /// </summary>
        public byte XAR_len;
        /// <summary>
        /// LBN of data (Intel format).
        /// </summary>
        public uint loc_extentI;
        /// <summary>
        /// LBN of data (Motorola format).
        /// </summary>
        public uint loc_extentM;
        /// <summary>
        /// Length of file (Intel format).
        /// </summary>
        public uint data_lenI;
        /// <summary>
        /// Length of file (Motorola format).
        /// </summary>
        public uint data_lenM;
        /// <summary>
        /// Date and time.
        /// </summary>
        public unsafe fixed byte record_time[7];
        /// <summary>
        /// File attributes.
        /// </summary>
        public byte file_flags_iso;
        /// <summary>
        /// Interleave size.
        /// </summary>
        public byte il_size;
        /// <summary>
        /// Interleave skip factor.
        /// </summary>
        public byte il_skip;
        /// <summary>
        /// Volume set sequence number (Intel).
        /// </summary>
        public ushort VSSNI;
        /// <summary>
        /// Volume set sequence number (Motorola).
        /// </summary>
        public ushort VSSNM;
        /// <summary>
        /// Length of name.
        /// </summary>
        public byte len_fi;
    }
}
