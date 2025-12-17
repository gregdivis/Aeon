using System.Runtime.InteropServices;

namespace Aeon.Emulator;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SegmentRegisterBlock<T> where T : unmanaged
{
    public T ES;
    public T CS;
    public T SS;
    public T DS;
    public T FS;
    public T GS;
}
