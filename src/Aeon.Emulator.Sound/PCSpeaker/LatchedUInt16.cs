using System;

namespace Aeon.Emulator.Sound.PCSpeaker
{
    /// <summary>
    /// Allows a 16-bit value to be read or written one byte at a time.
    /// </summary>
    public sealed class LatchedUInt16
    {
        #region Private Fields
        private ushort value;
        private bool wroteLow;
        private bool readLow;
        private byte latchedHighByte;
        private byte latchedLowByte;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the LatchedUInt16 class.
        /// </summary>
        public LatchedUInt16()
        {
        }
        /// <summary>
        /// Initializes a new instance of the LatchedUInt16 class.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public LatchedUInt16(ushort value)
        {
            this.value = value;
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Occurs when the value has changed.
        /// </summary>
        public event EventHandler ValueChanged;
        #endregion

        #region Operators
        /// <summary>
        /// Returns the current value.
        /// </summary>
        /// <param name="value">Current value.</param>
        /// <returns>The current value.</returns>
        public static implicit operator ushort(LatchedUInt16 value)
        {
            if(value == null)
                return 0;

            return value.value;
        }
        /// <summary>
        /// Returns a new LatchedUInt16 instance with an initial value.
        /// </summary>
        /// <param name="value">Initial value of the new LatchedUInt16 instance.</param>
        /// <returns>New LatchedUInt16 instance.</returns>
        public static implicit operator LatchedUInt16(ushort value)
        {
            return new LatchedUInt16(value);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the next byte of the value.
        /// </summary>
        /// <returns>The next byte of the value.</returns>
        public byte ReadByte()
        {
            if(this.readLow)
            {
                this.readLow = false;
                return this.latchedHighByte;
            }
            else
            {
                this.readLow = true;
                var value = this.value;
                this.latchedHighByte = (byte)(value >> 8);
                return (byte)value;
            }
        }
        /// <summary>
        /// Writes the next byte of the value.
        /// </summary>
        /// <param name="value">The next byte of the value.</param>
        public void WriteByte(byte value)
        {
            if(this.wroteLow)
            {
                this.wroteLow = false;
                this.value = (ushort)((value << 8) | this.latchedLowByte);
                OnValueChanged(EventArgs.Empty);
            }
            else
            {
                this.wroteLow = true;
                this.latchedLowByte = value;
            }
        }
        /// <summary>
        /// Sets the full 16-bit value.
        /// </summary>
        /// <param name="value">The full 16-bit value.</param>
        public void SetValue(ushort value)
        {
            this.value = value;
            OnValueChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>String representation of the value.</returns>
        public override string ToString()
        {
            return this.value.ToString();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="e">Unused EventArgs instance.</param>
        private void OnValueChanged(EventArgs e)
        {
            var handler = this.ValueChanged;
            if(handler != null)
                handler(this, e);
        }
        #endregion
    }
}
