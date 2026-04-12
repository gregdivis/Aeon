using System.Collections.Immutable;

namespace Aeon.Emulator.Sound;

public sealed class AudioMixer
{
    private ImmutableList<IAudioSource> sources = [];
    private float[] mixL = new float[4096];
    private float[] mixR = new float[4096];
    private float[] tempL = new float[4096];
    private float[] tempR = new float[4096];

    internal AudioMixer()
    {
    }

    public int SampleRate
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                foreach (var source in this.sources)
                    source.OutputSampleRate = value;
            }
        }
    } = 44100;

    public void RegisterSource(IAudioSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ImmutableInterlocked.Update(ref this.sources, static (sources, s) => sources.Add(s), source);
    }
    public void UnregisterSource(IAudioSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ImmutableInterlocked.Update(ref this.sources, static (sources, s) => sources.Remove(s), source);
    }

    public void FillBuffer(Span<float> buffer)
    {
        if (buffer.IsEmpty)
            return;

        int framesRequested = buffer.Length / 2;

        this.GetBuffers(framesRequested, out var mixL, out var mixR, out var tempL, out var tempR);

        mixL.Clear();
        mixR.Clear();

        foreach (var source in this.sources)
        {
            if (source.Active)
            {
                tempL.Clear();
                tempR.Clear();

                source.RenderAudio(tempL, tempR);

                for (int i = 0; i < tempL.Length; i++)
                {
                    mixL[i] += tempL[i];
                    mixR[i] += tempR[i];
                }
            }
        }

        for (int i = 0; i < mixL.Length; i++)
        {
            float l = Math.Clamp(mixL[i], -1f, 1f);
            float r = Math.Clamp(mixR[i], -1f, 1f);

            int index = i * 2;
            buffer[index] = l;
            buffer[index + 1] = r;
        }
    }

    private void GetBuffers(int length, out Span<float> mixL, out Span<float> mixR, out Span<float> tempL, out Span<float> tempR)
    {
        if (length > this.mixL.Length)
        {
            this.mixL = new float[length];
            this.mixR = new float[length];
            this.tempL = new float[length];
            this.tempR = new float[length];
        }

        mixL = this.mixL.AsSpan(0, length);
        mixR = this.mixR.AsSpan(0, length);
        tempL = this.tempL.AsSpan(0, length);
        tempR = this.tempR.AsSpan(0, length);
    }
}
