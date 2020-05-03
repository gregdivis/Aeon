using System.Runtime.CompilerServices;

namespace Aeon.Emulator
{
    public sealed partial class FlagState
    {
        private readonly SignZeroParityFlag signZeroParity = new SignZeroParityFlag();
        private readonly CarryFlag carry = new CarryFlag();
        private readonly OverflowFlag overflow = new OverflowFlag();

        public bool Carry
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.carry.Value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.carry.Value = value;
        }
        public bool Parity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.signZeroParity.Parity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.signZeroParity.Parity = value;
        }
        public bool Auxiliary { get; set; }
        public bool Zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.signZeroParity.Zero;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.signZeroParity.Zero = value;
        }
        public bool Sign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.signZeroParity.Sign;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.signZeroParity.Sign = value;
        }
        public bool Trap { get; set; }
        public bool InterruptEnable { get; set; } = true;
        public bool Direction { get; set; }
        public bool Overflow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.overflow.Value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.overflow.Value = value;
        }
        public int IOPrivilege { get; set; }
        public bool NestedTask { get; set; }
        public bool Identification { get; set; } = true;

        public EFlags Value
        {
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            get
            {
                var flags = EFlags.Reserved1;
                if (this.Carry)
                    flags |= EFlags.Carry;
                if (this.Parity)
                    flags |= EFlags.Parity;
                if (this.Auxiliary)
                    flags |= EFlags.Auxiliary;
                if (this.Zero)
                    flags |= EFlags.Zero;
                if (this.Sign)
                    flags |= EFlags.Sign;
                if (this.Trap)
                    flags |= EFlags.Trap;
                if (this.InterruptEnable)
                    flags |= EFlags.InterruptEnable;
                if (this.Direction)
                    flags |= EFlags.Direction;
                if (this.Overflow)
                    flags |= EFlags.Overflow;
                if (this.NestedTask)
                    flags |= EFlags.NestedTask;
                if (this.Identification)
                    flags |= EFlags.Identification;
                if ((this.IOPrivilege & 1) != 0)
                    flags |= EFlags.IOPrivilege1;
                if ((this.IOPrivilege & 2) != 0)
                    flags |= EFlags.IOPrivilege2;
                return flags;
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            set
            {
                this.Carry = value.HasFlag(EFlags.Carry);
                this.Parity = value.HasFlag(EFlags.Parity);
                this.Auxiliary = value.HasFlag(EFlags.Auxiliary);
                this.Zero = value.HasFlag(EFlags.Zero);
                this.Sign = value.HasFlag(EFlags.Sign);
                this.Trap = value.HasFlag(EFlags.Trap);
                this.InterruptEnable = value.HasFlag(EFlags.InterruptEnable);
                this.Direction = value.HasFlag(EFlags.Direction);
                this.Overflow = value.HasFlag(EFlags.Overflow);
                this.NestedTask = value.HasFlag(EFlags.NestedTask);
                this.Identification = value.HasFlag(EFlags.Identification);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Value_Byte(byte value) => this.signZeroParity.SetLazyByte(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Value_Word(ushort value) => this.signZeroParity.SetLazyWord(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Value_DWord(uint value) => this.signZeroParity.SetLazyDWord(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Mul(uint value)
        {
            this.carry.SetLazy(FlagOperation.Mul, value);
            this.overflow.SetLazy(FlagOperation.Mul, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_IMul(int value)
        {
            this.carry.SetLazy(FlagOperation.IMul, (uint)value);
            this.overflow.SetLazy(FlagOperation.IMul, (uint)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_IMul23_Word(short a, short b)
        {
            this.carry.SetLazy(FlagOperation.IMul23_Word, (uint)a, (uint)b);
            this.overflow.SetLazy(FlagOperation.IMul23_Word, (uint)a, (uint)b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_IMul23_DWord(int a, int b)
        {
            this.carry.SetLazy(FlagOperation.IMul23_DWord, (uint)a, (uint)b);
            this.overflow.SetLazy(FlagOperation.IMul23_DWord, (uint)a, (uint)b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Add_Byte(byte a, byte b, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Add_Byte, a, b);
            this.overflow.SetLazy(FlagOperation.Add_Byte, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Add_Word(ushort a, ushort b, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Add_Word, a, b);
            this.overflow.SetLazy(FlagOperation.Add_Word, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Add_DWord(uint a, uint b, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Add_DWord, a, b);
            this.overflow.SetLazy(FlagOperation.Add_DWord, a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Adc_Byte(byte a, byte b, uint c, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Adc_Byte, a, b, c);
            this.overflow.SetLazy(FlagOperation.Adc_Byte, a, b, c);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Adc_Word(ushort a, ushort b, uint c, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Adc_Word, a, b, c);
            this.overflow.SetLazy(FlagOperation.Adc_Word, a, b, c);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Adc_DWord(uint a, uint b, uint c, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Adc_DWord, a, b, c);
            this.overflow.SetLazy(FlagOperation.Adc_DWord, a, b, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sub_Byte(byte a, byte b, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Sub_Byte, a, b);
            this.overflow.SetLazy(FlagOperation.Sub_Byte, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sub_Word(ushort a, ushort b, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Sub_Word, a, b);
            this.overflow.SetLazy(FlagOperation.Sub_Word, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sub_DWord(uint a, uint b, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Sub_DWord, a, b);
            this.overflow.SetLazy(FlagOperation.Sub_DWord, a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sbb_Byte(byte a, byte b, uint c, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Sbb_Byte, a, b, c);
            this.overflow.SetLazy(FlagOperation.Sbb_Byte, a, b, c);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sbb_Word(ushort a, ushort b, uint c, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Sbb_Word, a, b, c);
            this.overflow.SetLazy(FlagOperation.Sbb_Word, a, b, c);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sbb_DWord(uint a, uint b, uint c, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Sbb_DWord, a, b, c);
            this.overflow.SetLazy(FlagOperation.Sbb_DWord, a, b, c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Inc_Byte(byte a, byte result)
        {
            this.Update_Value_Byte(result);
            this.overflow.SetLazy(FlagOperation.Inc_Byte, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Inc_Word(ushort a, ushort result)
        {
            this.Update_Value_Word(result);
            this.overflow.SetLazy(FlagOperation.Inc_Word, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Inc_DWord(uint a, uint result)
        {
            this.Update_Value_DWord(result);
            this.overflow.SetLazy(FlagOperation.Inc_DWord, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Dec_Byte(byte a, byte result)
        {
            this.Update_Value_Byte(result);
            this.overflow.SetLazy(FlagOperation.Dec_Byte, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Dec_Word(ushort a, ushort result)
        {
            this.Update_Value_Word(result);
            this.overflow.SetLazy(FlagOperation.Dec_Word, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Dec_DWord(uint a, uint result)
        {
            this.Update_Value_DWord(result);
            this.overflow.SetLazy(FlagOperation.Dec_DWord, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sar1_Byte(byte a, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Sar1, a);
            this.overflow.Value = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sar1_Word(ushort a, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Sar1, a);
            this.overflow.Value = false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sar1_DWord(uint a, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Sar1, a);
            this.overflow.Value = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sar_Byte(byte a, byte b, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Sar_Byte, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sar_Word(ushort a, ushort b, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Sar_Word, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Sar_DWord(uint a, uint b, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Sar_DWord, a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shl1_Byte(byte a, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Shl1_Byte, a);
            this.overflow.SetLazy(FlagOperation.Shl1_Byte, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shl1_Word(ushort a, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Shl1_Word, a);
            this.overflow.SetLazy(FlagOperation.Shl1_Word, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shl1_DWord(uint a, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Shl1_DWord, a);
            this.overflow.SetLazy(FlagOperation.Shl1_DWord, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shl_Byte(byte a, byte b, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Shl_Byte, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shl_Word(ushort a, ushort b, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Shl_Word, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shl_DWord(uint a, uint b, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Shl_DWord, a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shr1_Byte(byte a, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Shr1_Byte, a);
            this.overflow.SetLazy(FlagOperation.Shr1_Byte, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shr1_Word(ushort a, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Shr1_Word, a);
            this.overflow.SetLazy(FlagOperation.Shr1_Word, a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shr1_DWord(uint a, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Shr1_DWord, a);
            this.overflow.SetLazy(FlagOperation.Shr1_DWord, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shr_Byte(byte a, byte b, byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Shr, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shr_Word(ushort a, ushort b, ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Shr, a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shr_DWord(uint a, uint b, uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Shr, a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Rol1_Byte(byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Rol1_Byte, result);
            this.overflow.SetLazy(FlagOperation.Rol1_Byte, result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Rol1_Word(ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Rol1_Word, result);
            this.overflow.SetLazy(FlagOperation.Rol1_Word, result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Rol1_DWord(uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Rol1_DWord, result);
            this.overflow.SetLazy(FlagOperation.Rol1_DWord, result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Rol_Byte(byte result)
        {
            this.Update_Value_Byte(result);
            this.carry.SetLazy(FlagOperation.Rol, result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Rol_Word(ushort result)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Rol, result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Rol_DWord(uint result)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Rol, result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shld_Word(ushort result, uint dest, uint src, uint count)
        {
            this.Update_Value_Word(result);
            this.carry.SetLazy(FlagOperation.Shld_Word, dest, src, count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shld1_Word(ushort result, uint dest, uint src, uint count)
        {
            this.Update_Shld_Word(result, dest, src, count);
            this.overflow.SetLazy(FlagOperation.Shld_Word, dest, src);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shld_DWord(uint result, uint dest, uint src, uint count)
        {
            this.Update_Value_DWord(result);
            this.carry.SetLazy(FlagOperation.Shld_DWord, dest, src, count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update_Shld1_DWord(uint result, uint dest, uint src, uint count)
        {
            this.Update_Shld_DWord(result, dest, src, count);
            this.overflow.SetLazy(FlagOperation.Shld_DWord, dest, src);
        }
    }
}
