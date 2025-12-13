using System;
using System.Collections.Generic;
using System.Linq;
using Aeon.Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test;

/// <summary>
/// Summary description for UnitTest1
/// </summary>
[TestClass]
public class CompiledStrings
{
    public CompiledStrings()
    {
    }

    private const ushort CS = 0x1000;
    private const uint EIP = 0;

    private VirtualMachine vm;
    private CpuState initialState;
    private byte[] randomData;

    /// <summary>
    /// Gets or sets the test context which provides
    /// information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        this.vm = new VirtualMachine();
        this.initialState = new CpuState()
        {
            DS = 0x5000,
            ES = 0x7000,
            ECX = 5000
        };

        var random = new Random();
        this.randomData = new byte[5200];
        random.NextBytes(this.randomData);
        for(int i = 0; i < randomData.Length; i++)
        {
            if(this.randomData[i] == 0)
                this.randomData[i] = 1;
        }

        vm.WriteBytes(this.initialState.DS, this.initialState.ESI, this.randomData);
        vm.SetState(this.initialState);
        vm.WriteSegmentRegister(SegmentIndex.CS, CS);
        vm.Processor.EIP = EIP;
    }
    [TestCleanup]
    public void TestCleanup()
    {
        this.vm.Dispose();
    }

    #region LODS
    [TestMethod]
    public void Lodsb()
    {
        vm.WriteCode(
            "F3 AC", // rep lodsb
            "CD 20"  // int 20h
            );

        vm.TestEmulator(10200);

        Assert.AreEqual(0, vm.Processor.ECX);
        Assert.AreEqual(this.randomData[this.initialState.ECX - 1], vm.Processor.AL);
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)this.initialState.EDI, vm.Processor.DI);
    }
    [TestMethod]
    public void Lodsw()
    {
        vm.WriteCode(
            "F3 AD", // rep lodsw
            "CD 20"  // int 20h
            );

        vm.Processor.ECX /= 2;
        vm.TestEmulator(5200);

        var expectedAX = BitConverter.ToInt16(this.randomData, (int)this.initialState.ECX - 2);

        Assert.AreEqual(0, vm.Processor.ECX);
        Assert.AreEqual(expectedAX, vm.Processor.AX);
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)this.initialState.EDI, vm.Processor.DI);
    }
    #endregion

    #region STOS
    [TestMethod]
    public void Stosb()
    {
        vm.WriteCode(
            "F3 AA", // rep stosb
            "CD 20"  // int 20h
            );

        vm.Processor.AL = 17;

        vm.TestEmulator(5200 * 2);

        Assert.AreEqual(0, vm.Processor.ECX);
        Assert.AreEqual(17, vm.Processor.AL);
        Assert.AreEqual(0, vm.PhysicalMemory.GetByte(vm.Processor.ES, vm.Processor.EDI));
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.EDI), vm.Processor.DI);
        var data = vm.ReadBytes(this.initialState.ES, this.initialState.EDI, this.initialState.ECX);
        foreach(var b in data)
            Assert.AreEqual(17, b);
    }
    [TestMethod]
    public void Stosw()
    {
        vm.WriteCode(
            "F3 AB", // rep stosb
            "CD 20"  // int 20h
            );

        vm.Processor.ECX /= 2;
        vm.Processor.AX = 0x1717;

        vm.TestEmulator(5200);

        Assert.AreEqual(0, vm.Processor.ECX);
        Assert.AreEqual(0x1717, vm.Processor.AX);
        Assert.AreEqual(0, vm.PhysicalMemory.GetByte(vm.Processor.ES, vm.Processor.EDI));
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.EDI), vm.Processor.DI);
        var data = vm.ReadBytes(this.initialState.ES, this.initialState.EDI, this.initialState.ECX);
        foreach(var b in data)
            Assert.AreEqual(0x17, b);
    }
    #endregion

    #region MOVS
    [TestMethod]
    public void Movsb()
    {
        vm.WriteCode(
            "F3 A4", // rep movsb
            "CD 20"  // int 20h
            );

        vm.TestEmulator(5200 * 2);

        Assert.AreEqual(0, vm.Processor.ECX);
        Assert.AreEqual(0, vm.PhysicalMemory.GetByte(vm.Processor.ES, vm.Processor.EDI));
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.EDI), vm.Processor.DI);
        var data = vm.ReadBytes(this.initialState.ES, this.initialState.EDI, this.initialState.ECX);
        foreach(var value in this.randomData.Zip(data, (s, d) => s ^ d))
            Assert.AreEqual(0, value);
    }
    [TestMethod]
    public void Movsw()
    {
        vm.WriteCode(
            "F3 A5", // rep movsb
            "CD 20"  // int 20h
            );

        vm.Processor.ECX /= 2;

        vm.TestEmulator(5200);

        Assert.AreEqual(0, vm.Processor.ECX);
        Assert.AreEqual(0, vm.PhysicalMemory.GetByte(vm.Processor.ES, vm.Processor.EDI));
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)(this.initialState.ECX + this.initialState.EDI), vm.Processor.DI);
        var data = vm.ReadBytes(this.initialState.ES, this.initialState.EDI, this.initialState.ECX);
        foreach(var value in this.randomData.Zip(data, (s, d) => s ^ d))
            Assert.AreEqual(0, value);
    }
    #endregion

    #region CMPS
    [TestMethod]
    public void RepeCmpsb()
    {
        vm.WriteCode(
            "F3 A6", // repe cmpsb
            "CD 20"  // int 20h
            );

        vm.WriteBytes(this.initialState.DS, this.initialState.ESI, Bytes(17, 100).Concat(Bytes(1, 1)));
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(17, 100).Concat(Bytes(2, 1)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 101;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(101 + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)(101 + this.initialState.EDI), vm.Processor.DI);
    }
    [TestMethod]
    public void RepneCmpsb()
    {
        vm.WriteCode(
            "F2 A6", // repne cmpsb
            "CD 20"  // int 20h
            );

        vm.WriteBytes(this.initialState.DS, this.initialState.ESI, Bytes(17, 100).Concat(Bytes(1, 1)));
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(18, 100).Concat(Bytes(1, 1)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 101;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(101 + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)(101 + this.initialState.EDI), vm.Processor.DI);
    }
    [TestMethod]
    public void RepeCmpsw()
    {
        vm.WriteCode(
            "F3 A7", // repe cmpsw
            "CD 20"  // int 20h
            );

        vm.WriteBytes(this.initialState.DS, this.initialState.ESI, Bytes(17, 100).Concat(Bytes(1, 1)));
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(17, 100).Concat(Bytes(2, 1)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 51;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(102 + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)(102 + this.initialState.EDI), vm.Processor.DI);
    }
    [TestMethod]
    public void RepneCmpsw()
    {
        vm.WriteCode(
            "F2 A7", // repne cmpsw
            "CD 20"  // int 20h
            );

        vm.WriteBytes(this.initialState.DS, this.initialState.ESI, Bytes(17, 100).Concat(Bytes(1, 2)));
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(18, 100).Concat(Bytes(1, 2)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 51;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(102 + this.initialState.ESI), vm.Processor.SI);
        Assert.AreEqual((ushort)(102 + this.initialState.EDI), vm.Processor.DI);
    }
    #endregion

    #region SCAS
    [TestMethod]
    public void RepeScasb()
    {
        vm.WriteCode(
            "F3 AE", // repe scasb
            "CD 20"  // int 20h
            );

        vm.Processor.AL = 17;
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(17, 100).Concat(Bytes(2, 1)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 101;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(101 + this.initialState.EDI), vm.Processor.DI);
    }
    [TestMethod]
    public void RepneScasb()
    {
        vm.WriteCode(
            "F2 AE", // repne scasb
            "CD 20"  // int 20h
            );

        vm.Processor.AL = 1;
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(18, 100).Concat(Bytes(1, 1)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 101;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(101 + this.initialState.EDI), vm.Processor.DI);
    }
    [TestMethod]
    public void RepeScasw()
    {
        vm.WriteCode(
            "F3 AF", // repe scasw
            "CD 20"  // int 20h
            );

        vm.Processor.AX = 0x1717;
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(0x17, 100).Concat(Bytes(2, 1)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 51;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(102 + this.initialState.EDI), vm.Processor.DI);
    }
    [TestMethod]
    public void RepneScasw()
    {
        vm.WriteCode(
            "F2 AF", // repne scasw
            "CD 20"  // int 20h
            );

        vm.Processor.AX = 0x0101;
        vm.WriteBytes(this.initialState.ES, this.initialState.EDI, Bytes(18, 100).Concat(Bytes(1, 2)));

        vm.TestEmulator(5200);

        var finalCX = this.initialState.ECX - 51;
        Assert.AreEqual(finalCX, (uint)vm.Processor.ECX);

        Assert.AreEqual((ushort)(102 + this.initialState.EDI), vm.Processor.DI);
    }
    #endregion

    private static IEnumerable<byte> Bytes(byte value, int count)
    {
        for(int i = 0; i < count; i++)
            yield return value;
    }
}
