namespace Aeon.Emulator.Instructions.FPU
{
    internal static class SaveRestore
    {
        //http://www.mathemainzel.info/files/x86asmref.html

        [Opcode("DD/6 fulladdr:rmw", Name = "fnsave", OperandSize = 16 | 32, AddressSize = 16)]
        public static void Save(VirtualMachine vm, uint offset)
        {
            var fpu = vm.Processor.FPU;

            //https://www.website.masmforum.com/tutorials/fptute/fpuchap3.htm#fsave
            unsafe
            {
                var buffer = (byte*)vm.PhysicalMemory.GetSafePointer(offset, 94);
                *(ushort*)&buffer[0] = fpu.ControlWord;
                *(ushort*)&buffer[2] = fpu.StatusWord;
                *(ushort*)&buffer[4] = fpu.TagWord;
                *(ushort*)&buffer[6] = vm.Processor.IP;
                *(ushort*)&buffer[8] = vm.Processor.CS;
                *(ushort*)&buffer[10] = 0; // this is not correct
                *(ushort*)&buffer[12] = vm.Processor.DS;

                for (int i = 0; i < 8; i++)
                    *(Real10*)&buffer[(i * 10) + 14] = fpu.GetRegisterValue(i);
            }

            fpu.Reset();
        }

        [Alternate(nameof(Save), AddressSize = 32, OperandSize = 16 | 32)]
        public static void Save32(VirtualMachine vm, uint offset)
        {
            var fpu = vm.Processor.FPU;

            //https://www.website.masmforum.com/tutorials/fptute/fpuchap3.htm#fsave
            unsafe
            {
                var buffer = (byte*)vm.PhysicalMemory.GetSafePointer(offset, 108);
                *(ushort*)&buffer[0] = fpu.ControlWord;
                *(ushort*)&buffer[4] = fpu.StatusWord;
                *(ushort*)&buffer[8] = fpu.TagWord;
                *(uint*)&buffer[12] = vm.Processor.EIP;
                *(ushort*)&buffer[16] = vm.Processor.CS;
                *(uint*)&buffer[20] = offset;
                *(ushort*)&buffer[24] = vm.Processor.DS;

                for (int i = 0; i < 8; i++)
                    *(Real10*)&buffer[(i * 10) + 28] = fpu.GetRegisterValue(i);
            }

            fpu.Reset();
        }

        [Opcode("DD/4 fulladdr:rmw", Name = "frstor", OperandSize = 16 | 32, AddressSize = 16)]
        public static void Restore(VirtualMachine vm, uint offset)
        {
            var fpu = vm.Processor.FPU;

            unsafe
            {
                var buffer = (byte*)vm.PhysicalMemory.GetSafePointer(offset, 94);
                fpu.ControlWord = *(ushort*)&buffer[0];
                fpu.StatusWord = *(ushort*)&buffer[2];
                fpu.TagWord = *(ushort*)&buffer[4];

                for (int i = 0; i < 8; i++)
                    fpu.SetRegisterValue(i, (double)*(Real10*)&buffer[(i * 10) + 14]);
            }
        }
        [Alternate(nameof(Restore), OperandSize = 16 | 32, AddressSize = 32)]
        public static void Restore32(VirtualMachine vm, uint offset)
        {
            var fpu = vm.Processor.FPU;

            unsafe
            {
                var buffer = (byte*)vm.PhysicalMemory.GetSafePointer(offset, 108);
                fpu.ControlWord = *(ushort*)&buffer[0];
                fpu.StatusWord = *(ushort*)&buffer[4];
                fpu.TagWord = *(ushort*)&buffer[8];

                for (int i = 0; i < 8; i++)
                    fpu.SetRegisterValue(i, (double)*(Real10*)&buffer[(i * 10) + 28]);
            }
        }
    }
}
