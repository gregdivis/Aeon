using System;
using Aeon.Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test;

[TestClass]
public class Cmp
{
    private const ushort CS = 0x1000;
    private const uint EIP = 0;
    private const EFlags FlagMask = EFlags.Carry | EFlags.Sign | EFlags.Parity | EFlags.Overflow | EFlags.Zero;

    private VirtualMachine vm;
    private static readonly ushort[,] flagValues = new ushort[256, 256];

    /// <summary>
    /// Gets or sets the test context which provides
    /// information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        var buffer = Properties.Resources.CmpFlags;

        for(int a = 0; a < 256; a++)
        {
            for(int b = 0; b < 256; b++)
                flagValues[a, b] = BitConverter.ToUInt16(buffer, (a * 256 + b) * 2);
        }
    }

    [TestInitialize]
    public void TestInitialize()
    {
        this.vm = new VirtualMachine();

        Reset();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        this.vm.Dispose();
    }

    [TestMethod]
    public void ByteCmp()
    {
        vm.WriteCode(
            "38 C8", // cmp al, cl
            "CD 20"  // int 20h
            );

        for(int a = 0; a < 256; a++)
        {
            for(int b = 0; b < 256; b++)
            {
                Reset();
                vm.Processor.AL = (byte)a;
                vm.Processor.CL = (byte)b;

                vm.TestEmulator(50);

                Assert.AreEqual((EFlags)flagValues[a, b] & FlagMask, vm.Processor.Flags.Value & FlagMask, string.Format("a=0x{0:X2}, b=0x{1:X2}", a, b));
            }
        }
    }

    [TestMethod]
    public void ByteSub()
    {
        vm.WriteCode(
            "28 C8", // sub al, cl
            "CD 20"  // int 20h
            );

        for(int a = 0; a < 256; a++)
        {
            for(int b = 0; b < 256; b++)
            {
                Reset();
                vm.Processor.AL = (byte)a;
                vm.Processor.CL = (byte)b;

                vm.TestEmulator(50);

                Assert.AreEqual((byte)(a - b), vm.Processor.AL);
                Assert.AreEqual((EFlags)flagValues[a, b] & FlagMask, vm.Processor.Flags.Value & FlagMask, string.Format("a=0x{0:X2}, b=0x{1:X2}", a, b));
            }
        }
    }

    [TestMethod]
    public void ByteSbb()
    {
        vm.WriteCode(
            "18 C8", // sbb al, cl
            "CD 20"  // int 20h
            );

        for(int a = 0; a < 256; a++)
        {
            for(int b = 0; b < 256; b++)
            {
                Reset();
                vm.Processor.AL = (byte)a;
                vm.Processor.CL = (byte)b;

                vm.TestEmulator(50);

                Assert.AreEqual((byte)(a - b), vm.Processor.AL);
                Assert.AreEqual((EFlags)flagValues[a, b] & FlagMask, vm.Processor.Flags.Value & FlagMask, string.Format("a=0x{0:X2}, b=0x{1:X2}", a, b));

                Reset();
                vm.Processor.Flags.Value |= EFlags.Carry;
                vm.Processor.AL = (byte)a;
                vm.Processor.CL = (byte)b;

                vm.TestEmulator(50);

                Assert.AreEqual((byte)(a - (b + 1)), vm.Processor.AL);
                //Assert.AreEqual((EFlags)flagValues[a, b] & FlagMask, vm.Processor.Flags & FlagMask, string.Format("a=0x{0:X2}, b=0x{1:X2}", a, b));
            }
        }
    }

    private void Reset()
    {
        vm.Processor.Flags.Value = EFlags.Reserved1 | EFlags.InterruptEnable;
        vm.WriteSegmentRegister(SegmentIndex.CS, CS);
        vm.Processor.EIP = EIP;
        vm.Processor.ESP = 0xFFFF;
    }
}
