using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class SaveRestore
{
    //http://www.mathemainzel.info/files/x86asmref.html

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DD/6 fulladdr:rmw", Name = "fnsave", OperandSize = 16 | 32, AddressSize = 16)]
    public static void Save(VirtualMachine vm, uint offset)
    {
        var fpu = vm.Processor.FPU;

        //https://www.website.masmforum.com/tutorials/fptute/fpuchap3.htm#fsave
        var span = vm.PhysicalMemory.GetPagedSpan(offset, 94);
        BinaryPrimitives.WriteUInt16LittleEndian(span, fpu.ControlWord);
        BinaryPrimitives.WriteUInt16LittleEndian(span[2..], fpu.StatusWord);
        BinaryPrimitives.WriteUInt16LittleEndian(span[4..], fpu.TagWord);
        BinaryPrimitives.WriteUInt16LittleEndian(span[6..], vm.Processor.IP);
        BinaryPrimitives.WriteUInt16LittleEndian(span[8..], vm.Processor.CS);
        BinaryPrimitives.WriteUInt16LittleEndian(span[10..], 0); // this is not correct
        BinaryPrimitives.WriteUInt16LittleEndian(span[12..], vm.Processor.DS);

        var regs = MemoryMarshal.Cast<byte, Real10>(span[14..]);
        for (int i = 0; i < 8; i++)
            regs[i] = fpu.GetRegisterValue(i);

        fpu.Reset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(Save), AddressSize = 32, OperandSize = 16 | 32)]
    public static void Save32(VirtualMachine vm, uint offset)
    {
        var fpu = vm.Processor.FPU;

        //https://www.website.masmforum.com/tutorials/fptute/fpuchap3.htm#fsave
        var span = vm.PhysicalMemory.GetPagedSpan(offset, 108);
        BinaryPrimitives.WriteUInt32LittleEndian(span, fpu.ControlWord);
        BinaryPrimitives.WriteUInt32LittleEndian(span[4..], fpu.StatusWord);
        BinaryPrimitives.WriteUInt32LittleEndian(span[8..], fpu.TagWord);
        BinaryPrimitives.WriteUInt32LittleEndian(span[12..], vm.Processor.IP);
        BinaryPrimitives.WriteUInt32LittleEndian(span[16..], vm.Processor.CS);
        BinaryPrimitives.WriteUInt32LittleEndian(span[20..], offset);
        BinaryPrimitives.WriteUInt32LittleEndian(span[24..], vm.Processor.DS);

        var regs = MemoryMarshal.Cast<byte, Real10>(span[28..]);
        for (int i = 0; i < 8; i++)
            regs[i] = fpu.GetRegisterValue(i);

        fpu.Reset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DD/4 fulladdr:rmw", Name = "frstor", OperandSize = 16 | 32, AddressSize = 16)]
    public static void Restore(VirtualMachine vm, uint offset)
    {
        var fpu = vm.Processor.FPU;

        var span = vm.PhysicalMemory.GetPagedSpan(offset, 94);
        fpu.ControlWord = BinaryPrimitives.ReadUInt16LittleEndian(span);
        fpu.StatusWord = BinaryPrimitives.ReadUInt16LittleEndian(span[2..]);
        fpu.TagWord = BinaryPrimitives.ReadUInt16LittleEndian(span[4..]);
        var regs = MemoryMarshal.Cast<byte, Real10>(span[14..]);
        for (int i = 0; i < 8; i++)
            fpu.SetRegisterValue(i, (double)regs[i]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(Restore), OperandSize = 16 | 32, AddressSize = 32)]
    public static void Restore32(VirtualMachine vm, uint offset)
    {
        var fpu = vm.Processor.FPU;

        var span = vm.PhysicalMemory.GetPagedSpan(offset, 108);
        fpu.ControlWord = BinaryPrimitives.ReadUInt16LittleEndian(span);
        fpu.StatusWord = BinaryPrimitives.ReadUInt16LittleEndian(span[4..]);
        fpu.TagWord = BinaryPrimitives.ReadUInt16LittleEndian(span[8..]);
        var regs = MemoryMarshal.Cast<byte, Real10>(span[28..]);
        for (int i = 0; i < 8; i++)
            fpu.SetRegisterValue(i, (double)regs[i]);
    }
}
