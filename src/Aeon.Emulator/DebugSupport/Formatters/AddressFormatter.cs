using System.Collections.Generic;

namespace Aeon.Emulator.DebugSupport
{
    internal static class AddressFormatter
    {
        /// <summary>
        /// Specifies the prefix override flags.
        /// </summary>
        public const PrefixState SegmentOverrideMask = PrefixState.CS | PrefixState.DS | PrefixState.ES | PrefixState.FS | PrefixState.GS | PrefixState.SS;

        private static readonly SortedList<CodeMemoryBase, string> effectiveAddresses = new SortedList<CodeMemoryBase, string>();
        private static readonly SortedList<CodeSibRegister, string> sibRegisters = new SortedList<CodeSibRegister, string>();

        static AddressFormatter()
        {
            effectiveAddresses.Add(CodeMemoryBase.BP, "bp");
            effectiveAddresses.Add(CodeMemoryBase.BP_plus_DI, "bp+di");
            effectiveAddresses.Add(CodeMemoryBase.BP_plus_SI, "bp+si");
            effectiveAddresses.Add(CodeMemoryBase.BX, "bx");
            effectiveAddresses.Add(CodeMemoryBase.BX_plus_DI, "bx+di");
            effectiveAddresses.Add(CodeMemoryBase.BX_plus_SI, "bx+si");
            effectiveAddresses.Add(CodeMemoryBase.DI, "di");
            effectiveAddresses.Add(CodeMemoryBase.EAX, "eax");
            effectiveAddresses.Add(CodeMemoryBase.EBX, "ebx");
            effectiveAddresses.Add(CodeMemoryBase.EBP, "ebp");
            effectiveAddresses.Add(CodeMemoryBase.ECX, "ecx");
            effectiveAddresses.Add(CodeMemoryBase.EDI, "edi");
            effectiveAddresses.Add(CodeMemoryBase.EDX, "edx");
            effectiveAddresses.Add(CodeMemoryBase.ESI, "esi");
            effectiveAddresses.Add(CodeMemoryBase.SI, "si");

            sibRegisters.Add(CodeSibRegister.EAX, "eax");
            sibRegisters.Add(CodeSibRegister.EBP, "ebp");
            sibRegisters.Add(CodeSibRegister.EBX, "ebx");
            sibRegisters.Add(CodeSibRegister.ECX, "ecx");
            sibRegisters.Add(CodeSibRegister.EDI, "edi");
            sibRegisters.Add(CodeSibRegister.EDX, "edx");
            sibRegisters.Add(CodeSibRegister.ESI, "esi");
            sibRegisters.Add(CodeSibRegister.ESP, "esp");
        }

        /// <summary>
        /// Returns a string representation of a memory address.
        /// </summary>
        /// <param name="memoryOperand">Memory address operand to be converted to a string.</param>
        /// <param name="prefixes">Current instruction prefixes in effect.</param>
        /// <returns>String representation of the memory address.</returns>
        public static string Format(CodeOperand memoryOperand, PrefixState prefixes)
        {
            string prefix = GetSizePrefix(memoryOperand.OperandSize);

            if (memoryOperand.EffectiveAddress == CodeMemoryBase.SIB)
                return prefix + FormatSib(memoryOperand);

            string register;
            effectiveAddresses.TryGetValue(memoryOperand.EffectiveAddress, out register);

            string segmentOverride = FormatSegment(prefixes);

            if (register == null)
                return $"{prefix} {segmentOverride}[{memoryOperand.ImmediateValue:X4}]";
            else if (memoryOperand.ImmediateValue == 0)
                return $"{prefix} {segmentOverride}[{register}]";
            else
                return $"{prefix} {segmentOverride}[{register}+{memoryOperand.ImmediateValue:X4}]";
        }

        private static string FormatSib(CodeOperand sibOperand)
        {
            sibRegisters.TryGetValue(sibOperand.Base, out var baseRegister);
            var scaleIndex = FormatScaleIndex(sibOperand.Scale, sibOperand.Index);

            if (baseRegister == null)
            {
                return $"[{sibOperand.ImmediateValue:X8}]{scaleIndex}";
            }
            else
            {
                if (sibOperand.ImmediateValue == 0)
                    return $"[{baseRegister}]{scaleIndex}";
                else
                    return $"[{baseRegister}+{sibOperand.ImmediateValue:X8}]{scaleIndex}";
            }
        }
        private static string FormatScaleIndex(int scale, CodeSibRegister index)
        {
            sibRegisters.TryGetValue(index, out var indexRegister);

            if (indexRegister == null)
                return string.Empty;

            if (scale == 1)
                return $"[{indexRegister}]";
            else
                return $"[{indexRegister}*{scale}]";
        }
        private static string GetSizePrefix(CodeOperandSize size)
        {
            switch (size)
            {
                case CodeOperandSize.Byte:
                    return "byte ptr";

                case CodeOperandSize.Word:
                    return "word ptr";

                case CodeOperandSize.DoubleWord:
                    return "dword ptr";

                default:
                    return string.Empty;
            }
        }
        private static string FormatSegment(PrefixState prefixes)
        {
            switch (prefixes & SegmentOverrideMask)
            {
                case PrefixState.CS:
                    return "cs:";

                case PrefixState.DS:
                    return "ds:";

                case PrefixState.ES:
                    return "es:";

                case PrefixState.FS:
                    return "fs:";

                case PrefixState.GS:
                    return "gs:";

                case PrefixState.SS:
                    return "ss:";

                default:
                    return string.Empty;
            }
        }
    }
}
