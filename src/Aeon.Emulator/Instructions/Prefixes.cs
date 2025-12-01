namespace Aeon.Emulator.Instructions;

internal static class Prefixes
{
    [Opcode("F0", Name = "lock:", IsPrefix = true)]
    public static void Lock(Processor p)
    {
        p.IncrementPrefixCount();
    }
    [Opcode("2E", Name = "cs:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void CSOverride(Processor p)
    {
        p.SegmentOverride = SegmentRegister.CS;
        p.IncrementPrefixCount();
    }
    [Opcode("36", Name = "ss:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SSOverride(Processor p)
    {
        p.SegmentOverride = SegmentRegister.SS;
        p.IncrementPrefixCount();
    }
    [Opcode("3E", Name = "ds:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DSOverride(Processor p)
    {
        p.SegmentOverride = SegmentRegister.DS;
        p.IncrementPrefixCount();
    }
    [Opcode("26", Name = "es:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ESOverride(Processor p)
    {
        p.SegmentOverride = SegmentRegister.ES;
        p.IncrementPrefixCount();
    }
    [Opcode("64", Name = "fs:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void FSOverride(Processor p)
    {
        p.SegmentOverride = SegmentRegister.FS;
        p.IncrementPrefixCount();
    }
    [Opcode("65", Name = "gs:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GSOverride(Processor p)
    {
        p.SegmentOverride = SegmentRegister.GS;
        p.IncrementPrefixCount();
    }
    [Opcode("66", Name = "opsize:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void OperandSize(Processor p)
    {
        p.SetSizeOverrideFlag(1);
        p.IncrementPrefixCount();
    }
    [Opcode("67", Name = "adrsize:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void AddressSize(Processor p)
    {
        p.SetSizeOverrideFlag(2);
        p.IncrementPrefixCount();
    }
    [Opcode("F2", Name = "repne:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Repne(Processor p)
    {
        p.RepeatPrefix = RepeatPrefix.Repne;
        p.IncrementPrefixCount();
    }
    [Opcode("F3", Name = "repe:", IsPrefix = true, OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Repe(Processor p)
    {
        p.RepeatPrefix = RepeatPrefix.Repe;
        p.IncrementPrefixCount();
    }
}
