using Aeon.Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test
{
    [TestClass]
    public class Addressing16
    {
        private const ushort CS = 0x1000;
        private const uint EIP = 0;

        private VirtualMachine vm;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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

        #region Mod = 0
        [TestMethod]
        public void BXSI0()
        {
            vm.WriteCode(
                "8D 00",    // lea ax,[bx+si]
                "CD 20");

            Run();

            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.SI), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BXDI0()
        {
            vm.WriteCode(
                "8D 01",    // lea ax,[bx+di]
                "CD 20");

            Run();

            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.DI), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BPSI0()
        {
            vm.WriteCode(
                "8D 02",    // lea ax,[bp+si]
                "CD 20");

            Run();

            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.SI), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BPDI0()
        {
            vm.WriteCode(
                "8D 03",    // lea ax,[bp+di]
                "CD 20");

            Run();

            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.DI), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void SI0()
        {
            vm.WriteCode(
                "8D 04",    // lea ax,[si]
                "CD 20");

            Run();

            Assert.AreEqual(vm.Processor.SI, (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void DI0()
        {
            vm.WriteCode(
                "8D 05",    // lea ax,[di]
                "CD 20");

            Run();

            Assert.AreEqual(vm.Processor.DI, (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void DispOnly()
        {
            vm.WriteCode(
                "8D 06 F725",    // lea ax,[F725]
                "CD 20");

            Run();

            Assert.AreEqual((ushort)0xF725, (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BX0()
        {
            vm.WriteCode(
                "8D 07",    // lea ax,[bx]
                "CD 20");

            Run();

            Assert.AreEqual((ushort)vm.Processor.BX, (ushort)vm.Processor.AX);
        }
        #endregion

        #region Mod = 1
        [TestMethod]
        public void BXSI1()
        {
            vm.WriteCode(
                "8D 40 10",    // lea ax,[bx+si+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.SI + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 40 FF",    // lea ax,[bx+si-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.SI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BXDI1()
        {
            vm.WriteCode(
                "8D 41 10",    // lea ax,[bx+di+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.DI + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 41 FF",    // lea ax,[bx+di-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.DI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BPSI1()
        {
            vm.WriteCode(
                "8D 42 10",    // lea ax,[bp+si+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.SI + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 42 FF",    // lea ax,[bp+si-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.SI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BPDI1()
        {
            vm.WriteCode(
                "8D 43 10",    // lea ax,[bp+di+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.DI + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 43 FF",    // lea ax,[bp+di-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.DI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void SI1()
        {
            vm.WriteCode(
                "8D 44 10",    // lea ax,[si+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.SI + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 44 FF",    // lea ax,[si-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.SI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void DI1()
        {
            vm.WriteCode(
                "8D 45 10",    // lea ax,[di+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.DI + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 45 FF",    // lea ax,[di-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.DI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BP1()
        {
            vm.WriteCode(
                "8D 46 10",    // lea ax,[bp+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 46 FF",    // lea ax,[bp-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BX1()
        {
            vm.WriteCode(
                "8D 47 10",    // lea ax,[bx+16]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + 0x10), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 47 FF",    // lea ax,[bx-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX - 1), (ushort)vm.Processor.AX);
        }
        #endregion

        #region Mod = 2
        [TestMethod]
        public void BXSI2()
        {
            vm.WriteCode(
                "8D 80 1000",    // lea ax,[bx+si+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.SI + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 80 FFFF",    // lea ax,[bx+si-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.SI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BXDI2()
        {
            vm.WriteCode(
                "8D 81 1000",    // lea ax,[bx+di+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.DI + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 81 FFFF",    // lea ax,[bx+di-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + vm.Processor.DI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BPSI2()
        {
            vm.WriteCode(
                "8D 82 1000",    // lea ax,[bp+si+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.SI + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 82 FFFF",    // lea ax,[bp+si-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.SI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BPDI2()
        {
            vm.WriteCode(
                "8D 83 1000",    // lea ax,[bp+di+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.DI + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 83 FFFF",    // lea ax,[bp+di-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + vm.Processor.DI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void SI2()
        {
            vm.WriteCode(
                "8D 84 1000",    // lea ax,[si+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.SI + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 84 FFFF",    // lea ax,[si-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.SI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void DI2()
        {
            vm.WriteCode(
                "8D 85 1000",    // lea ax,[di+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.DI + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 85 FFFF",    // lea ax,[di-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.DI - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BP2()
        {
            vm.WriteCode(
                "8D 86 1000",    // lea ax,[bp+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 86 FFFF",    // lea ax,[bp-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BP - 1), (ushort)vm.Processor.AX);
        }
        [TestMethod]
        public void BX2()
        {
            vm.WriteCode(
                "8D 87 1000",    // lea ax,[bx+4096]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX + 0x1000), (ushort)vm.Processor.AX);

            Reset();
            vm.WriteCode(
                "8D 87 FFFF",    // lea ax,[bx-1]
                "CD 20");

            Run();
            Assert.AreEqual((ushort)(vm.Processor.BX - 1), (ushort)vm.Processor.AX);
        }
        #endregion

        private void Run()
        {
            vm.TestEmulator(10);
        }
        private void Reset()
        {
            this.vm.WriteSegmentRegister(SegmentIndex.CS, CS);
            this.vm.Processor.EIP = EIP;
            this.vm.Processor.AX = 0;
            this.vm.Processor.BX = 719;
            this.vm.Processor.SI = 0xFFF4;
            this.vm.Processor.BP = 3;
            this.vm.Processor.DI = 582;
        }
    }
}
