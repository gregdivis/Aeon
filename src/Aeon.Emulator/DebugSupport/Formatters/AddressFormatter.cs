﻿using System.Collections.Generic;

namespace Aeon.Emulator.DebugSupport
{
    internal static class AddressFormatter
    {
        /// <summary>
        /// Specifies the prefix override flags.
        /// </summary>
        public const PrefixState SegmentOverrideMask = PrefixState.CS | PrefixState.DS | PrefixState.ES | PrefixState.FS | PrefixState.GS | PrefixState.SS;

        private static readonly SortedList<CodeMemoryBase, string> effectiveAddresses = new();
        private static readonly SortedList<CodeSibRegister, string> sibRegisters = new();

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
            var prefix = GetSizePrefix(memoryOperand.OperandSize);

            if (memoryOperand.EffectiveAddress == CodeMemoryBase.SIB)
                return prefix + FormatSib(memoryOperand);

            effectiveAddresses.TryGetValue(memoryOperand.EffectiveAddress, out var register);

            var segmentOverride = FormatSegment(prefixes);

            if (register == null)
            {
                return $"{prefix} {segmentOverride}[{(int)memoryOperand.ImmediateValue:X}]";
            }
            else if (memoryOperand.ImmediateValue == 0)
            {
                return $"{prefix} {segmentOverride}[{register}]";
            }
            else
            {
                int value = (int)memoryOperand.ImmediateValue;
                char sign = value < 0 ? '-' : '+';
                if (value < 0)
                    value = -value;

                return $"{prefix} {segmentOverride}[{register}{sign}{value:X}]";
            }
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
            return size switch
            {
                CodeOperandSize.Byte => "byte ptr",
                CodeOperandSize.Word => "word ptr",
                CodeOperandSize.DoubleWord => "dword ptr",
                _ => string.Empty
            };
        }
        private static string FormatSegment(PrefixState prefixes)
        {
            return (prefixes & SegmentOverrideMask) switch
            {
                PrefixState.CS => "cs:",
                PrefixState.DS => "ds:",
                PrefixState.ES => "es:",
                PrefixState.FS => "fs:",
                PrefixState.GS => "gs:",
                PrefixState.SS => "ss:",
                _ => string.Empty
            };
        }
    }
}
