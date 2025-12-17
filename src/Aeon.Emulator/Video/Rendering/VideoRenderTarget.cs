using System.Runtime.InteropServices;

namespace Aeon.Emulator.Video.Rendering;

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
