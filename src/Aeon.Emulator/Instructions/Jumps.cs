using System;
using System.Runtime.CompilerServices;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Instructions
{
    internal static class Jmp
    {
        [Opcode("EB irelb|E9 irelw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RelativeNearJump(Processor p, short offset)
        {
            p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(RelativeNearJump), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RelativeNearJump(Processor p, int offset)
        {
            p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("FF/4 jmprmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AbsoluteNearJump(Processor p, ushort ip)
        {
            p.EIP = ip;
        }
        [Alternate(nameof(AbsoluteNearJump), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AbsoluteNearJump32(Processor p, uint ip)
        {
            p.EIP = ip;
        }

        [Opcode("EA iptr|FF/5 mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AbsoluteFarJump(VirtualMachine vm, uint address)
        {
            ushort segment = (ushort)(address >> 16);
            if (!vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable) | vm.Processor.Flags.Virtual8086Mode)
            {
                vm.WriteSegmentRegister(SegmentIndex.CS, segment);
                vm.Processor.EIP = (ushort)address;
            }
            else
            {
                ProtectedModeFarJump(vm, segment, (ushort)address, false);
            }
        }
        [Alternate(nameof(AbsoluteFarJump), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void AbsoluteFarJump32(VirtualMachine vm, ulong address)
        {
            var segment = (ushort)(address >> 32);
            if (!vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable) | vm.Processor.Flags.Virtual8086Mode)
            {
                vm.WriteSegmentRegister(SegmentIndex.CS, segment);
                vm.Processor.EIP = (uint)address;
            }
            else
            {
                ProtectedModeFarJump(vm, segment, (uint)address, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ProtectedModeFarJump(VirtualMachine vm, ushort selector, uint offset, bool is32Bit)
        {
            var descriptor = vm.PhysicalMemory.GetDescriptor(selector);
            var type = descriptor.DescriptorType;

            if (type == DescriptorType.Segment)
            {
                vm.WriteSegmentRegister(SegmentIndex.CS, selector);
                vm.Processor.EIP = offset;
            }
            else if (type == DescriptorType.TaskSegmentSelector)
            {
                vm.TaskSwitch32(selector, true, null);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    internal static class ConditionalJumps
    {
        [Opcode("70 irelb|0F80 irelw", Name = "jo", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpOverflow(Processor p, short offset)
        {
            if (p.Flags.Overflow)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpOverflow), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpOverflow32(Processor p, int offset)
        {
            if (p.Flags.Overflow)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("71 irelb|0F81 irelw", Name = "jno", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotOverflow(Processor p, short offset)
        {
            if (!p.Flags.Overflow)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpNotOverflow), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotOverflow32(Processor p, int offset)
        {
            if (!p.Flags.Overflow)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("72 irelb|0F82 irelw", Name = "jc", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpCarry(Processor p, short offset)
        {
            if (p.Flags.Carry)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpCarry), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpCarry32(Processor p, int offset)
        {
            if (p.Flags.Carry)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("73 irelb|0F83 irelw", Name = "jnc", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotCarry(Processor p, short offset)
        {
            if (!p.Flags.Carry)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpNotCarry), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotCarry32(Processor p, int offset)
        {
            if (!p.Flags.Carry)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("74 irelb|0F84 irelw", Name = "jz", AddressSize = 16 | 32)]
        public static void JumpZero(Processor p, short offset)
        {
            if (p.Flags.Zero)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpZero), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpZero32(Processor p, int offset)
        {
            if (p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("75 irelb|0F85 irelw", Name = "jnz", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotZero(Processor p, short offset)
        {
            if (!p.Flags.Zero)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpNotZero), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotZero32(Processor p, int offset)
        {
            if (!p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("76 irelb|0F86 irelw", Name = "jcz", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpCarryOrZero(Processor p, short offset)
        {
            if (p.Flags.Carry || p.Flags.Zero)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpCarryOrZero), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpCarryOrZero32(Processor p, int offset)
        {
            if (p.Flags.Carry || p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("77 irelb|0F87 irelw", Name = "jncz", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotCarryAndNotZero(Processor p, short offset)
        {
            if (!p.Flags.Carry && !p.Flags.Zero)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpNotCarryAndNotZero), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotCarryAndNotZero32(Processor p, int offset)
        {
            if (!p.Flags.Carry && !p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("78 irelb|0F88 irelw", Name = "js", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpSign(Processor p, short offset)
        {
            if (p.Flags.Sign)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpSign), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpSign32(Processor p, int offset)
        {
            if (p.Flags.Sign)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("79 irelb|0F89 irelw", Name = "jns", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotSign(Processor p, short offset)
        {
            if (!p.Flags.Sign)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpNotSign), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNotSign32(Processor p, int offset)
        {
            if (!p.Flags.Sign)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("7C irelb|0F8C irelw", Name = "jl", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpLessThan(Processor p, short offset)
        {
            if (p.Flags.Sign != p.Flags.Overflow)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpLessThan), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpLessThan32(Processor p, int offset)
        {
            if (p.Flags.Sign != p.Flags.Overflow)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("7D irelb|0F8D irelw", Name = "jge", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpGreaterThanOrEqual(Processor p, short offset)
        {
            if (p.Flags.Sign == p.Flags.Overflow)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpGreaterThanOrEqual), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpGreaterThanOrEqual32(Processor p, int offset)
        {
            if (p.Flags.Sign == p.Flags.Overflow)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("7E irelb|0F8E irelw", Name = "jle", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpLessThanOrEqual(Processor p, short offset)
        {
            if (p.Flags.Zero || (p.Flags.Sign != p.Flags.Overflow))
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpLessThanOrEqual), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpLessThanOrEqual32(Processor p, int offset)
        {
            if (p.Flags.Zero || (p.Flags.Sign != p.Flags.Overflow))
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("7F irelb|0F8F irelw", Name = "jg", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpGreaterThan(Processor p, short offset)
        {
            if (!p.Flags.Zero && (p.Flags.Sign == p.Flags.Overflow))
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpGreaterThan), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpGreaterThan32(Processor p, int offset)
        {
            if (!p.Flags.Zero && (p.Flags.Sign == p.Flags.Overflow))
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("E3 irelb", Name = "jcxz", AddressSize = 16, OperandSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpCXZero(Processor p, short offset)
        {
            if (p.CX == 0)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpCXZero), AddressSize = 32, OperandSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpECXZero(Processor p, short offset)
        {
            if (p.ECX == 0)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpCXZero), AddressSize = 16, OperandSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpCXZero32(Processor p, int offset)
        {
            if (p.CX == 0)
                p.EIP = (uint)((int)p.EIP + offset);
        }
        [Alternate(nameof(JumpCXZero), AddressSize = 32, OperandSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpECXZero32(Processor p, int offset)
        {
            if (p.ECX == 0)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("7A irelb|0F8A irelw", Name = "jp", AddressSize = 16 | 32, OperandSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpParity(Processor p, short offset)
        {
            if (p.Flags.Parity)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpParity), AddressSize = 16 | 32, OperandSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpParity32(Processor p, int offset)
        {
            if (p.Flags.Parity)
                p.EIP = (uint)((int)p.EIP + offset);
        }

        [Opcode("7B irelb|0F8B irelw", Name = "jnp", AddressSize = 16 | 32, OperandSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNoParity(Processor p, short offset)
        {
            if (!p.Flags.Parity)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(JumpNoParity), AddressSize = 16 | 32, OperandSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void JumpNoParity32(Processor p, int offset)
        {
            if (!p.Flags.Parity)
                p.EIP = (uint)((int)p.EIP + offset);
        }
    }
}
