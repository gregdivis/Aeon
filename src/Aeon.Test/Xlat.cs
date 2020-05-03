using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aeon.Emulator;

namespace Aeon.Test
{
    [TestClass]
    public class Xlat
    {
        private const ushort CS = 0x1000;
        private const uint EIP = 0;

        private VirtualMachine vm;
        private CpuState initialState;
        private byte[] testData;

        /// <summary>
        /// Initializes a new instance of the Xlat class.
        /// </summary>
        public Xlat()
        {
        }

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
                DS = 0x5000
            };

            var random = new Random();
            this.testData = new byte[ushort.MaxValue + 1];
            random.NextBytes(this.testData);
            for(int i = 0; i < testData.Length; i++)
            {
                if(this.testData[i] == 0)
                    this.testData[i] = 1;
            }

            vm.WriteBytes(this.initialState.DS, 0, this.testData);
            vm.SetState(this.initialState);
            vm.WriteSegmentRegister(SegmentIndex.CS, CS);
            vm.Processor.EIP = EIP;
        }
        [TestCleanup]
        public void TestCleanup()
        {
            this.vm.Dispose();
        }

        [TestMethod]
        public void Xlat16_0()
        {
            vm.WriteCode(
                "D7",       // xlat
                "CD 20"     // int 20h
                );

            vm.Processor.AL = 0;
            vm.Processor.BX = 0;

            vm.TestEmulator(10);

            Assert.AreEqual(this.testData[0], vm.Processor.AL);
        }

        [TestMethod]
        public void Xlat16_Rnd()
        {
            vm.WriteCode(
                "D7",       // xlat
                "CD 20"     // int 20h
                );

            var rnd = new Random();

            vm.Processor.AL = (byte)rnd.Next(256);
            vm.Processor.BX = (short)(ushort)rnd.Next(65536);

            int index = (ushort)((ushort)vm.Processor.BX + vm.Processor.AL);

            vm.TestEmulator(10);

            Assert.AreEqual(this.testData[index], vm.Processor.AL);
        }

        [TestMethod]
        public void Xlat16_Max()
        {
            vm.WriteCode(
                "D7",       // xlat
                "CD 20"     // int 20h
                );

            var rnd = new Random();

            vm.Processor.AL = 255;
            vm.Processor.BX = -1;

            int index = (ushort)((ushort)vm.Processor.BX + vm.Processor.AL);

            vm.TestEmulator(10);

            Assert.AreEqual(this.testData[index], vm.Processor.AL);
        }
    }
}
