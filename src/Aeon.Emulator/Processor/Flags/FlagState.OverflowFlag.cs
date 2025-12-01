using System.Runtime.CompilerServices;

namespace Aeon.Emulator;

partial class FlagState
{
    private struct OverflowFlag
    {
        private bool? currentValue;
        private FlagOperation operation;
        private uint a;
        private uint b;
        private uint c;

        public OverflowFlag(bool initialValue) : this() => this.currentValue = initialValue;

        public bool Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.currentValue ?? this.CalculateValue();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.currentValue = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazy(FlagOperation operation, uint a)
        {
            this.operation = operation;
            this.a = a;
            this.currentValue = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazy(FlagOperation operation, uint a, uint b)
        {
            this.operation = operation;
            this.a = a;
            this.b = b;
            this.currentValue = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazy(FlagOperation operation, uint a, uint b, uint c)
        {
            this.operation = operation;
            this.a = a;
            this.b = b;
            this.c = c;
            this.currentValue = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool CalculateValue()
        {
            int signed;
            long longSigned;

            bool value = this.operation switch
            {
                FlagOperation.Mul => this.a != 0,

                FlagOperation.IMul => this.a != 0 && this.a != uint.MaxValue,

                FlagOperation.IMul23_Word => (signed = (short)(ushort)this.a * (short)(ushort)this.b) < short.MinValue || signed > short.MaxValue,
                FlagOperation.IMul23_DWord => (longSigned = (int)this.a * (int)this.b) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Add_Byte => (signed = (sbyte)(byte)this.a + (sbyte)(byte)this.b) < sbyte.MinValue || signed > sbyte.MaxValue,
                FlagOperation.Add_Word => (signed = (short)(ushort)this.a + (short)(ushort)this.b) < short.MinValue || signed > short.MaxValue,
                FlagOperation.Add_DWord => (longSigned = (long)(int)this.a + (int)this.b) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Adc_Byte => (signed = (sbyte)(byte)this.a + (sbyte)(byte)this.b + (sbyte)(byte)this.c) < sbyte.MinValue || signed > sbyte.MaxValue,
                FlagOperation.Adc_Word => (signed = (short)(ushort)this.a + (short)(ushort)this.b + (short)(ushort)this.c) < short.MinValue || signed > short.MaxValue,
                FlagOperation.Adc_DWord => (longSigned = (long)(int)this.a + (int)this.b + (int)this.c) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Sub_Byte => (signed = (sbyte)(byte)this.a - (sbyte)(byte)this.b) < sbyte.MinValue || signed > sbyte.MaxValue,
                FlagOperation.Sub_Word => (signed = (short)(ushort)this.a - (short)(ushort)this.b) < short.MinValue || signed > short.MaxValue,
                FlagOperation.Sub_DWord => (longSigned = (long)(int)this.a - (int)this.b) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Sbb_Byte => (signed = (sbyte)(byte)this.a - (sbyte)(byte)this.b - (sbyte)(byte)this.c) < sbyte.MinValue || signed > sbyte.MaxValue,
                FlagOperation.Sbb_Word => (signed = (short)(ushort)this.a - (short)(ushort)this.b - (short)(ushort)this.c) < short.MinValue || signed > short.MaxValue,
                FlagOperation.Sbb_DWord => (longSigned = (long)(int)this.a - (int)this.b - (int)this.c) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Inc_Byte => (signed = (sbyte)(byte)this.a + 1) < sbyte.MinValue || signed > sbyte.MaxValue,
                FlagOperation.Inc_Word => (signed = (short)(ushort)this.a + 1) < short.MinValue || signed > short.MaxValue,
                FlagOperation.Inc_DWord => (longSigned = (long)(int)this.a + 1) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Dec_Byte => (signed = (sbyte)(byte)this.a - 1) < sbyte.MinValue || signed > sbyte.MaxValue,
                FlagOperation.Dec_Word => (signed = (short)(ushort)this.a - 1) < short.MinValue || signed > short.MaxValue,
                FlagOperation.Dec_DWord => (longSigned = (long)(int)this.a - 1) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Shl1_Byte => (this.a & 0xC0) == 0x80 || (this.a & 0xC0) == 0x40,
                FlagOperation.Shl1_Word => (this.a & 0xC000) == 0x8000 || (this.a & 0xC000) == 0x4000,
                FlagOperation.Shl1_DWord => (this.a & 0xC0000000) == 0x80000000 || (this.a & 0xC0000000) == 0x40000000,

                FlagOperation.Shr1_Byte => (this.a & 0x80) == 0x80,
                FlagOperation.Shr1_Word => (this.a & 0x8000) == 0x8000,
                FlagOperation.Shr1_DWord => (this.a & 0x80000000) == 0x80000000,

                FlagOperation.Rol1_Byte => (this.a & 0x81) == 0x80 || (this.a & 0x81) == 0x01,
                FlagOperation.Rol1_Word => (this.a & 0x8001) == 0x8000 || (this.a & 0x8001) == 0x0001,
                FlagOperation.Rol1_DWord => (this.a & 0x80000001) == 0x80000000 || (this.a & 0x80000001) == 0x00000001,

                FlagOperation.Shld_Word => (((this.a << 1) | (this.b >> 15)) ^ this.a) == 0x8000,
                FlagOperation.Shld_DWord => (((this.a << 1) | (this.b >> 31)) ^ this.a) == 0x80000000,

                _ => false
            };

            this.currentValue = value;
            return value;
        }
    }
}
