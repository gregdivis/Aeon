using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aeon.Emulator;

namespace Aeon.Test
{
    [TestClass]
    public class Conditionals
    {
        private const ushort CS = 0x1000;
        private const uint EIP = 0;

        private VirtualMachine vm;
        private bool branch;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.vm = new VirtualMachine();
            this.vm.RegisterVirtualDevice(new CallbackInt(0x99, () => this.branch = true));

            Reset();
        }
        [TestCleanup]
        public void TestCleanup()
        {
            this.vm.Dispose();
        }

        [TestMethod]
        public void Jo()
        {
            vm.WriteCode(
                "70 02",    // jo +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);

            Assert.IsFalse(this.branch);

            this.vm.WriteSegmentRegister(SegmentIndex.CS, CS);
            this.vm.Processor.EIP = EIP;
            this.vm.Processor.Flags.Overflow = true;

            this.vm.TestEmulator(10);

            Assert.IsTrue(this.branch);
        }
        [TestMethod]
        public void Jno()
        {
            vm.WriteCode(
                "71 02",    // jno +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);

            Assert.IsTrue(this.branch);

            this.vm.WriteSegmentRegister(SegmentIndex.CS, CS);
            this.vm.Processor.EIP = EIP;
            this.vm.Processor.Flags.Overflow = true;
            this.branch = false;

            this.vm.TestEmulator(10);

            Assert.IsFalse(this.branch);
        }

        [TestMethod]
        public void Jc()
        {
            vm.WriteCode(
                "72 02",    // jc +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Carry = true;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);
        }
        [TestMethod]
        public void Jnc()
        {
            vm.WriteCode(
                "73 02",    // jnc +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Carry = true;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }

        [TestMethod]
        public void Jz()
        {
            vm.WriteCode(
                "74 02",    // jz +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Zero = true;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);
        }
        [TestMethod]
        public void Jnz()
        {
            vm.WriteCode(
                "75 02",    // jnz +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Zero = true;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }

        [TestMethod]
        public void Jcz()
        {
            vm.WriteCode(
                "76 02",    // jcz +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);

            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Carry;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Carry | EFlags.Zero;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);
        }
        [TestMethod]
        public void Jncz()
        {
            vm.WriteCode(
                "77 02",    // jcz +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);

            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Carry;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Carry | EFlags.Zero;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }

        [TestMethod]
        public void Js()
        {
            vm.WriteCode(
                "78 02",    // js +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Sign = true;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);
        }
        [TestMethod]
        public void Jns()
        {
            vm.WriteCode(
                "79 02",    // jns +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Sign = true;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }

        [TestMethod]
        public void Jl()
        {
            vm.WriteCode(
                "7C 02",    // jl +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Overflow;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign | EFlags.Overflow;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }
        [TestMethod]
        public void Jge()
        {
            vm.WriteCode(
                "7D 02",    // jge +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Overflow;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign | EFlags.Overflow;

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);
        }

        [TestMethod]
        public void Jle()
        {
            vm.WriteCode(
                "7E 02",    // jle +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign | EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero | EFlags.Sign;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero | EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero | EFlags.Sign | EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);
        }
        [TestMethod]
        public void Jg()
        {
            vm.WriteCode(
                "7F 02",    // jg +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Sign | EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero | EFlags.Sign;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero | EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);

            Reset();
            this.vm.Processor.Flags.Value = EFlags.Zero | EFlags.Sign | EFlags.Overflow;
            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }

        [TestMethod]
        public void Jcxz()
        {
            vm.WriteCode(
                "E3 02",    // jcxz +2
                "CD 20",    // int 20h
                "CD 99",    // int 99h
                "CD 20"     // int 20h
                );

            this.vm.TestEmulator(10);
            Assert.IsTrue(this.branch);

            Reset();
            this.vm.Processor.CX = 1;

            this.vm.TestEmulator(10);
            Assert.IsFalse(this.branch);
        }

        private void Reset()
        {
            this.vm.WriteSegmentRegister(SegmentIndex.CS, CS);
            this.vm.Processor.EIP = EIP;
            this.vm.Processor.Flags.Value = 0;
            this.vm.Processor.CX = 0;
            this.branch = false;
        }
    }
}
