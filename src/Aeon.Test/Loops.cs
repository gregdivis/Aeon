using Aeon.Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test;

[TestClass]
public class Loops
{
    private const ushort CS = 0x1000;
    private const uint EIP = 0;

    private VirtualMachine vm;
    private int count;

    /// <summary>
    /// Gets or sets the test context which provides
    /// information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        this.vm = new VirtualMachine(
            new VirtualMachineInitializationOptions
            {
                AdditionalDevices =
                [
                    _ => new CallbackInt(0x99, () => this.count++),
                    vm => new CallbackInt(0x9A,
                        () =>
                        {
                            this.count++;
                            if(this.count == 500)
                                this.vm.PhysicalMemory.SetUInt16(this.vm.Processor.SS, (ushort)(this.vm.Processor.SP + 4), 0);
                        }),
                    vm => new CallbackInt(0x9B, () =>
                        {
                            this.count++;
                            if(this.count == 500)
                            this.vm.PhysicalMemory.SetUInt16(this.vm.Processor.SS, (ushort)(this.vm.Processor.SP + 4), (ushort)EFlags.Zero);
                        })
                ]
            }
        );

        Reset();
    }
    [TestCleanup]
    public void TestCleanup()
    {
        this.vm.Dispose();
    }

    [TestMethod]
    public void Loop()
    {
        vm.WriteCode(
            "CD 99",    // int 99h
            "E2 FC",    // loop -4
            "CD 20"     // int 20h
            );

        this.vm.TestEmulator(5000);
        Assert.AreEqual(1000, this.count);
    }
    [TestMethod]
    public void Loope()
    {
        vm.WriteCode(
            "CD 9A",    // int 9Ah
            "E1 FC",    // loope -4
            "CD 20"     // int 20h
            );

        this.vm.Processor.Flags.Zero = true;

        this.vm.TestEmulator(5000);

        Assert.AreEqual(500, this.count);
    }
    [TestMethod]
    public void Loopne()
    {
        vm.WriteCode(
            "CD 9B",    // int 9Bh
            "E0 FC",    // loopne -4
            "CD 20"     // int 20h
            );

        this.vm.Processor.Flags.Zero = false;

        this.vm.TestEmulator(5000);

        Assert.AreEqual(500, this.count);
    }

    private void Reset()
    {
        this.count = 0;
        this.vm.Processor.ECX = 1000;
        this.vm.WriteSegmentRegister(SegmentIndex.CS, CS);
        this.vm.Processor.EIP = EIP;
    }
}
