using System.Runtime.InteropServices;

namespace Aeon.Emulator.Video.Rendering;

public abstract class VideoRenderer(VideoMode mode)
{
    public VideoMode Mode { get; } = mode;
    public int Width => this.Mode.PixelWidth;
    public int Height => this.Mode.PixelHeight;

    internal static VideoRenderer? Create<TPixelFormat>(VirtualMachine vm)
        where TPixelFormat : IOutputPixelFormat
    {
        ArgumentNullException.ThrowIfNull(vm);
        if (vm.VideoMode is null)
            return null;

        var videoMode = vm.VideoMode;

        if (videoMode.VideoModeType == VideoModeType.Text)
        {
            return new TextRenderer<TPixelFormat>(vm);
        }
        else
        {
            return videoMode.BitsPerPixel switch
            {
                2 => new VideoRenderer2Bit<TPixelFormat>(videoMode),
                4 => new VideoRenderer4Bit<TPixelFormat>(videoMode),
                8 when videoMode.IsPlanar => new VideoRendererModeX<TPixelFormat>(videoMode),
                8 when !videoMode.IsPlanar => new VideoRenderer8Bit<TPixelFormat>(videoMode),
                16 => new VideoRenderer16Bit<TPixelFormat>(videoMode),
                _ => null
            };
        }

    }

    public VideoRenderTarget CreateRenderTarget() => new(this);
    public void Draw(Span<uint> destination) => this.RenderFrame(destination);

    protected abstract void RenderFrame(Span<uint> destination);
}

public sealed class VideoRenderTarget
{
    private readonly VideoRenderer renderer;

    internal VideoRenderTarget(VideoRenderer renderer)
    {
        this.renderer = renderer;
        this.TargetData = new byte[renderer.Width * renderer.Height * 4];
    }

    public int Width => this.renderer.Width;
    public int Height => this.renderer.Height;
    public byte[] TargetData { get; }

    public void Update()
    {
        this.renderer.Draw(MemoryMarshal.Cast<byte, uint>(this.TargetData.AsSpan()));
    }
}

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
