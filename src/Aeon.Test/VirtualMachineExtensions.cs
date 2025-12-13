using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Aeon.Emulator;
using Aeon.Emulator.DebugSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test;

/// <summary>
/// Contains extension methods for the VirtualMachine class.
/// </summary>
internal static class VirtualMachineExtensions
{
    /// <summary>
    /// Sets all of the registers on an emulated processor.
    /// </summary>
    /// <param name="vm">VirtualMachine instance which owns the processor.</param>
    /// <param name="cpuState">New state of the processor.</param>
    public static void SetState(this VirtualMachine vm, IRegisterContainer cpuState)
    {
        var p = vm.Processor;
        p.EAX = (int)cpuState.EAX;
        p.EBX = (int)cpuState.EBX;
        p.ECX = (int)cpuState.ECX;
        p.EDX = (int)cpuState.EDX;
        p.ESI = cpuState.ESI;
        p.EDI = cpuState.EDI;
        p.ESP = cpuState.ESP;
        p.EBP = cpuState.EBP;
        vm.WriteSegmentRegister(SegmentIndex.DS, cpuState.DS);
        vm.WriteSegmentRegister(SegmentIndex.ES, cpuState.ES);
        vm.WriteSegmentRegister(SegmentIndex.FS, cpuState.FS);
        vm.WriteSegmentRegister(SegmentIndex.GS, cpuState.GS);
        vm.WriteSegmentRegister(SegmentIndex.SS, cpuState.SS);
        p.CR0 = cpuState.CR0;
        p.Flags.Value = cpuState.Flags;
    }
    /// <summary>
    /// Writes machine code to emulated memory.
    /// </summary>
    /// <param name="vm">VirtualMachine instance where code will be written.</param>
    /// <param name="segment">Segment where code will be written.</param>
    /// <param name="offset">Offset where code will be written.</param>
    /// <param name="code">Strings of hexadecimal values to write to memory.</param>
    public static void WriteCode(this VirtualMachine vm, ushort segment, uint offset, params string[] code)
    {
        if(code == null || code.Length == 0)
            return;

        WriteBytes(vm, segment, offset, code.SelectMany(c => ParseCode(c)));
    }
    /// <summary>
    /// Writes machine code to emulated memory starting at CS:EIP.
    /// </summary>
    /// <param name="vm">VirtualMachine instance where code will be written.</param>
    /// <param name="code">Strings of hexadecimal values to write to memory.</param>
    public static void WriteCode(this VirtualMachine vm, params string[] code)
    {
        WriteCode(vm, vm.Processor.CS, vm.Processor.EIP, code);
    }
    /// <summary>
    /// Writes bytes to emulated memory.
    /// </summary>
    /// <param name="vm">VirtualMachine instance where bytes will be written.</param>
    /// <param name="segment">Segment where bytes will be written.</param>
    /// <param name="offset">Offset where bytes will be written.</param>
    /// <param name="bytes">Bytes to write to memory.</param>
    public static void WriteBytes(this VirtualMachine vm, ushort segment, uint offset, IEnumerable<byte> bytes)
    {
        foreach(var b in bytes)
        {
            vm.PhysicalMemory.SetByte(segment, offset, b);
            offset++;
        }
    }
    /// <summary>
    /// Reads bytes from emulated memory.
    /// </summary>
    /// <param name="vm">VirtualMachine instance to read bytes from.</param>
    /// <param name="segment">Segment where bytes will be read from.</param>
    /// <param name="offset">Offset where bytes will be read from.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>Bytes read from the specified address.</returns>
    public static IEnumerable<byte> ReadBytes(this VirtualMachine vm, ushort segment, uint offset, uint count)
    {
        for(uint i = 0; i < count; i++)
            yield return vm.PhysicalMemory.GetByte(segment, offset + i);
    }
    /// <summary>
    /// Test-runs the emulator starting at CS:EIP.
    /// </summary>
    /// <param name="vm">VirtualMachine instance to test.</param>
    /// <param name="limit">Maximum number of instructions to emulate.</param>
    public static void TestEmulator(this VirtualMachine vm, int limit)
    {
        int count = 0;
        try
        {
            while(count < limit)
            {
                vm.Emulate();
                count++;
            }
        }
        catch(EndOfProgramException)
        {
            return;
        }

        Assert.Fail("Instruction limit exceeded.");
    }

    /// <summary>
    /// Converts a string of machine code into an enumeration of bytes.
    /// </summary>
    /// <param name="code">String of machine code to convert.</param>
    /// <returns>Enumeration of bytes.</returns>
    private static IEnumerable<byte> ParseCode(string code)
    {
        if(string.IsNullOrWhiteSpace(code))
            yield break;

        var units = code.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach(var unit in units)
        {
            string s = unit;
            if((unit.Length % 2) == 1)
                s = "0" + unit;

            for(int i = s.Length - 2; i >= 0; i -= 2)
                yield return byte.Parse(s.Substring(i, 2), NumberStyles.HexNumber);
        }
    }
}
