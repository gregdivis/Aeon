using Aeon.Emulator;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Test
{
    internal sealed class CpuState : IRegisterContainer
    {
        public uint EAX { get; set; }
        public uint EBX { get; set; }
        public uint ECX { get; set; }
        public uint EDX { get; set; }
        public uint ESI { get; set; }
        public uint EDI { get; set; }
        public uint EBP { get; set; }
        public uint ESP { get; set; }
        public ushort DS { get; set; }
        public ushort ES { get; set; }
        public ushort FS { get; set; }
        public ushort GS { get; set; }
        public ushort SS { get; set; }
        public EFlags Flags { get; set; }
        public CR0 CR0 { get; set; }
    }
}
