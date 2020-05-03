using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aeon.Emulator.Video.Vesa
{
    internal static class Functions
    {
        public const byte ReturnVBEControllerInformation = 0x00;
        public const byte ReturnSVGAModeInformation = 0x01;
        public const byte SetSVGAVideoMode = 0x02;
        public const byte MemoryWindowControl = 0x05;
        public const byte DisplayStartControl = 0x07;
    }
}
