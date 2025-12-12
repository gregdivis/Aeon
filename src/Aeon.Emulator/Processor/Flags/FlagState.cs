using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator;

public sealed partial class FlagState
{
    private SignZeroParityFlag signZeroParity = new(false);
    private CarryFlag carry = new(false);
    private OverflowFlag overflow = new(false);

    public bool Carry
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.carry.Carry;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.carry.Carry = value;
    }
    public bool Parity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.signZeroParity.Parity;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.signZeroParity.Parity = value;
    }
    public bool Auxiliary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.carry.Aux;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this.carry.Aux = value;
    }
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
    public bool Virtual8086Mode { get; set; }

    public EFlags Value
    {
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
            if (this.Virtual8086Mode)
                flags |= EFlags.Virtual8086Mode;
            return flags;
        }
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

            this.Virtual8086Mode = value.HasFlag(EFlags.Virtual8086Mode);

            this.IOPrivilege = value.HasFlag(EFlags.IOPrivilege1) && value.HasFlag(EFlags.IOPrivilege2) ? 3
                : value.HasFlag(EFlags.IOPrivilege2) && !value.HasFlag(EFlags.IOPrivilege1) ? 2
                : value.HasFlag(EFlags.IOPrivilege1) && !value.HasFlag(EFlags.IOPrivilege2) ? 1
                : 0;
        }
    }

    public void Clear(EFlags mask)
    {
        if (mask.HasFlag(EFlags.Carry))
            this.Carry = false;
        if (mask.HasFlag(EFlags.Parity))
            this.Parity = false;
        if (mask.HasFlag(EFlags.Sign))
            this.Sign = false;
        if (mask.HasFlag(EFlags.Trap))
            this.Trap = false;
        if (mask.HasFlag(EFlags.Virtual8086Mode))
            this.Virtual8086Mode = false;
        if (mask.HasFlag(EFlags.Zero))
            this.Zero = false;
        if (mask.HasFlag(EFlags.Direction))
            this.Direction = false;
        if (mask.HasFlag(EFlags.Identification))
            this.Identification = false;
        if (mask.HasFlag(EFlags.InterruptEnable))
            this.InterruptEnable = false;
        if (mask.HasFlag(EFlags.IOPrivilege1) && mask.HasFlag(EFlags.IOPrivilege2))
            this.IOPrivilege = 0;
        if (mask.HasFlag(EFlags.NestedTask))
            this.NestedTask = false;
        if (mask.HasFlag(EFlags.Overflow))
            this.Overflow = false;
    }
    public void SetWithMask(EFlags value, EFlags mask)
    {
        if (mask.HasFlag(EFlags.Carry))
            this.Carry = value.HasFlag(EFlags.Carry);
        if (mask.HasFlag(EFlags.Parity))
            this.Parity = value.HasFlag(EFlags.Parity);
        if (mask.HasFlag(EFlags.Sign))
            this.Sign = value.HasFlag(EFlags.Sign);
        if (mask.HasFlag(EFlags.Trap))
            this.Trap = value.HasFlag(EFlags.Trap);
        if (mask.HasFlag(EFlags.Virtual8086Mode))
            this.Virtual8086Mode = value.HasFlag(EFlags.Virtual8086Mode);
        if (mask.HasFlag(EFlags.Zero))
            this.Zero = value.HasFlag(EFlags.Zero);
        if (mask.HasFlag(EFlags.Direction))
            this.Direction = value.HasFlag(EFlags.Direction);
        if (mask.HasFlag(EFlags.Identification))
            this.Identification = value.HasFlag(EFlags.Identification);
        if (mask.HasFlag(EFlags.InterruptEnable))
            this.InterruptEnable = value.HasFlag(EFlags.InterruptEnable);
        if (mask.HasFlag(EFlags.NestedTask))
            this.NestedTask = value.HasFlag(EFlags.NestedTask);
        if (mask.HasFlag(EFlags.Overflow))
            this.Overflow = value.HasFlag(EFlags.Overflow);

        this.IOPrivilege = (int)Intrinsics.ExtractBits((uint)value, 12, 2, (uint)(EFlags.IOPrivilege1 | EFlags.IOPrivilege2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Value<TValue>(TValue value) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        if (Unsafe.SizeOf<TValue>() == 1)
            this.Update_Value_Byte(byte.CreateTruncating(value));
        else if (Unsafe.SizeOf<TValue>() == 2)
            this.Update_Value_Word(ushort.CreateTruncating(value));
        else
            this.Update_Value_DWord(uint.CreateTruncating(value));
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
    public void Update_Add<TValue>(TValue a, TValue b, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Add_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Add_Word
            : FlagOperation.Add_DWord;
        this.carry.SetLazy(op, a, b);
        this.overflow.SetLazy(op, a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Adc<TValue>(TValue a, TValue b, TValue c, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Adc_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Adc_Word
            : FlagOperation.Adc_DWord;
        this.carry.SetLazy(op, a, b, c);
        this.overflow.SetLazy(op, a, b, c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Sub<TValue>(TValue a, TValue b, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Sub_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Sub_Word
            : FlagOperation.Sub_DWord;
        this.carry.SetLazy(op, a, b);
        this.overflow.SetLazy(op, a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Sbb<TValue>(TValue a, TValue b, TValue c, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Sbb_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Sbb_Word
            : FlagOperation.Sbb_DWord;
        this.carry.SetLazy(op, a, b, c);
        this.overflow.SetLazy(op, a, b, c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Inc<TValue>(TValue a, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.overflow.SetLazy(
            Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Inc_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Inc_Word
            : FlagOperation.Inc_DWord,
            a
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Dec<TValue>(TValue a, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.overflow.SetLazy(
            Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Dec_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Dec_Word
            : FlagOperation.Dec_DWord,
            a
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Sar1<TValue>(TValue a, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.carry.SetLazy(FlagOperation.Sar1, a);
        this.overflow.Value = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Sar<TValue>(TValue a, TValue b, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.carry.SetLazy(
            Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Sar_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Sar_Word
            : FlagOperation.Sar_DWord
            , a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Shl1<TValue>(TValue a, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Shl1_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Shl1_Word
            : FlagOperation.Shl1_DWord;

        this.carry.SetLazy(op, a);
        this.overflow.SetLazy(op, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Shl<TValue>(TValue a, TValue b, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.carry.SetLazy(Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Shl_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Shl_Word
            : FlagOperation.Shl_DWord, a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Shr1<TValue>(TValue a, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Shr1_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Shr1_Word
            : FlagOperation.Shr1_DWord;
        this.carry.SetLazy(op, a);
        this.overflow.SetLazy(op, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Shr<TValue>(TValue a, TValue b, TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.carry.SetLazy(FlagOperation.Shr, a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Rol1<TValue>(TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Rol1_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Rol1_Word
            : FlagOperation.Rol1_DWord;
        this.carry.SetLazy(op, result);
        this.overflow.SetLazy(op, result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Rol<TValue>(TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        this.carry.SetLazy(FlagOperation.Rol, result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Ror1<TValue>(TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Ror_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Ror_Word
            : FlagOperation.Ror_DWord;
        this.carry.SetLazy(op, result);
        this.overflow.SetLazy(op, result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update_Ror<TValue>(TValue result) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        this.Update_Value(result);
        var op = Unsafe.SizeOf<TValue>() == 1 ? FlagOperation.Ror_Byte
            : Unsafe.SizeOf<TValue>() == 2 ? FlagOperation.Ror_Word
            : FlagOperation.Ror_DWord;
        this.carry.SetLazy(op, result);
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
