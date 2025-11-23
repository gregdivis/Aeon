namespace Aeon.Emulator.Video.Rendering;

/// <summary>
/// Renders emulated video RAM data to a bitmap.
/// </summary>
public abstract class Presenter : IDisposable
{
    private Scaler? scaler;
    private MemoryBitmap? internalBuffer;
    private readonly Lock syncLock = new();
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Presenter"/> class.
    /// </summary>
    /// <param name="videoMode"><see cref="Video.VideoMode"/> instance describing the video mode.</param>
    protected Presenter(VideoMode videoMode)
    {
        this.VideoMode = videoMode;
    }

    /// <summary>
    /// Gets or sets the scaler used on the output.
    /// </summary>
    public ScalingAlgorithm Scaler
    {
        get
        {
            return this.scaler switch
            {
                Scale2x => ScalingAlgorithm.Scale2x,
                Scale3x => ScalingAlgorithm.Scale3x,
                _ => ScalingAlgorithm.None
            };
        }
        set
        {
            if (this.Scaler == value)
                return;

            if (value != ScalingAlgorithm.None && this.internalBuffer == null)
            {
                this.internalBuffer = new MemoryBitmap(this.VideoMode.PixelWidth, this.VideoMode.PixelHeight);
            }
            else
            {
                this.internalBuffer?.Dispose();
                this.internalBuffer = null;
            }

            this.scaler = value switch
            {
                ScalingAlgorithm.Scale2x => new Scale2x(this.VideoMode.PixelWidth, this.VideoMode.PixelHeight),
                ScalingAlgorithm.Scale3x => new Scale3x(this.VideoMode.PixelWidth, this.VideoMode.PixelHeight),
                _ => null
            };
        }
    }
    /// <summary>
    /// Gets the required pixel width of the render target.
    /// </summary>
    public int TargetWidth => this.scaler?.TargetWidth ?? this.VideoMode.PixelWidth;
    /// <summary>
    /// Gets the required pixel height of the render target.
    /// </summary>
    public int TargetHeight => this.scaler?.TargetHeight ?? this.VideoMode.PixelHeight;
    /// <summary>
    /// Gets the width ratio of the output if a scaler is being used; otherwise 1.
    /// </summary>
    public int WidthRatio => this.scaler?.WidthRatio ?? 1;
    /// <summary>
    /// Gets the height ratio of the output if a scaler is being used; otherwise 1.
    /// </summary>
    public int HeightRatio => this.scaler?.HeightRatio ?? 1;

    public TimeSpan RenderTime { get; private set; }
    public TimeSpan ScalerTime { get; private set; }

    /// <summary>
    /// Gets information about the video mode.
    /// </summary>
    protected VideoMode VideoMode { get; }

    /// <summary>
    /// Updates the bitmap to match the current state of the video RAM.
    /// </summary>
    public void Update(IntPtr destination)
    {
        lock (this.syncLock)
        {
            if (this.scaler == null)
            {
                this.DrawFrame(destination);
            }
            else
            {
                this.DrawFrame(this.internalBuffer!.PixelBuffer);
                this.scaler.Apply(this.internalBuffer.PixelBuffer, destination);
            }
        }
    }

    public virtual MemoryBitmap? Dump() => null;

    /// <summary>
    /// Updates the bitmap to match the current state of the video RAM.
    /// </summary>
    protected abstract void DrawFrame(IntPtr destination);

    public void Dispose()
    {
        lock (this.syncLock)
        {
            if (!this.disposed)
            {
                this.internalBuffer?.Dispose();
                this.disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
