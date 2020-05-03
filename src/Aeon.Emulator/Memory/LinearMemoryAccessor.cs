using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Provides read/write access to emulated physical memory without paging.
    /// </summary>
    internal sealed class LinearMemoryAccessor : MemoryAccessor
    {
        public LinearMemoryAccessor(IntPtr rawView)
            : base(rawView)
        {
        }


        public override byte GetByte(uint address)
        {
            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    return RawView[address];
                }
            }
            else
            {
                return video.GetVramByte(address - PhysicalMemory.VramAddress);
            }
        }
        public override void SetByte(uint address, byte value)
        {
            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    RawView[address] = value;
                }
            }
            else
            {
                video.SetVramByte(address - PhysicalMemory.VramAddress, value);
            }
        }

        public override ushort GetUInt16(uint address)
        {
            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    return *(ushort*)(RawView + address);
                }
            }
            else
            {
                return video.GetVramWord(address - PhysicalMemory.VramAddress);
            }
        }
        public override void SetUInt16(uint address, ushort value)
        {
            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    *(ushort*)(RawView + address) = value;
                }
            }
            else
            {
                video.SetVramWord(address - PhysicalMemory.VramAddress, value);
            }
        }

        public override uint GetUInt32(uint address)
        {
            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    return *(uint*)(RawView + address);
                }
            }
            else
            {
                return video.GetVramDWord(address - PhysicalMemory.VramAddress);
            }
        }
        public override void SetUInt32(uint address, uint value)
        {
            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    *(uint*)(RawView + address) = value;
                }
            }
            else
            {
                video.SetVramDWord(address - PhysicalMemory.VramAddress, value);
            }
        }

        public override ulong GetUInt64(uint address)
        {
            unsafe
            {
                return *(ulong*)(RawView + address);
            }
        }
        public override void SetUInt64(uint address, ulong value)
        {
            unsafe
            {
                *(ulong*)(RawView + address) = value;
            }
        }

        public override float GetReal32(uint address)
        {
            unsafe
            {
                return *(float*)(RawView + address);
            }
        }
        public override void SetReal32(uint address, float value)
        {
            unsafe
            {
                *(float*)(RawView + address) = value;
            }
        }

        public override double GetReal64(uint address)
        {
            unsafe
            {
                return *(double*)(RawView + address);
            }
        }
        public override void SetReal64(uint address, double value)
        {
            unsafe
            {
                *(double*)(RawView + address) = value;
            }
        }

        public override Real10 GetReal80(uint address)
        {
            unsafe
            {
                return *(Real10*)(RawView + address);
            }
        }
        public override void SetReal80(uint address, Real10 value)
        {
            unsafe
            {
                *(Real10*)(RawView + address) = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override unsafe void FetchInstruction(uint address, byte* buffer)
        {
            *(ulong*)buffer = *(ulong*)(RawView + address);
            ((ulong*)buffer)[1] = ((ulong*)(RawView + address))[1];
        }

        public override unsafe void* GetSafePointer(uint address, uint size) => RawView + address;
    }
}
