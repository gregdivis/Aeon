using System.Runtime.CompilerServices;

namespace Aeon.Emulator;

partial class FlagState
{
    private struct CarryFlag
    {
        private Overrides overrides;
        private FlagOperation operation;
        private uint a;
        private uint b;
        private uint c;

        public CarryFlag(bool initialValue) : this() => this.overrides = new Overrides(initialValue);

        public bool Carry
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.overrides.Carry ?? this.CalculateCarryValue();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.overrides.Carry = value;
        }
        public bool Aux
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.overrides.Aux ?? this.CalculateAuxValue();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.overrides.Aux = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazy(FlagOperation operation, uint a)
        {
            this.operation = operation;
            this.a = a;
            this.overrides = default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazy(FlagOperation operation, uint a, uint b)
        {
            this.operation = operation;
            this.a = a;
            this.b = b;
            this.overrides = default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazy(FlagOperation operation, uint a, uint b, uint c)
        {
            this.operation = operation;
            this.a = a;
            this.b = b;
            this.c = c;
            this.overrides = default;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool CalculateCarryValue()
        {
            int signed;
            long longSigned;

            bool value = this.operation switch
            {
                FlagOperation.Mul => this.a != 0,

                FlagOperation.IMul => this.a != 0 && this.a != uint.MaxValue,

                FlagOperation.IMul23_Word => (signed = (short)(ushort)this.a * (short)(ushort)this.b) < short.MinValue || signed > short.MaxValue,
                FlagOperation.IMul23_DWord => (longSigned = (int)this.a * (int)this.b) < int.MinValue || longSigned > int.MaxValue,

                FlagOperation.Add_Byte => ((this.a + this.b) & 0x100u) != 0,
                FlagOperation.Add_Word => ((this.a + this.b) & 0x1_0000u) != 0,
                FlagOperation.Add_DWord => (((ulong)this.a + this.b) & 0x1_0000_0000u) != 0,

                FlagOperation.Adc_Byte => ((this.a + this.b + this.c) & 0x100u) != 0,
                FlagOperation.Adc_Word => ((this.a + this.b + this.c) & 0x1_0000u) != 0,
                FlagOperation.Adc_DWord => (((ulong)this.a + this.b + this.c) & 0x1_0000_0000u) != 0,

                FlagOperation.Sub_Byte => ((this.a - this.b) & 0x100u) != 0,
                FlagOperation.Sub_Word => ((this.a - this.b) & 0x1_0000u) != 0,
                FlagOperation.Sub_DWord => (((ulong)this.a - this.b) & 0x1_0000_0000u) != 0,

                FlagOperation.Sbb_Byte => ((this.a - this.b - this.c) & 0x100u) != 0,
                FlagOperation.Sbb_Word => ((this.a - this.b - this.c) & 0x1_0000u) != 0,
                FlagOperation.Sbb_DWord => (((ulong)this.a - this.b - this.c) & 0x1_0000_0000u) != 0,

                FlagOperation.Sar1 => (this.a & 1) == 1,
                FlagOperation.Sar_Byte => ((sbyte)this.a & (1 << ((int)this.b - 1))) != 0,
                FlagOperation.Sar_Word => ((short)this.a & (1 << ((int)this.b - 1))) != 0,
                FlagOperation.Sar_DWord => ((int)this.a & (1 << ((int)this.b - 1))) != 0,

                FlagOperation.Shl1_Byte => (this.a & 0x80) == 0x80,
                FlagOperation.Shl1_Word => (this.a & 0x8000) == 0x8000,
                FlagOperation.Shl1_DWord => (this.a & 0x80000000) == 0x80000000,

                FlagOperation.Shl_Byte => (this.a & (1 << (8 - (int)(this.b & 0x1F)))) != 0,
                FlagOperation.Shl_Word => (this.a & (1 << (16 - (int)(this.b & 0x1F)))) != 0,
                FlagOperation.Shl_DWord => (this.a & (1 << (32 - (int)(this.b & 0x1F)))) != 0,

                FlagOperation.Shr1_Byte => (this.a & 1) == 1,
                FlagOperation.Shr1_Word => (this.a & 1) == 1,
                FlagOperation.Shr1_DWord => (this.a & 1) == 1,
                FlagOperation.Shr => (this.a & (1 << ((int)(this.b & 0x1F) - 1))) != 0,

                FlagOperation.Rol1_Byte => (this.a & 1) == 1,
                FlagOperation.Rol1_Word => (this.a & 1) == 1,
                FlagOperation.Rol1_DWord => (this.a & 1) == 1,

                FlagOperation.Rol => (this.a & 1) == 1,

                FlagOperation.Shld_Word => (((this.a << (int)this.c) | (this.b >> (int)(16 - (this.c & 0xFu)))) & 0x1_0000u) == 0x1_0000u,
                FlagOperation.Shld_DWord => ((((ulong)this.a << (int)this.c) | (this.b >> (int)(32 - (this.c & 0xFu)))) & 0x1_0000_0000u) == 0x1_0000_0000u,

                _ => false
            };

            this.overrides.Carry = value;
            return value;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool CalculateAuxValue()
        {
            bool value = this.operation switch
            {
                FlagOperation.Add_Byte or FlagOperation.Add_Word or FlagOperation.Add_DWord => (((this.a & 0xF) + (this.b & 0xF)) & 0x10u) != 0,
                FlagOperation.Adc_Byte or FlagOperation.Adc_Word or FlagOperation.Adc_DWord => (((this.a & 0xF) + (this.b & 0xF) + (this.c & 0xF)) & 0x10u) != 0,
                FlagOperation.Sub_Byte or FlagOperation.Sub_Word or FlagOperation.Sub_DWord => (((this.a & 0xF) - (this.b & 0xF)) & 0x10u) != 0,
                FlagOperation.Sbb_Byte or FlagOperation.Sbb_Word or FlagOperation.Sbb_DWord => (((this.a & 0xF) - (this.b & 0xF) - (this.c & 0xF)) & 0x10u) != 0,
                _ => false
            };

            this.overrides.Aux = value;
            return value;
        }

        private struct Overrides
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Overrides(bool initialValue)
            {
                this.Carry = initialValue;
                this.Aux = initialValue;
            }

            public bool? Carry;
            public bool? Aux;
        }
    }
}
