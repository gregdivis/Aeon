using System.Runtime.InteropServices;

namespace Aeon.Emulator.Dos;

/// <summary>
/// Defines the layout of the DOS swappable data area.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SwappableDataArea
{
    /// <summary>
    /// Critical error flag.
    /// </summary>
    public byte ErrorMode;
    /// <summary>
    /// In-DOS flag.
    /// </summary>
    public byte InDOS;
    /// <summary>
    /// Drive on which current critical error occurred or FFh.
    /// </summary>
    public byte ErrorDrive;
    /// <summary>
    /// Locus of last error.
    /// </summary>
    public byte LastErrorLocus;
    /// <summary>
    /// Extended error code of last error.
    /// </summary>
    public ushort LastErrorExtendedCode;
    /// <summary>
    /// Suggested action for last error.
    /// </summary>
    public byte LastErrorSuggestedAction;
    /// <summary>
    /// Class of last error.
    /// </summary>
    public byte LastErrorClass;
    /// <summary>
    /// DI value for last error.
    /// </summary>
    public ushort LastErrorOffset;
    /// <summary>
    /// ES value for last error.
    /// </summary>
    public ushort LastErrorSegment;
    /// <summary>
    /// Offset of current disk transfer area.
    /// </summary>
    public ushort CurrentDTAOffset;
    /// <summary>
    /// Segment of current disk transfer area. 
    /// </summary>
    public ushort CurrentDTASegment;
    /// <summary>
    /// Current program segment prefix.
    /// </summary>
    public ushort CurrentPSP;
    /// <summary>
    /// Stores SP across an INT 23.
    /// </summary>
    public ushort INT23SP;
    /// <summary>
    /// Return code from last process.
    /// </summary>
    public ushort ReturnCode;
    /// <summary>
    /// Current drive.
    /// </summary>
    public byte CurrentDrive;
    /// <summary>
    /// Extended break flag.
    /// </summary>
    public byte ExtendedBreakFlag;
    /// <summary>
    /// Code page switching flag.
    /// </summary>
    public byte CodePageSwitchingFlag;
    /// <summary>
    /// Copy of previous byte.
    /// </summary>
    public byte INT24ByteCopy;
}
