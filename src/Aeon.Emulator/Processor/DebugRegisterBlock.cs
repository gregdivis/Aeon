using System.Runtime.InteropServices;

namespace Aeon.Emulator;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DebugRegisterBlock
{
    public uint DR0;
    public uint DR1;
    public uint DR2;
    public uint DR3;
    public uint DR4;
    public uint DR5;
    public uint DR6;
    public uint DR7;
}
