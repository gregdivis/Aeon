namespace Aeon.SourceGenerator.Emitters
{
    internal readonly struct EmitterType
    {
        public EmitterType(EmitterTypeCode typeCode, bool isPointer = false)
        {
            this.TypeCode = typeCode;
            this.IsPointer = isPointer;
        }

        public EmitterTypeCode TypeCode { get; }
        public bool IsPointer { get; }
    }

    internal enum EmitterTypeCode
    {
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double,
        Real10,
        SegmentIndex,
        IntPtr,
        TypeParameter
    }
}
