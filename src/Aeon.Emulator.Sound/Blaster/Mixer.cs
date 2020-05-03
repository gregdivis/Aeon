using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aeon.Emulator.Sound.Blaster
{
    internal sealed class Mixer
    {
        private readonly SoundBlaster blaster;

        public Mixer(SoundBlaster blaster)
        {
            this.blaster = blaster;
        }

        public int CurrentAddress { get; set; }
        public InterruptStatus InterruptStatusRegister { get; set; }

        public byte ReadData()
        {
            switch(this.CurrentAddress)
            {
            case MixerRegisters.InterruptStatus:
                return (byte)this.InterruptStatusRegister;

            case MixerRegisters.IRQ:
                return GetIRQByte();

            case MixerRegisters.DMA:
                return GetDMAByte();

            default:
                System.Diagnostics.Debug.WriteLine(string.Format("Unsupported mixer register {0:X2}h", this.CurrentAddress));
                return 0;
            }
        }

        private byte GetIRQByte()
        {
            switch(this.blaster.IRQ)
            {
            case 2:
                return 1 << 0;

            case 5:
                return 1 << 1;

            case 7:
                return 1 << 2;
                
            case 10:
                return 1 << 3;
            }

            return 0;
        }
        private byte GetDMAByte()
        {
            return (byte)(1 << this.blaster.DMA);
        }
    }
}
