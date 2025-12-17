namespace Aeon.Emulator.Video.Rendering;

internal abstract class PalettizedVideoRenderer<TPixelFormat>(VideoMode mode) : VideoRenderer(mode)
    where TPixelFormat : IOutputPixelFormat
{
    private readonly uint[] palette = new uint[256];

    protected abstract void RenderFrame(UnsafePointer<uint> palette, Span<uint> destination);
    protected sealed override void RenderFrame(Span<uint> destination)
    {
        TPixelFormat.ConvertBGRAPalette(this.Mode.Palette, this.palette);
        this.RenderFrame(new UnsafePointer<uint>(this.palette), destination);
    }
}
