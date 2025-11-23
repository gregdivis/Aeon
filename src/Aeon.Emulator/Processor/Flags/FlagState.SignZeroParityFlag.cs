using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator;

partial class FlagState
{
    private struct SignZeroParityFlag
    {
        private Overrides overrides;
        private uint result;

        public SignZeroParityFlag(bool initialValue)
        {
            this.overrides = new Overrides(initialValue);
            this.result = default;
        }

        public bool Sign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.overrides.Sign ?? this.CalculateSign();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.overrides.Sign = value;
        }
        public bool Zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.overrides.Zero ?? this.CalculateZero();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.overrides.Zero = value;
        }
        public bool Parity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.overrides.Parity ?? this.CalculateParity();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.overrides.Parity = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazyByte(byte result)
        {
            this.overrides = default;
            this.result = (uint)(sbyte)result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazyWord(ushort result)
        {
            this.overrides = default;
            this.result = (uint)(short)result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLazyDWord(uint result)
        {
            this.overrides = default;
            this.result = result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool CalculateSign()
        {
            bool value = (this.result & (1 << 31)) != 0;
            this.overrides.Sign = value;
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool CalculateZero()
        {
            bool value = this.result == 0;
            this.overrides.Zero = value;
            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool CalculateParity()
        {
            bool value = (BitOperations.PopCount(this.result & 0xFFu) & 1) == 0;
            this.overrides.Parity = value;
            return value;
        }

        private struct Overrides
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Overrides(bool initialValue)
            {
                this.Sign = initialValue;
                this.Zero = initialValue;
                this.Parity = initialValue;
            }

            public bool? Sign;
            public bool? Zero;
            public bool? Parity;
        }
    }
}
