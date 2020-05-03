namespace Aeon.Emulator.Instructions.BitShifting
{
    internal static class Shld
    {
        [Opcode("0FA4/r rmw,rw,ib|0FA5/r rmw,rw,cl", AddressSize = 16 | 32)]
        public static void Shld16(Processor p, ref ushort dest, ushort src, byte count)
        {
            int actualCount = count % 32;

            if (actualCount == 0)
                return;

            if (actualCount > 16)
            {
                dest = (ushort)(src << (32 - actualCount));
                return;
            }

            int result = (dest << actualCount) | (src >> (16 - (actualCount % 16)));
            if ((result & 0x00010000) != 0)
                p.Flags.Carry = true;
            else
                p.Flags.Carry = false;

            if (count == 1)
            {
                if (((result ^ dest) & 0x8000) == 0x8000)
                    p.Flags.Overflow = true;
                else
                    p.Flags.Overflow = false;
            }

            p.Flags.Update_Value_Word((ushort)(uint)result);
            dest = (ushort)(uint)result;
        }
        [Alternate(nameof(Shld16), AddressSize = 16 | 32)]
        public static void Shld32(Processor p, ref uint dest, uint src, byte count)
        {
            int actualCount = count % 32;

            if (actualCount == 0)
                return;

            uint result = (dest << actualCount) | (src >> (32 - actualCount));
            if ((dest & (1 << (actualCount - 1))) != 0)
                p.Flags.Carry = true;
            else
                p.Flags.Carry = false;

            if (count == 1)
            {
                if (((result ^ dest) & 0x80000000) == 0x80000000)
                    p.Flags.Overflow = true;
                else
                    p.Flags.Overflow = false;
            }

            p.Flags.Update_Value_DWord(result);
            dest = result;
        }
    }

    internal static class Shrd
    {
        [Opcode("0FAC/r rmw,rw,ib|0FAD/r rmw,rw,cl", AddressSize = 16 | 32)]
        public static void Shrd16(Processor p, ref ushort dest, ushort src, byte count)
        {
            int actualCount = count % 32;

            if (actualCount == 0)
                return;

            if (actualCount > 16)
            {
                dest = (ushort)(src >> (32 - actualCount));
                return;
            }

            int result;
            if (actualCount == 1)
            {
                if ((dest & 1) != 0)
                    p.Flags.Carry = true;
                else
                    p.Flags.Carry = false;

                result = (dest >> 1) | (src << 15);

                if (((result ^ dest) & 0x8000) == 0x8000)
                    p.Flags.Overflow = true;
                else
                    p.Flags.Overflow = false;
            }
            else
            {
                result = (dest >> (actualCount - 1)) | (src << (16 - (actualCount - 1)));
                if ((dest & 1) != 0)
                    p.Flags.Carry = true;
                else
                    p.Flags.Carry = false;

                result >>= 1;
            }

            dest = (ushort)result;
        }

        [Alternate(nameof(Shrd16), AddressSize = 16 | 32)]
        public static void Shrd32(Processor p, ref uint dest, uint src, byte count)
        {
            int actualCount = count % 32;

            if (actualCount == 0)
                return;

            long result;
            if (actualCount == 1)
            {
                if ((dest & 1) != 0)
                    p.Flags.Carry = true;
                else
                    p.Flags.Carry = false;

                result = ((long)dest >> 1) | ((long)src << 31);

                if (((result ^ dest) & 0x80000000) == 0x80000000)
                    p.Flags.Overflow = true;
                else
                    p.Flags.Overflow = false;
            }
            else
            {
                result = ((long)dest >> (actualCount - 1)) | ((long)src << (32 - (actualCount - 1)));
                if ((dest & 1) != 0)
                    p.Flags.Carry = true;
                else
                    p.Flags.Carry = false;

                result >>= 1;
            }

            dest = (uint)result;
        }
    }
}
