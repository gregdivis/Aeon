using System.Runtime.CompilerServices;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Instructions
{
    internal static class Call
    {
        [Opcode("E8 irelw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearRelativeCall(VirtualMachine vm, short offset)
        {
            int ip = vm.Processor.IP;
            vm.PushToStack((ushort)ip);

            vm.Processor.EIP = (ushort)(ip + offset);
        }
        [Alternate(nameof(NearRelativeCall), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearRelativeCall32(VirtualMachine vm, int offset)
        {
            int ip = (int)vm.Processor.EIP;
            vm.PushToStack32((uint)ip);

            vm.Processor.EIP = (uint)(ip + offset);
        }

        [Opcode("FF/2 jmprmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearAbsoluteCall(VirtualMachine vm, ushort ip)
        {
            vm.PushToStack(vm.Processor.IP);

            vm.Processor.EIP = ip;
        }
        [Alternate(nameof(NearAbsoluteCall), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearAbsoluteCall32(VirtualMachine vm, uint ip)
        {
            vm.PushToStack32(vm.Processor.EIP);

            vm.Processor.EIP = ip;
        }

        [Opcode("9A iptr|FF/3 mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FarAbsoluteCall(VirtualMachine vm, uint address)
        {
            if (address == 0)
                ThrowHelper.ThrowNullCallException();

            if (!vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable))
            {
                // Real mode call.
                vm.PushToStack(vm.Processor.CS, vm.Processor.IP);

                vm.WriteSegmentRegister(SegmentIndex.CS, Intrinsics.HighWord(address));
                vm.Processor.EIP = Intrinsics.LowWord(address);
            }
            else
            {
                ProtectedModeFarCall(vm, Intrinsics.HighWord(address), Intrinsics.LowWord(address), false);
            }
        }
        [Alternate(nameof(FarAbsoluteCall), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FarAbsoluteCall32(VirtualMachine vm, ulong address)
        {
            if (address == 0)
                ThrowHelper.ThrowNullCallException();

            if (!vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable))
            {
                // Real mode call.
                vm.PushToStack32(vm.Processor.CS);
                vm.PushToStack32(vm.Processor.EIP);

                vm.WriteSegmentRegister(SegmentIndex.CS, (ushort)Intrinsics.HighDWord(address));
                vm.Processor.EIP = Intrinsics.LowDWord(address);
            }
            else
            {
                ProtectedModeFarCall(vm, (ushort)Intrinsics.HighDWord(address), Intrinsics.LowDWord(address), true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void ProtectedModeFarCall(VirtualMachine vm, ushort selector, uint offset, bool is32Bit)
        {
            var descriptor = vm.PhysicalMemory.GetDescriptor(selector);
            var type = descriptor.DescriptorType;

            if (type == DescriptorType.Segment)
            {
                if (is32Bit)
                {
                    vm.PushToStack32(vm.Processor.CS);
                    vm.PushToStack32(vm.Processor.EIP);
                }
                else
                {
                    vm.PushToStack(vm.Processor.CS, vm.Processor.IP);
                }

                vm.WriteSegmentRegister(SegmentIndex.CS, selector);
                vm.Processor.EIP = offset;
            }
            else if (type == DescriptorType.CallGate)
            {
                var callGate = (CallGateDescriptor)descriptor;

                uint cpl = vm.Processor.CS & 3u;
                uint dpl = callGate.Selector & 3u;

                if (callGate.DWordCount != 0)
                    ThrowHelper.ThrowNotImplementedException();

                ushort oldSS = vm.Processor.SS;
                uint oldESP = vm.Processor.ESP;

                if (cpl > dpl)
                {
                    // Need to munge the stack in this case.
                    ushort newSS = vm.GetPrivilegedSS(dpl, 4);
                    uint newESP = vm.GetPrivilegedESP(dpl, 4);

                    vm.WriteSegmentRegister(SegmentIndex.SS, newSS);
                    vm.Processor.ESP = newESP;

                    vm.PushToStack32(oldSS);
                    vm.PushToStack32(oldESP);
                }
                else if (cpl < dpl)
                {
                    ThrowHelper.ThrowCplLessThanDplException();
                }

                vm.PushToStack32(vm.Processor.CS);
                vm.PushToStack32(vm.Processor.EIP);

                vm.WriteSegmentRegister(SegmentIndex.CS, callGate.Selector);
                vm.Processor.EIP = callGate.Offset;
            }
            else
            {
                ThrowHelper.ThrowNotImplementedException();
            }
        }
    }

    internal static class Ret
    {
        [Opcode("C3", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearReturn(VirtualMachine vm)
        {
            vm.Processor.EIP = vm.PopFromStack();

            vm.Processor.InstructionEpilog();
        }
        [Alternate(nameof(NearReturn), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearReturn32(VirtualMachine vm)
        {
            vm.Processor.EIP = vm.PopFromStack32();

            vm.Processor.InstructionEpilog();
        }

        [Opcode("C2 i16", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearReturnPop(VirtualMachine vm, ushort bytesToPop)
        {
            vm.Processor.EIP = vm.PopFromStack();
            vm.AddToStackPointer(bytesToPop);
        }
        [Alternate(nameof(NearReturnPop), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void NearReturnPop32(VirtualMachine vm, ushort bytesToPop)
        {
            vm.Processor.EIP = vm.PopFromStack32();
            vm.AddToStackPointer(bytesToPop);
        }

        [Opcode("CB", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FarReturn(VirtualMachine vm)
        {
            FarReturnPop(vm, 0);
            vm.Processor.InstructionEpilog();
        }
        [Alternate(nameof(FarReturn), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FarReturn32(VirtualMachine vm)
        {
            FarReturnPop32(vm, 0);
            vm.Processor.InstructionEpilog();
        }

        [Opcode("CA i16")]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FarReturnPop(VirtualMachine vm, ushort bytesToPop)
        {
            uint dest = vm.PeekStack32();
            uint eip = dest & 0xFFFFu;
            ushort cs = (ushort)(dest >> 16);

            if (vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable))
            {
                uint cpl = vm.Processor.CS & 3u;
                uint rpl = cs & 3u;
                if (cpl != rpl)
                    System.Diagnostics.Debugger.Break();
            }

            vm.WriteSegmentRegister(SegmentIndex.CS, cs);
            vm.Processor.EIP = eip;
            vm.AddToStackPointer(4u + bytesToPop);
        }
        [Alternate(nameof(FarReturnPop))]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void FarReturnPop32(VirtualMachine vm, ushort bytesToPop)
        {
            ulong dest = vm.PeekStack48();
            uint eip = Intrinsics.LowDWord(dest);
            ushort cs = (ushort)Intrinsics.HighDWord(dest);

            if (vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable))
            {
                uint cpl = vm.Processor.CS & 3u;
                uint rpl = cs & 3u;
                if (cpl != rpl)
                    System.Diagnostics.Debugger.Break();
            }

            vm.WriteSegmentRegister(SegmentIndex.CS, cs);
            vm.Processor.EIP = eip;
            vm.AddToStackPointer(8u + bytesToPop);
        }
    }
}
