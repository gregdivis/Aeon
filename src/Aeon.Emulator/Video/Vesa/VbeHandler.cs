using System;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Video.Vesa
{
    /// <summary>
    /// Emulates SVGA VESA VBE functions.
    /// </summary>
    internal sealed class VbeHandler : ICallbackProvider
    {
        /// <summary>
        /// VBE capabilities: 8-bit DAC, no VGA support
        /// </summary>
        private const uint VbeCaps = 0x03;
        private const string OemString = "AeonVbe";
        private const string VendorName = "Aeon";
        private const string ProductName = "AeonVbe";
        private const string ProductRev = "2.0";

        private readonly VideoHandler videoHandler;
        private readonly VirtualMachine vm;
        private Modes.VesaWindowed windowedMode;
        private RealModeAddress windowFuncPtr;

        /// <summary>
        /// Initializes a new instance of the VbeHandler class.
        /// </summary>
        /// <param name="videoHandler">VideoHandler instance which owns this VbeHandler.</param>
        public VbeHandler(VideoHandler videoHandler)
        {
            this.vm = videoHandler.VirtualMachine;
            this.videoHandler = videoHandler;
        }

        /// <summary>
        /// Gets a value indicating whether the callback is hookable.
        /// </summary>
        bool ICallbackProvider.IsHookable => false;
        /// <summary>
        /// Sets the address of the callback function.
        /// </summary>
        RealModeAddress ICallbackProvider.CallbackAddress
        {
            set => this.windowFuncPtr = value;
        }

        /// <summary>
        /// Emulates the function in the AL register.
        /// </summary>
        public void HandleFunction()
        {
            switch (vm.Processor.AL)
            {
                case Functions.ReturnVBEControllerInformation:
                    GetControllerInfo();
                    break;

                case Functions.ReturnSVGAModeInformation:
                    GetModeInfo();
                    break;

                case Functions.SetSVGAVideoMode:
                    SetMode(vm.Processor.BX & 0x7FFF);
                    break;

                case Functions.MemoryWindowControl:
                    if (vm.Processor.BH == 0)
                        SetWindowPosition((ushort)vm.Processor.DX);
                    else if (vm.Processor.BH == 1)
                        GetWindowPosition();
                    else
                        throw new InvalidOperationException();
                    break;

                case Functions.DisplayStartControl:
                    if (vm.Processor.BH == 0)
                        SetDisplayStart(vm.Processor.CX, vm.Processor.DX);
                    else
                        throw new InvalidOperationException();
                    break;

                default:
                    throw new NotImplementedException();
                    //vm.Processor.AL = 0;
                    //vm.Processor.AH = 1;
                    //return;
            }

            // Success
            vm.Processor.AL = 0x4F;
        }
        /// <summary>
        /// Performs the callback action.
        /// </summary>
        void ICallbackProvider.InvokeCallback()
        {
            if (vm.Processor.BH == 0)
                SetWindowPosition((ushort)vm.Processor.DX);
            else if (vm.Processor.BH == 1)
                GetWindowPosition();
            else
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a 4-character string as a 4-byte System.UInt32 value.
        /// </summary>
        /// <param name="signature">4-character string to convert.</param>
        /// <returns>4-byte System.UInt32 value.</returns>
        private static uint MakeSignature(string signature)
        {
            return (byte)signature[0] | ((uint)signature[1] << 8) | ((uint)signature[2] << 16) | ((uint)signature[3] << 24);
        }

        /// <summary>
        /// Returns VBE controller information.
        /// </summary>
        private void GetControllerInfo()
        {
            unsafe
            {
                uint oemStringOffset = vm.Processor.DI;
                uint modeListOffset = vm.Processor.DI;

                var infoBlock = (VbeInfoBlock*)vm.PhysicalMemory.GetPointer(vm.Processor.ES, vm.Processor.DI).ToPointer();
                bool isVbe2 = infoBlock->VbeSignature == MakeSignature("VBE2");

                if (isVbe2)
                {
                    byte* ptr = (byte*)infoBlock;
                    for (int i = 0; i < 512; i++)
                        ptr[i] = 0;

                    modeListOffset += (uint)sizeof(VbeInfoBlock);
                    oemStringOffset += 256;
                }
                else
                {
                    oemStringOffset += (uint)sizeof(VbeInfoBlock);
                    modeListOffset += (uint)sizeof(VbeInfoBlock) + (uint)OemString.Length + 1u;
                }

                infoBlock->VbeSignature = MakeSignature("VESA");
                infoBlock->VbeVersion = 0x0200;
                infoBlock->Capabilities = VbeCaps;
                infoBlock->TotalMemory = VideoHandler.TotalVramBytes / 65536;
                infoBlock->OemStringPtr = WriteOemString(ref oemStringOffset, OemString);
                infoBlock->VideoModePtr = (uint)(vm.Processor.ES << 16) | modeListOffset;

                if (isVbe2)
                {
                    infoBlock->OemSoftwareRev = 0x0200;
                    infoBlock->OemVendorNamePtr = WriteOemString(ref oemStringOffset, VendorName);
                    infoBlock->OemProductNamePtr = WriteOemString(ref oemStringOffset, ProductName);
                    infoBlock->OemProductRevPtr = WriteOemString(ref oemStringOffset, ProductRev);
                }

                var modePtr = (ushort*)vm.PhysicalMemory.GetPointer(vm.Processor.ES, modeListOffset).ToPointer();
                modePtr[0] = 0x100; // 640x400x8
                modePtr[1] = 0x101; // 640x480x8
                modePtr[2] = 0xFFFF;
            }

            vm.Processor.AH = 0;
        }
        /// <summary>
        /// Returns information about a supported video mode.
        /// </summary>
        private void GetModeInfo()
        {
            if (vm.Processor.CX != 0x100 && vm.Processor.CX != 0x101)
            {
                vm.Processor.AH = 1;
                return;
            }

            unsafe
            {
                var modeInfo = (ModeInfoBlock*)vm.PhysicalMemory.GetPointer(vm.Processor.ES, vm.Processor.DI).ToPointer();

                var buffer = (byte*)modeInfo;
                for (int i = 0; i < 256; i++)
                    buffer[i] = 0;

                modeInfo->ModeAttributes = ModeAttributes.Supported | ModeAttributes.Reserved1 | ModeAttributes.Color | ModeAttributes.Graphics | ModeAttributes.LinearFrameBuffer;
                modeInfo->WinAAtrributes = WindowAttributes.Supported | WindowAttributes.Readable | WindowAttributes.Writeable;
                modeInfo->WinASegment = 0xA000;
                modeInfo->WinGranularity = 64;
                modeInfo->WinSize = 64;
                modeInfo->XResolution = 640;
                modeInfo->YResolution = (ushort)((vm.Processor.CX == 0x100) ? 400 : 480);
                modeInfo->XCharSize = 8;
                modeInfo->YCharSize = 16;
                modeInfo->NumberOfPlanes = 1;
                modeInfo->BitsPerPixel = 8;
                modeInfo->NumberOfBanks = 1;
                modeInfo->MemoryModel = MemoryModel.Unchained256;
                modeInfo->Reserved1 = 1;
                modeInfo->WinFuncPtr = (uint)this.windowFuncPtr.Offset | ((uint)this.windowFuncPtr.Segment << 16);
                modeInfo->BytesPerScanLine = 640;

                modeInfo->PhysicalBasePointer = Modes.VesaLinear.BaseAddress;
                modeInfo->OffscreenMemoryOffset = (uint)modeInfo->XResolution * (uint)modeInfo->YResolution;
                modeInfo->OffscreenMemorySize = (ushort)((VideoHandler.TotalVramBytes - modeInfo->OffscreenMemoryOffset) / 1024u);
            }

            vm.Processor.AH = 0;
        }
        /// <summary>
        /// Sets the current display mode.
        /// </summary>
        /// <param name="mode">Index of the display mode.</param>
        private void SetMode(int mode)
        {
            VideoMode videoMode = mode switch
            {
                0x100 => this.windowedMode = new Modes.VesaWindowed256(640, 400, this.videoHandler),
                0x101 => this.windowedMode = new Modes.VesaWindowed256(640, 480, this.videoHandler),
                0x111 => this.windowedMode = new Modes.VesaWindowed16Bit(640, 480, this.videoHandler),
                _ => throw new NotSupportedException()
            };

            this.videoHandler.SetDisplayMode(videoMode);
            vm.Processor.AH = 0;
        }
        /// <summary>
        /// Sets the VBE window position.
        /// </summary>
        /// <param name="position">New window position in granularity units.</param>
        private void SetWindowPosition(uint position)
        {
            if (this.windowedMode == null)
            {
                vm.Processor.AH = 0x03;
                return;
            }

            this.windowedMode.WindowPosition = position;
            vm.Processor.AH = 0;
        }
        /// <summary>
        /// Gets the VBE window position.
        /// </summary>
        private void GetWindowPosition()
        {
            if (this.windowedMode == null)
                throw new InvalidOperationException();

            vm.Processor.DX = (short)this.windowedMode.WindowPosition;
            vm.Processor.AH = 0;
        }
        /// <summary>
        /// Sets the top-left corner of the display area.
        /// </summary>
        /// <param name="firstPixel">Horizontal position.</param>
        /// <param name="scanLine">Vertical position.</param>
        private void SetDisplayStart(int firstPixel, int scanLine)
        {
            if (this.windowedMode == null)
            {
                vm.Processor.AH = 1;
                return;
            }

            this.windowedMode.SetDisplayStart(firstPixel, scanLine);
            vm.Processor.AH = 0;
        }
        /// <summary>
        /// Writes an OEM string, advances the offset, and returns the address.
        /// </summary>
        /// <param name="offset">The offset to write the string.</param>
        /// <param name="value">The string to write.</param>
        /// <returns>Address of the string.</returns>
        private uint WriteOemString(ref uint offset, string value)
        {
            uint address = offset | ((uint)vm.Processor.ES << 16);

            vm.PhysicalMemory.SetString(vm.Processor.ES, offset, value);
            offset += (uint)value.Length + 1u;

            return address;
        }
    }
}
