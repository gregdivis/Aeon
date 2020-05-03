using System;

namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Contains a list of recently decoded instructions.
    /// </summary>
    public sealed class InstructionLog : IDisposable
    {
        private readonly LogWriter logWriter;

        public const int GprSize = 12 * 4;
        public const int SrSize = 6 * 2;
        public const int EntrySize = GprSize + SrSize + 16;

        public InstructionLog(string fileName) => this.logWriter = LogWriter.Create(fileName);

        public void Dispose() => this.logWriter.Dispose();

        internal void Write(Processor processor)
        {
            unsafe
            {
                const int bufferSize = GprSize + SrSize;

                var buffer = stackalloc byte[bufferSize];
                uint* gpr = (uint*)buffer;
                gpr[0] = (uint)processor.EAX;
                gpr[1] = (uint)processor.EBX;
                gpr[2] = (uint)processor.ECX;
                gpr[3] = (uint)processor.EDX;
                gpr[4] = processor.EBP;
                gpr[5] = processor.ESP;
                gpr[6] = processor.ESI;
                gpr[7] = processor.EDI;
                gpr[8] = processor.EIP - (uint)processor.PrefixCount;
                gpr[9] = (uint)processor.Flags.Value;
                gpr[10] = (uint)GetPrefixState(processor);
                gpr[11] = (uint)processor.CR0;

                ushort* segs = (ushort*)(buffer + GprSize);
                segs[0] = processor.CS;
                segs[1] = processor.DS;
                segs[2] = processor.ES;
                segs[3] = processor.FS;
                segs[4] = processor.GS;
                segs[5] = processor.SS;

                this.logWriter.Write(new ReadOnlySpan<byte>(buffer, bufferSize), new ReadOnlySpan<byte>(processor.CachedInstruction, 16));
            }
        }

        private static PrefixState GetPrefixState(Processor p)
        {
            var value = p.SegmentOverride switch
            {
                SegmentRegister.CS => PrefixState.CS,
                SegmentRegister.DS => PrefixState.DS,
                SegmentRegister.ES => PrefixState.ES,
                SegmentRegister.FS => PrefixState.FS,
                SegmentRegister.GS => PrefixState.GS,
                SegmentRegister.SS => PrefixState.SS,
                _ => PrefixState.None,
            };

            if ((p.SizeOverride & 1) != 0)
                value |= PrefixState.OperandSize;

            if ((p.SizeOverride & 2) != 0)
                value |= PrefixState.AddressSize;

            switch (p.RepeatPrefix)
            {
                case RepeatPrefix.Repe:
                    value |= PrefixState.Repe;
                    break;

                case RepeatPrefix.Repne:
                    value |= PrefixState.Repne;
                    break;
            }

            return value;
        }
    }
}
