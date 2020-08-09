using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator
{
    /// <summary>
    /// Contains the registers of an x87 floating-point unit.
    /// </summary>
    public sealed class FPU
    {
        /// <summary>
        /// Stores the ST register contents.
        /// </summary>
        private readonly unsafe double* reg;
        /// <summary>
        /// Stores values indicating whether registers are in use.
        /// </summary>
        private readonly unsafe byte* isUsed;
        /// <summary>
        /// The FPU top pointer.
        /// </summary>
        private uint top;
        private readonly UnsafeBuffer<double> regBuffer = new UnsafeBuffer<double>(8);
        private readonly UnsafeBuffer<byte> isUsedBuffer = new UnsafeBuffer<byte>(8);

        /// <summary>
        /// Initializes a new instance of the FPU class.
        /// </summary>
        public FPU()
        {
            unsafe
            {
                this.reg = this.regBuffer.ToPointer();
                this.isUsed = this.isUsedBuffer.ToPointer();
            }

            this.Reset();
        }

        /// <summary>
        /// Gets the index of the FPU top register.
        /// </summary>
        public int Top => (int)this.top;
        /// <summary>
        /// Gets an enumeration of the contents of the ST registers.
        /// </summary>
        public IEnumerable<double?> ST
        {
            get
            {
                for (uint i = 0; i < 8u; i++)
                {
                    uint index = (top + i) & 0x7u;
                    if (getIsUsed(index))
                        yield return getReg(index);
                    else
                        yield return null;
                }

                bool getIsUsed(uint j)
                {
                    unsafe
                    {
                        return this.isUsed[j] != 0;
                    }
                }

                double getReg(uint j)
                {
                    unsafe
                    {
                        return this.reg[j];
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets flags on the FPU status register.
        /// </summary>
        public FPUStatus StatusFlags { get; set; }
        /// <summary>
        /// Gets or sets the value of the FPU status word.
        /// </summary>
        public ushort StatusWord
        {
            get
            {
                uint value = (uint)this.StatusFlags | (top << 11);
                return (ushort)value;
            }
            set
            {
                this.StatusFlags = (FPUStatus)(value & 0xC7FF);
                this.top = (uint)((value >> 11) & 7);
            }
        }
        /// <summary>
        /// Gets masked FPU exceptions.
        /// </summary>
        public ExcepetionMask MaskedExceptions { get; private set; }
        /// <summary>
        /// Gets the rounding mode of the FPU.
        /// </summary>
        public RoundingControl RoundingMode { get; set; }
        /// <summary>
        /// Gets the precision mode of the FPU.
        /// </summary>
        public PrecisionControl PrecisionMode { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the interrupt enable mask is set.
        /// </summary>
        public bool InterruptEnableMask { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the infinity control bit is set.
        /// </summary>
        public bool InfinityControl { get; private set; }
        /// <summary>
        /// Gets or sets the value of the FPU control word.
        /// </summary>
        public ushort ControlWord
        {
            get
            {
                uint value = (uint)this.MaskedExceptions | ((uint)this.PrecisionMode << 8) | ((uint)this.RoundingMode << 10);
                if (this.InterruptEnableMask)
                    value |= (1u << 7);
                if (this.InfinityControl)
                    value |= (1u << 12);

                return (ushort)value;
            }
            set
            {
                this.MaskedExceptions = (ExcepetionMask)(value & 0x3Fu);
                this.PrecisionMode = (PrecisionControl)((value >> 8) & 0x3u);
                this.RoundingMode = (RoundingControl)((value >> 10) & 0x3u);
                this.InterruptEnableMask = (value & (1u << 7)) != 0;
                this.InfinityControl = (value & (1u << 12)) != 0;
            }
        }
        /// <summary>
        /// Gets or sets the value of the FPU tag word.
        /// </summary>
        public ushort TagWord
        {
            get
            {
                unsafe
                {
                    uint tag = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        uint currentValue = 3;
                        if (this.isUsed[i] != 0)
                        {
                            if (this.reg[i] == 0)
                                currentValue = 1;
                            else if (double.IsNaN(this.reg[i]) || double.IsInfinity(this.reg[i]))
                                currentValue = 2;
                            else
                                currentValue = 0;
                        }

                        tag |= currentValue << (i * 2);
                    }

                    return (ushort)tag;
                }
            }
            set
            {
                unsafe
                {
                    for (int i = 0; i < 8; i++)
                    {
                        uint currentValue = (uint)(value >> (i * 2)) & 3;
                        this.isUsed[i] = (currentValue != 3) ? (byte)1 : (byte)0;
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the value of the ST0 register.
        /// </summary>
        public double ST0
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetRegisterValue(0);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRegisterValue(0, value);
        }

        public ref double ST0_Ref
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe
                {
                    return ref this.reg[this.top & 0x7];
                }
            }
        }

        /// <summary>
        /// Returns the FPU to its initial state.
        /// </summary>
        public void Reset()
        {
            unsafe
            {
                var regVector = MemoryMarshal.Cast<double, Vector<double>>(new Span<double>(this.reg, 8));
                for (int i = 0; i < regVector.Length; i++)
                    regVector[i] = default;

                *(ulong*)this.isUsed = 0;
            }

            this.StatusFlags = FPUStatus.Clear;
            this.ControlWord = 0x3BF;
        }
        /// <summary>
        /// Pushes a value onto the next ST register.
        /// </summary>
        /// <param name="value">Value to push.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Push(double value)
        {
            top = (top - 1u) & 0x7u;

            unsafe
            {
                if (this.isUsed[top] == 0)
                {
                    this.reg[top] = value;
                    this.isUsed[top] = 1;
                }
                else
                {
                    // Invalid operation
                }
            }
        }
        /// <summary>
        /// Pops the value at ST0.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Pop()
        {
            unsafe
            {
                if (this.isUsed[top] != 0)
                {
                    isUsed[top] = 0;
                    top = (top + 1u) & 0x7u;
                }
                else
                {
                    // Invalid operation
                }
            }
        }
        /// <summary>
        /// Returns the value of an ST register.
        /// </summary>
        /// <param name="st">Register ST index.</param>
        /// <returns>Value of the specified register.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetRegisterValue(int st)
        {
            uint i = (this.top + (uint)st) & 0x7u;
            unsafe
            {
                return this.reg[i];
            }
        }
        /// <summary>
        /// Returns a pointer to an ST register.
        /// </summary>
        /// <param name="st">Register ST index.</param>
        /// <returns>Value of the specified register.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref double GetRegisterRef(uint st)
        {
            unsafe
            {
                return ref this.reg[(this.top + st) & 0x7u];
            }
        }
        /// <summary>
        /// Writes a value to an ST register.
        /// </summary>
        /// <param name="st">Register ST index.</param>
        /// <param name="value">Value to write to the register.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRegisterValue(int st, double value)
        {
            uint i = (this.top + (uint)st) & 0x7u;
            unsafe
            {
                this.reg[i] = value;
            }
        }
        /// <summary>
        /// Marks a register as unused.
        /// </summary>
        /// <param name="st">Register ST index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FreeRegister(int st)
        {
            uint i = (this.top + (uint)st) & 0x7u;
            unsafe
            {
                this.isUsed[i] = 0;
            }
        }
        /// <summary>
        /// Returns a rounded value based on the rounding mode set in the FPU control register.
        /// </summary>
        /// <param name="value">Value to round.</param>
        /// <returns>Rounded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public double Round(double value)
        {
            var mode = this.RoundingMode;
            if (mode == RoundingControl.Truncate)
                return Math.Truncate(value);
            else if (mode == RoundingControl.Nearest)
                return Math.Round(value);
            else if (mode == RoundingControl.Down)
                return Math.Floor(value);
            else
                return Math.Ceiling(value);
        }

        /// <summary>
        /// Returns a value indicating whether a register is used.
        /// </summary>
        /// <param name="st">Register ST index.</param>
        /// <returns>Value indicating whether the register is used.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRegisterUsed(int st)
        {
            uint i = (this.top + (uint)st) & 0x7u;
            unsafe
            {
                return this.isUsed[i] != 0;
            }
        }

        /// <summary>
        /// Specifies floating-point exceptions.
        /// </summary>
        [Flags]
        public enum ExcepetionMask
        {
            /// <summary>
            /// No exceptions.
            /// </summary>
            Clear = 0,
            /// <summary>
            /// The floating-point invalid operation exception.
            /// </summary>
            InvalidOperation = (1 << 0),
            /// <summary>
            /// The operand denormalized exception.
            /// </summary>
            Denormalized = (1 << 1),
            /// <summary>
            /// The floating-point divide by zero exception.
            /// </summary>
            ZeroDivide = (1 << 2),
            /// <summary>
            /// The floating-point overflow exception.
            /// </summary>
            Overflow = (1 << 3),
            /// <summary>
            /// The floating-point underflow exception.
            /// </summary>
            Underflow = (1 << 4),
            /// <summary>
            /// The floating-point precision exception.
            /// </summary>
            Precision = (1 << 5)
        }

        /// <summary>
        /// Specifies the rounding mode used by the FPU.
        /// </summary>
        public enum RoundingControl
        {
            /// <summary>
            /// Round to nearest integer.
            /// </summary>
            Nearest,
            /// <summary>
            /// Round towards -infinity.
            /// </summary>
            Down,
            /// <summary>
            /// Round towards +infinity.
            /// </summary>
            Up,
            /// <summary>
            /// Round towards zero.
            /// </summary>
            Truncate
        }

        /// <summary>
        /// Specifies the precision of the FPU.
        /// </summary>
        public enum PrecisionControl
        {
            /// <summary>
            /// 32-bit REAL4 floating-point values.
            /// </summary>
            Real4,
            /// <summary>
            /// This value is invalid.
            /// </summary>
            Invalid,
            /// <summary>
            /// 64-bit REAL8 floating-point values.
            /// </summary>
            Real8,
            /// <summary>
            /// 80-bit REAL10 floating-point values.
            /// </summary>
            Real10
        }
    }
}
