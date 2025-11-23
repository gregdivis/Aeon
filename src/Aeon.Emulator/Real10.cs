using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator;

/// <summary>
/// Represents an 80-bit extended-precision floating-point number.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
public readonly struct Real10
{
    /// <summary>
    /// Represents positive infinity.
    /// </summary>
    public static readonly Real10 PositiveInfinity = new(0, 0x7FFF);
    /// <summary>
    /// Represents negative infinity.
    /// </summary>
    public static readonly Real10 NegativeInfinity = new(0, 0xFFFF);
    /// <summary>
    /// Represents a value that is not a number (NaN).
    /// </summary>
    public static readonly Real10 NaN = new(0x7FFFFFFFFFFFFFFF, 0xFFFF);

    private readonly ulong mantissa;
    private readonly ushort exponentAndSign;

    /// <summary>
    /// Initializes a new instance of the Real10 struct.
    /// </summary>
    /// <param name="mantissa">Mantissa binary value.</param>
    /// <param name="exponentAndSign">Exponent binary value with sign bit.</param>
    private Real10(ulong mantissa, ushort exponentAndSign)
    {
        this.mantissa = mantissa | 0x8000000000000000u;
        this.exponentAndSign = exponentAndSign;
    }

    public static explicit operator double(Real10 value) => value.ToDouble();
    public static implicit operator Real10(double value) => FromDouble(value);

    public readonly override string ToString() => ((double)this).ToString();

    private readonly double ToDouble()
    {
        // The mantissa is the low 63 bits.
        ulong mantissa = this.mantissa & 0x7FFFFFFFFFFFFFFFu;

        // The exponent is the next 15 bits.
        int exponent = this.exponentAndSign & 0x7FFF;

        // The sign is the highest bit.
        byte sign = (byte)(this.exponentAndSign & 0x80u);

        // Drop the lowest 11 bits from the mantissa.
        mantissa >>= 11;

        if (exponent == 0)
            return 0.0;

        if (exponent == 0x7FFF) //+infinity, -infinity or nan
        {
            if (mantissa != 0)
                return double.NaN;
            if (sign == 0)
                return double.PositiveInfinity;
            else
                return double.NegativeInfinity;
        }

        exponent -= (0x3FFF - 0x3FF);

        if (exponent >= 0x7FF)
        {
            return sign == 0 ? double.PositiveInfinity : double.NegativeInfinity;
        }
        else if (exponent < -51)
        {
            return 0.0;
        }
        else if (exponent < 0)
        {
            mantissa |= 0x1000000000000000u;
            mantissa >>= 1 - exponent;
            exponent = 0;
        }

        ulong doubleMantissa = mantissa & 0x000FFFFFFFFFFFFFu;
        doubleMantissa |= ((ulong)(uint)exponent) << 52;
        doubleMantissa |= (ulong)sign << 56;

        return Unsafe.BitCast<ulong, double>(doubleMantissa);
    }

    private static Real10 FromDouble(double value)
    {
        if (double.IsPositiveInfinity(value))
            return PositiveInfinity;
        else if (double.IsNegativeInfinity(value))
            return NegativeInfinity;
        else if (double.IsNaN(value))
            return NaN;

        ulong doubleInt;
        unsafe
        {
            doubleInt = *(ulong*)&value;
        }

        ulong mantissa = (doubleInt & 0x000FFFFFFFFFFFFFu) << 11;

        uint exponent = (uint)(doubleInt >> 52) & 0x7FFu;
        exponent += (0x3FFF - 0x3FF);

        if ((doubleInt & 0x8000000000000000u) != 0)
            exponent |= 0x8000;

        return new Real10(mantissa, (ushort)exponent);
    }
}
