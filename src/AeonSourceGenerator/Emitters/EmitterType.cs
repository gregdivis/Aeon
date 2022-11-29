using System;
using System.Collections.Generic;
using System.Text;

namespace AeonSourceGenerator.Emitters
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
        IntPtr
    }
}
