namespace Aeon.Emulator.Video.Rendering;

public abstract class VideoRenderer
{
    private protected VideoRenderer(VideoMode mode) => this.Mode = mode;

    public VideoMode Mode { get; }
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
