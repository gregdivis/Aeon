using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Aeon.Emulator.DebugSupport;
using Aeon.Emulator.Decoding;

namespace Aeon.Emulator.Launcher
{
    internal sealed class LogAccessor : IReadOnlyList<DebugLogItem>, IList<DebugLogItem>
    {
        private readonly ReadOnlyMemory<byte> buffer;

        static LogAccessor()
        {
            InstructionSet.Initialize();
        }

        public LogAccessor(ReadOnlyMemory<byte> buffer) => this.buffer = buffer;

        public DebugLogItem this[int index] => new(this.buffer.Slice(index * InstructionLog.EntrySize, InstructionLog.EntrySize));

        DebugLogItem IList<DebugLogItem>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        public int Count => this.buffer.Length / InstructionLog.EntrySize;

        bool ICollection<DebugLogItem>.IsReadOnly => true;

        public static LogAccessor Open(string fileName)
        {
            using var zip = new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read);

            int offset = 0;

            using var buffer = new MemoryStream();

            while (true)
            {
                var entry = zip.GetEntry(offset.ToString());
                if (entry == null)
                    break;
                using var entryStream = entry.Open();
                entryStream.CopyTo(buffer);
                offset += (int)entry.Length / InstructionLog.EntrySize;
            }

            return new LogAccessor(buffer.ToArray());
        }

        public int FindNextError(int start)
        {
            for (int i = start; i < this.Count; i++)
            {
                var item = this[i];
                if (item.HasError)
                    return i;
            }

            return -1;
        }

        public IEnumerator<DebugLogItem> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        void ICollection<DebugLogItem>.Add(DebugLogItem item) => throw new NotSupportedException();
        void ICollection<DebugLogItem>.Clear() => throw new NotSupportedException();
        bool ICollection<DebugLogItem>.Contains(DebugLogItem item) => throw new NotSupportedException();
        void ICollection<DebugLogItem>.CopyTo(DebugLogItem[] array, int arrayIndex) => throw new NotSupportedException();
        int IList<DebugLogItem>.IndexOf(DebugLogItem item) => throw new NotSupportedException();
        void IList<DebugLogItem>.Insert(int index, DebugLogItem item) => throw new NotSupportedException();
        bool ICollection<DebugLogItem>.Remove(DebugLogItem item) => throw new NotSupportedException();
        void IList<DebugLogItem>.RemoveAt(int index) => throw new NotSupportedException();
    }

    internal readonly struct DebugLogItem
    {
        private readonly ReadOnlyMemory<byte> data;

        public DebugLogItem(ReadOnlyMemory<byte> data) => this.data = data;

        public OpcodeInfo Opcode => InstructionSet.Decode(this.OpcodeSpan);

        public uint EAX => this.ReadUInt32(0);
        public uint EBX => this.ReadUInt32(1);
        public uint ECX => this.ReadUInt32(2);
        public uint EDX => this.ReadUInt32(3);
        public uint EBP => this.ReadUInt32(4);
        public uint ESP => this.ReadUInt32(5);
        public uint ESI => this.ReadUInt32(6);
        public uint EDI => this.ReadUInt32(7);
        public uint EIP => this.ReadUInt32(8);
        public EFlags Flags => (EFlags)this.ReadUInt32(9);
        public PrefixState Prefixes => (PrefixState)this.ReadUInt32(10);
        public CR0 CR0 => (CR0)this.ReadUInt32(11);

        public uint CS => this.ReadUInt16(0);
        public uint DS => this.ReadUInt16(1);
        public uint ES => this.ReadUInt16(2);
        public uint FS => this.ReadUInt16(3);
        public uint GS => this.ReadUInt16(4);
        public uint SS => this.ReadUInt16(5);

        public string RegisterText
        {
            get
            {
                var sb = new StringBuilder(100);

                append("EAX: ", this.EAX);
                append("ECX: ", this.ECX);
                append("EBX: ", this.EBX);
                append("EDX: ", this.EDX);
                append("ESP: ", this.ESP);
                append("EBP: ", this.EBP);
                append("ESI: ", this.ESI);
                append("EDI: ", this.EDI);

                append16("SS: ", this.SS);
                append16("DS: ", this.DS);
                append16("ES: ", this.ES);
                append16("FS: ", this.FS);
                append16("GS: ", this.GS);

                sb.Append("Flags: ");
                appendFlags(this.Flags);

                sb.Append("CR0: ");
                sb.AppendLine(this.CR0.ToString());

                sb.Append("Opcode: ");
                var span = this.OpcodeSpan;
                for (int i = 0; i < 4; i++)
                {
                    int n = span[i] >> 4;
                    sb.Append((char)(n < 10 ? ('0' + n) : ('A' + n - 10)));
                    n = span[i] & 0xF;
                    sb.Append((char)(n < 10 ? ('0' + n) : ('A' + n - 10)));
                }

                sb.AppendLine();

                return sb.ToString();

                void append(string name, uint value)
                {
                    sb.Append(name);
                    sb.AppendLine(value.ToString("X8"));
                }

                void append16(string name, uint value)
                {
                    sb.Append(name);
                    sb.AppendLine(value.ToString("X4"));
                }

                void appendFlags(EFlags f)
                {
                    sb.Append(f.HasFlag(EFlags.Carry) ? 'C' : ' ');
                    sb.Append(f.HasFlag(EFlags.Reserved1) ? 'R' : ' ');
                    sb.Append(f.HasFlag(EFlags.Parity) ? 'P' : ' ');
                    sb.Append(f.HasFlag(EFlags.Auxiliary) ? 'A' : ' ');
                    sb.Append(f.HasFlag(EFlags.Zero) ? 'Z' : ' ');
                    sb.Append(f.HasFlag(EFlags.Sign) ? 'S' : ' ');
                    sb.Append(f.HasFlag(EFlags.Trap) ? 'T' : ' ');
                    sb.Append(f.HasFlag(EFlags.InterruptEnable) ? 'I' : ' ');
                    sb.Append(f.HasFlag(EFlags.Direction) ? 'D' : ' ');
                    sb.Append(f.HasFlag(EFlags.Overflow) ? 'O' : ' ');
                    sb.Append(f.HasFlag(EFlags.Virtual8086Mode) ? 'V' : ' ');
                    sb.AppendLine();
                }
            }
        }

        public bool HasError => !InstructionDecoder.TryDecode(this.Opcode, this.data.Span, this.Prefixes).HasValue;

        private ReadOnlySpan<byte> OpcodeSpan => this.data.Span[(InstructionLog.GprSize + InstructionLog.SrSize)..];

        public override string ToString()
        {
            var opcode = this.Opcode;

            var prefixes = this.Prefixes;
            if (this.CR0.HasFlag(CR0.ProtectedModeEnable))
            {
                var sizePrefixes = prefixes & (PrefixState.OperandSize | PrefixState.AddressSize);
                prefixes &= ~(PrefixState.OperandSize | PrefixState.AddressSize);
                prefixes |= sizePrefixes ^ (PrefixState.OperandSize | PrefixState.AddressSize);
            }

            var decoded = InstructionDecoder.TryDecode(opcode, this.OpcodeSpan[opcode.Length..], prefixes);
            var sb = new StringBuilder(100);
            sb.Append(this.CS.ToString("X4"));
            sb.Append(':');
            sb.Append(this.EIP.ToString("X8"));
            sb.Append(' ');
            sb.Append(this.Opcode.Name);
            sb.Append(' ');
            sb.Append(decoded?.ToString((int)this.EIP, this.Prefixes) ?? "<ERR>");
            return sb.ToString();
        }

        private uint ReadUInt32(int offset)
        {
            MemoryMarshal.TryRead(this.data.Span.Slice(offset * 4, 4), out uint value);
            return value;
        }
        private uint ReadUInt16(int offset)
        {
            MemoryMarshal.TryRead(this.data.Span.Slice(InstructionLog.GprSize + offset * 2, 2), out ushort value);
            return value;
        }
    }
 }
