using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Video.Rendering;

internal sealed class TextRenderer<TPixelFormat>(VirtualMachine vm) : PalettizedVideoRenderer<TPixelFormat>(vm.VideoMode!)
    where TPixelFormat : IOutputPixelFormat
{
    private readonly uint consoleWidth = (uint)vm.VideoMode!.Width;
    private readonly uint consoleHeight = (uint)vm.VideoMode.Height;
    private readonly uint fontHeight = (uint)vm.VideoMode!.FontHeight;
    private readonly byte[] font = vm.VideoMode!.Font;
    private readonly VirtualMachine vm = vm;
    private int cursorBlink;

    protected override void RenderFrame(UnsafePointer<uint> palette, Span<uint> destination)
    {
        var internalPalette = new UnsafePointer<byte>(this.Mode.InternalPalette);
        uint displayPage = (uint)this.Mode.ActiveDisplayPage;

        ref uint destPtr = ref destination[0];

        ref byte textPlane = ref Unsafe.Add(ref MemoryMarshal.AsRef<byte>(this.Mode.VideoRamSpan), VideoMode.DisplayPageSize * displayPage);
        ref byte attrPlane = ref Unsafe.Add(ref MemoryMarshal.AsRef<byte>(this.Mode.VideoRamSpan), VideoMode.PlaneSize + VideoMode.DisplayPageSize * displayPage);

        for (uint y = 0; y < this.consoleHeight; y++)
        {
            for (uint x = 0; x < this.consoleWidth; x++)
            {
                uint srcOffset = y * this.consoleWidth + x;
                ref uint dest = ref Unsafe.Add(ref destPtr, (y * this.consoleWidth * 8 * this.fontHeight) + (x * 8));
                byte attrValue = Unsafe.Add(ref attrPlane, srcOffset);
                DrawCharacter(ref dest, Unsafe.Add(ref textPlane, srcOffset), palette[internalPalette[attrValue & 0x0F]], palette[internalPalette[(attrValue >> 4) & 0x0F]]);
            }
        }

        if (this.vm.IsCursorVisible)
        {
            this.cursorBlink = (this.cursorBlink + 1) % 16;
            if (cursorBlink >= 8)
            {
                uint stride = this.consoleWidth * 8;
                uint color = palette[internalPalette[7]];

                var cursorPos = this.vm.CursorPosition;
                int destOffset = (int)(((uint)cursorPos.Y * stride * this.fontHeight) + ((uint)cursorPos.X * 8) + (stride * (this.fontHeight - 2)));
                if (destOffset < destination.Length - 8)
                {
                    destination.Slice(destOffset, 8).Fill(color);
                    destination.Slice(destOffset + (int)stride, 8).Fill(color);
                }
            }
        }
    }

    private void DrawCharacter(ref uint dest, byte index, uint foregroundColor, uint backgroundColor)
    {
        if (Vector.IsHardwareAccelerated)
        {
            ReadOnlySpan<uint> indexes = [1 << 7, 1 << 6, 1 << 5, 1 << 4, 1 << 3, 1 << 2, 1 << 1, 1 << 0];
            var indexVector = MemoryMarshal.Cast<uint, Vector<uint>>(indexes);
            var foregroundVector = new Vector<uint>(foregroundColor);
            var backgroundVector = new Vector<uint>(backgroundColor);

            for (int y = 0; y < this.fontHeight; y++)
            {
                byte current = this.font[(index * fontHeight) + y];
                var currentVector = new Vector<uint>(current);

                int x = 0;

                for (int i = 0; i < indexVector.Length; i++)
                {
                    var maskResult = Vector.BitwiseAnd(currentVector, indexVector[i]);
                    var equalsResult = Vector.Equals(maskResult, indexVector[i]);
                    var result = Vector.ConditionalSelect(equalsResult, foregroundVector, backgroundVector);
                    for (int j = 0; j < Vector<uint>.Count; j++)
                        Unsafe.Add(ref dest, x + j) = result[j];

                    x += Vector<uint>.Count;
                }

                dest = ref Unsafe.Add(ref dest, this.consoleWidth * 8);
            }
        }
        else
        {
            for (int y = 0; y < this.fontHeight; y++)
            {
                byte current = this.font[(index * fontHeight) + y];

                for (int x = 0; x < 8; x++)
                    Unsafe.Add(ref dest, x) = (current & (1 << (7 - x))) != 0 ? foregroundColor : backgroundColor;

                dest += this.consoleWidth * 8;
            }
        }
    }
}
