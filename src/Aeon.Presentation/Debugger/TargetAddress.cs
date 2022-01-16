using Aeon.Emulator.DebugSupport;

namespace Aeon.Presentation.Debugger
{
    /// <summary>
    /// Describes the target address of a jump or call.
    /// </summary>
    public sealed class TargetAddress
    {
        /// <summary>
        /// Initializes a new instance of the TargetAddress class.
        /// </summary>
        /// <param name="address">The target address.</param>
        /// <param name="addressType">The type of the target address.</param>
        public TargetAddress(QualifiedAddress address, TargetAddressType addressType)
        {
            this.Address = address;
            this.AddressType = addressType;
        }

        /// <summary>
        /// Gets the target address.
        /// </summary>
        public QualifiedAddress Address { get; private set; }
        /// <summary>
        /// Gets the type of the target address.
        /// </summary>
        public TargetAddressType AddressType { get; private set; }
    }

    /// <summary>
    /// Specifies the type of a target address.
    /// </summary>
    public enum TargetAddressType
    {
        /// <summary>
        /// The address refers to a code segment.
        /// </summary>
        Code,
        /// <summary>
        /// The address refers to a data segment.
        /// </summary>
        Data
    }
}
