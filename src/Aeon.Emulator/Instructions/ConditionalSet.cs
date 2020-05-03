namespace Aeon.Emulator.Instructions
{
    internal static class ConditionalSet
    {
        [Opcode("0F90 rmb", Name = "seto", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetO(Processor p, out byte value)
        {
            // Set byte if OF = 1
            if (p.Flags.Overflow)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F91 rmb", Name = "setno", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetNO(Processor p, out byte value)
        {
            // Set byte if OF = 0
            if (!p.Flags.Overflow)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F92 rmb", Name = "setb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetB(Processor p, out byte value)
        {
            // Set byte if CF = 1
            value = (byte)(p.Flags.Carry ? 1 : 0);
        }

        [Opcode("0F93 rmb", Name = "setae", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetAE(Processor p, out byte value)
        {
            // Set byte if CF = 0
            if (!p.Flags.Carry)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F94 rmb", Name = "sete", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetE(Processor p, out byte value)
        {
            // Set byte if ZF = 1
            if (p.Flags.Zero)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F95 rmb", Name = "setne", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetNE(Processor p, out byte value)
        {
            // Set byte if ZF = 0
            if (!p.Flags.Zero)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F96 rmb", Name = "setbe", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetBE(Processor p, out byte value)
        {
            // Set byte if CF = 1 or ZF = 1
            if (p.Flags.Carry || p.Flags.Zero)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F97 rmb", Name = "seta", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetA(Processor p, out byte value)
        {
            // Set byte if CF = 0 and ZF = 0
            if (!p.Flags.Carry && !p.Flags.Zero)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F98 rmb", Name = "sets", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetS(Processor p, out byte value)
        {
            // Set byte if SF = 0
            if (p.Flags.Sign)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F99 rmb", Name = "setns", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetNS(Processor p, out byte value)
        {
            // Set byte if SF = 0
            if (!p.Flags.Sign)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F9A rmb", Name = "setp", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetP(Processor p, out byte value)
        {
            // Set byte if PF = 1
            if (p.Flags.Parity)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F9B rmb", Name = "setpo", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetPO(Processor p, out byte value)
        {
            // Set byte if PF = 0
            if (!p.Flags.Parity)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F9C rmb", Name = "setl", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetL(Processor p, out byte value)
        {
            // Set byte if SF != OF
            if (p.Flags.Sign != p.Flags.Overflow)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F9D rmb", Name = "setge", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetGE(Processor p, out byte value)
        {
            // Set byte if SF = OF
            if (p.Flags.Sign == p.Flags.Overflow)
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F9E rmb", Name = "setle", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetLE(Processor p, out byte value)
        {
            // Set byte if ZF = 1 or SF != OF
            if (p.Flags.Zero || (p.Flags.Sign != p.Flags.Overflow))
                value = 1;
            else
                value = 0;
        }

        [Opcode("0F9F rmb", Name = "setnle", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void SetNLE(Processor p, out byte value)
        {
            // Set byte if ZF = 0 and SF = OF
            if (!p.Flags.Zero && (p.Flags.Sign == p.Flags.Overflow))
                value = 1;
            else
                value = 0;
        }
    }
}
