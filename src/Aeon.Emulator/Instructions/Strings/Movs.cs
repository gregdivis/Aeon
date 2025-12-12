namespace Aeon.Emulator.Instructions.Strings;

internal static class Movsb
{
    [Opcode("A4", OperandSize = 16 | 32)]
    public static void MoveByte(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            MoveSingleByte(vm);
        else
            MoveBytes(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void MoveSingleByte(VirtualMachine vm)
    {
        var srcBase = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        byte src = vm.PhysicalMemory.GetByte(srcBase + vm.Processor.SI);

        vm.PhysicalMemory.SetByte(vm.Processor.ESBase + vm.Processor.DI, src);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.SI++;
            vm.Processor.DI++;
        }
        else
        {
            vm.Processor.SI--;
            vm.Processor.DI--;
        }
    }
    private static void MoveBytes(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            MoveSingleByte(vm);
            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(MoveByte), AddressSize = 32, OperandSize = 16 | 32)]
    public static void MoveByte32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            MoveSingleByte32(vm);
        else
            MoveBytes32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void MoveSingleByte32(VirtualMachine vm)
    {
        uint srcBase;
        var p = vm.Processor;

        unsafe
        {
            uint* basePtr = p.baseOverrides[(int)SegmentIndex.DS];
            if (basePtr == null)
                srcBase = p.segmentBases[(int)SegmentIndex.DS];
            else
                srcBase = *basePtr;

            ref uint esi =  ref p.ESI;
            ref uint edi = ref p.EDI;

            byte src = vm.PhysicalMemory.GetByte(srcBase + esi);
            vm.PhysicalMemory.SetByte(p.ESBase + edi, src);

            if (p.Flags.Direction)
            {
                esi--;
                edi--;
            }
            else
            {
                esi++;
                edi++;
            }
        }
    }
    private static void MoveBytes32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            MoveSingleByte32(vm);
            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}

internal static class Movsw
{
    private const uint MaxChunkSize = 512;

    [Opcode("A5")]
    public static void MoveWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            MoveSingleWord(vm);
        else
            MoveWords(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void MoveSingleWord(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        ushort src = vm.PhysicalMemory.GetUInt16(srcSegment + vm.Processor.SI);
        vm.PhysicalMemory.SetUInt16(vm.Processor.ESBase + vm.Processor.DI, src);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.SI += 2;
            vm.Processor.DI += 2;
        }
        else
        {
            vm.Processor.SI -= 2;
            vm.Processor.DI -= 2;
        }
    }
    private static void MoveWords(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            MoveSingleWord(vm);
            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(MoveWord), AddressSize = 16, OperandSize = 32)]
    public static void MoveDWord16(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            MoveSingleDWord16(vm);
        else
            MoveDWords16(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void MoveSingleDWord16(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        uint src = vm.PhysicalMemory.GetUInt32(srcSegment + vm.Processor.SI);
        vm.PhysicalMemory.SetUInt32(vm.Processor.ESBase + vm.Processor.DI, src);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.SI += 4;
            vm.Processor.DI += 4;
        }
        else
        {
            vm.Processor.SI -= 4;
            vm.Processor.DI -= 4;
        }
    }
    private static void MoveDWords16(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            MoveSingleDWord16(vm);
            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(MoveWord), AddressSize = 32, OperandSize = 16)]
    public static void MoveWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            MoveSingleWord32(vm);
        else
            MoveWords32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void MoveSingleWord32(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        ushort src = vm.PhysicalMemory.GetUInt16(srcSegment + vm.Processor.ESI);
        vm.PhysicalMemory.SetUInt16(vm.Processor.ESBase + vm.Processor.EDI, src);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.ESI += 2;
            vm.Processor.EDI += 2;
        }
        else
        {
            vm.Processor.ESI -= 2;
            vm.Processor.EDI -= 2;
        }
    }
    private static void MoveWords32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            MoveSingleWord32(vm);
            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(MoveWord), AddressSize = 32, OperandSize = 32)]
    public static void MoveDWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            MoveSingleDWord32(vm);
        else
            MoveDWords32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void MoveSingleDWord32(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        uint src = vm.PhysicalMemory.GetUInt32(srcSegment + vm.Processor.ESI);
        vm.PhysicalMemory.SetUInt32(vm.Processor.ESBase + vm.Processor.EDI, src);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.ESI += 4;
            vm.Processor.EDI += 4;
        }
        else
        {
            vm.Processor.ESI -= 4;
            vm.Processor.EDI -= 4;
        }
    }
    private static void MoveDWords32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            if (CopyDWordChunk32(vm))
            {
                MoveSingleDWord32(vm);
                vm.Processor.ECX--;
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
            }
        }
    }

    private static bool CopyDWordChunk32(VirtualMachine vm)
    {
        var p = vm.Processor;
        var srcSegment = p.GetOverrideBase(SegmentIndex.DS);
        uint srcAddress = srcSegment + p.ESI;
        uint destAddress = p.ESBase + p.EDI;

        uint maxBytes = Math.Min((uint)p.ECX, MaxChunkSize) * 4;

        var m = vm.PhysicalMemory;

        if (!p.Flags.Direction)
        {
            uint i = 0;
            try
            {
                for (i = 0; i < maxBytes; i += 4)
                    m.SetUInt32(destAddress + i, m.GetUInt32(srcAddress + i));
            }
            finally
            {
                p.ESI += i;
                p.EDI += i;
                p.ECX -= (int)i / 4;
            }
        }
        else
        {
            uint i = 0;
            try
            {
                for (i = 0; i < maxBytes; i += 4)
                    m.SetUInt32(destAddress - i, m.GetUInt32(srcAddress - i));
            }
            finally
            {
                p.ESI -= i;
                p.EDI -= i;
                p.ECX -= (int)i / 4;
            }
        }

        return p.ECX != 0;
    }
}
