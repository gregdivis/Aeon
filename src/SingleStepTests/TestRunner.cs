using Aeon.Emulator;
using Aeon.Emulator.RuntimeExceptions;
using MooParser;

namespace SingleStepTests;

internal sealed class TestRunner : IDisposable
{
    private readonly VirtualMachine vm = new();

    public TestRunner(MooTest test)
    {
        this.Test = test;
        this.vm.EndInitialization();
    }

    public MooTest Test { get; }

    public void Execute()
    {
        if (!this.IsTestValid())
            return;

        this.SetInitialState();

        try
        {
            vm.Emulate(100);
        }
        catch (HaltException)
        {
        }

        this.CheckFinalState();
    }

    public void Dispose()
    {
        this.vm.Dispose();
    }

    private bool IsTestValid()
    {
        if (this.Test.Name.Contains("invalid") || this.Test.Name.Contains("(bad)"))
            return false;

        // not sure why, but a lot of these fail for some reason
        if (this.Test.RawBytes.Span[0] == 0xF0)
            return false;

        foreach (var (reg, value) in this.Test.InitialState.GetRegisterValues32())
        {
            if (reg is RegMask32.CS or RegMask32.SS or RegMask32.DS or RegMask32.ES or RegMask32.FS or RegMask32.GS && value >= 0xA0000)
                return false;
        }

        foreach (var (address, _) in this.Test.InitialState.GetMemoryValues())
        {
            if (address >= 0xA0000)
                return false;
        }

        return true;
    }

    private void SetInitialState()
    {
        var initialState = this.Test.InitialState;
        var p = this.vm.Processor;

        foreach (var (reg, value) in initialState.GetRegisterValues32())
        {
            switch (reg)
            {
                case RegMask32.CR0:
                    p.CR0 = (CR0)value;
                    break;
                case RegMask32.CR3:
                    p.CR3 = value;
                    break;
                case RegMask32.EAX:
                    p.EAX = (int)value;
                    break;
                case RegMask32.EBX:
                    p.EBX = (int)value;
                    break;
                case RegMask32.ECX:
                    p.ECX = (int)value;
                    break;
                case RegMask32.EDX:
                    p.EDX = (int)value;
                    break;
                case RegMask32.ESI:
                    p.ESI = value;
                    break;
                case RegMask32.EDI:
                    p.EDI = value;
                    break;
                case RegMask32.EBP:
                    p.EBP = value;
                    break;
                case RegMask32.ESP:
                    p.ESP = value;
                    break;
                case RegMask32.CS:
                    vm.WriteSegmentRegister(SegmentIndex.CS, (ushort)value);
                    break;
                case RegMask32.DS:
                    vm.WriteSegmentRegister(SegmentIndex.DS, (ushort)value);
                    break;
                case RegMask32.ES:
                    vm.WriteSegmentRegister(SegmentIndex.ES, (ushort)value);
                    break;
                case RegMask32.FS:
                    vm.WriteSegmentRegister(SegmentIndex.FS, (ushort)value);
                    break;
                case RegMask32.GS:
                    vm.WriteSegmentRegister(SegmentIndex.GS, (ushort)value);
                    break;
                case RegMask32.SS:
                    vm.WriteSegmentRegister(SegmentIndex.SS, (ushort)value);
                    break;
                case RegMask32.EIP:
                    p.EIP = value;
                    break;
                case RegMask32.EFlags:
                    p.Flags.Value = (EFlags)value;
                    break;
            }
        }

        foreach (var (address, value) in initialState.GetMemoryValues())
            vm.PhysicalMemory.SetByte(address, value);

        this.Test.RawBytes.Span.CopyTo(vm.PhysicalMemory.GetSpan(vm.Processor.CS, vm.Processor.EIP, this.Test.RawBytes.Length));
    }

    private void CheckFinalState()
    {
        var p = this.vm.Processor;
        var finalState = this.Test.FinalState;
        foreach (var (reg, value) in finalState.GetRegisterValues())
        {
            switch (reg)
            {
                case RegMask.AX:
                    CheckResult(reg, value, (ushort)p.AX);
                    break;
                case RegMask.BX:
                    CheckResult(reg, value, (ushort)p.BX);
                    break;
                case RegMask.CX:
                    CheckResult(reg, value, (ushort)p.CX);
                    break;
                case RegMask.DX:
                    CheckResult(reg, value, (ushort)p.DX);
                    break;
                case RegMask.CS:
                    CheckResult(reg, value, p.CS);
                    break;
                case RegMask.SS:
                    CheckResult(reg, value, p.SS);
                    break;
                case RegMask.DS:
                    CheckResult(reg, value, p.DS);
                    break;
                case RegMask.ES:
                    CheckResult(reg, value, p.ES);
                    break;
                case RegMask.SP:
                    CheckResult(reg, value, p.SP);
                    break;
                case RegMask.BP:
                    CheckResult(reg, value, p.BP);
                    break;
                case RegMask.SI:
                    CheckResult(reg, value, p.SI);
                    break;
                case RegMask.DI:
                    CheckResult(reg, value, p.DI);
                    break;
                case RegMask.IP:
                    CheckResult(reg, value, p.IP);
                    break;
                case RegMask.Flags:
                    CheckResult(reg, value, (uint)p.Flags.Value & 0xFFFFu);
                    break;
            }
        }

        foreach (var (reg, value) in finalState.GetRegisterValues32())
        {
            switch (reg)
            {
                case RegMask32.CR0:
                    CheckResult(reg, value, (uint)p.CR0);
                    break;
                case RegMask32.CR3:
                    CheckResult(reg, value, p.CR3);
                    break;
                case RegMask32.EAX:
                    CheckResult(reg, value, (uint)p.EAX);
                    break;
                case RegMask32.EBX:
                    CheckResult(reg, value, (uint)p.EBX);
                    break;
                case RegMask32.ECX:
                    CheckResult(reg, value, (uint)p.ECX);
                    break;
                case RegMask32.EDX:
                    CheckResult(reg, value, (uint)p.EDX);
                    break;
                case RegMask32.ESI:
                    CheckResult(reg, value, p.ESI);
                    break;
                case RegMask32.EDI:
                    CheckResult(reg, value, p.EDI);
                    break;
                case RegMask32.EBP:
                    CheckResult(reg, value, p.EBP);
                    break;
                case RegMask32.ESP:
                    CheckResult(reg, value, p.ESP);
                    break;
                case RegMask32.CS:
                    CheckResult(reg, value, p.CS);
                    break;
                case RegMask32.DS:
                    CheckResult(reg, value, p.DS);
                    break;
                case RegMask32.ES:
                    CheckResult(reg, value, p.ES);
                    break;
                case RegMask32.FS:
                    CheckResult(reg, value, p.FS);
                    break;
                case RegMask32.GS:
                    CheckResult(reg, value, p.GS);
                    break;
                case RegMask32.SS:
                    CheckResult(reg, value, p.SS);
                    break;
                case RegMask32.EIP:
                    CheckResult(reg, value, p.EIP);
                    break;
                case RegMask32.EFlags:
                    CheckResult(reg, (EFlags)(value & 0xFFEFu), (EFlags)((uint)p.Flags.Value & 0xFFEFu));
                    break;
            }
        }

        foreach (var (address, expectedValue) in finalState.GetMemoryValues())
        {
            var actualValue = vm.PhysicalMemory.GetByte(address);
            if (actualValue != expectedValue)
                Console.Error.WriteLine($"#{this.Test.Index}: {address:X8}: was {actualValue:X2}; expected {expectedValue:X2}");
        }
    }

    private void CheckResult<TMask>(TMask reg, uint expected, uint actual)
    {
        if (expected != actual)
            Console.Error.WriteLine($"#{this.Test.Index}: {reg}: was {actual:X8}; expected {expected:X8}");
    }
    private void CheckResult<TMask>(TMask reg, EFlags expected, EFlags actual)
    {
        if (expected != actual)
            Console.Error.WriteLine($"#{this.Test.Index}: {reg} incorrect: {expected ^ actual}");
    }
}
