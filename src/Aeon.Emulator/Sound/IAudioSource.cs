namespace Aeon.Emulator.Sound;

/// <summary>
/// Implemented by devices that generate audio data.
/// </summary>
public interface IAudioSource
{
    /// <summary>
    /// Gets a value indicating whether the audio source is currently active.
    /// </summary>
    bool Active { get; }
    /// <summary>
    /// Gets or sets the output sample rate expected by the <see cref="RenderAudio(Span{float}, Span{float})"/> method.
    /// </summary>
    int OutputSampleRate { get; set; }

    /// <summary>
    /// Fills the specified buffers with audio samples.
    /// </summary>
    /// <param name="left">Left channel.</param>
    /// <param name="right">Right channel.</param>
    /// <remarks>
    /// Mono sources should write the same value to <paramref name="left"/> and <paramref name="right"/>.
    /// </remarks>
    void RenderAudio(Span<float> left, Span<float> right);
}
