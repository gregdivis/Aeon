namespace Aeon.Emulator.Sound.Blaster;

internal sealed class StatefulResampler
{
    private readonly Queue<float> inputQueue = new();
    private double phase = 0.0;
    private double step = 1.0;
    private float currentSample = 0f;
    private float nextSample = 0f;
    private bool hasCurrent = false;

    public void SetRates(int inputRate, int outputRate)
    {
        if (outputRate > 0)
            step = (double)inputRate / outputRate;
    }

    public void AddInput(ReadOnlySpan<float> samples)
    {
        foreach (var sample in samples)
            inputQueue.Enqueue(sample);
    }

    public void Resample(Span<float> output)
    {
        for (int i = 0; i < output.Length; i++)
        {
            if (!hasCurrent)
            {
                if (inputQueue.Count == 0)
                {
                    output[i] = 0f;
                    continue;
                }

                currentSample = inputQueue.Dequeue();
                hasCurrent = true;
            }

            if (inputQueue.Count > 0)
                nextSample = inputQueue.Peek();
            else
                nextSample = currentSample; // Hold last on underrun

            output[i] = currentSample * (1f - (float)phase) + nextSample * (float)phase;

            phase += step;
            while (phase >= 1.0)
            {
                phase -= 1.0;
                currentSample = nextSample;
                if (inputQueue.Count > 0)
                    inputQueue.Dequeue(); // Consume after advance
                else
                    hasCurrent = false;
            }
        }
    }

    public void Reset()
    {
        inputQueue.Clear();
        phase = 0.0;
        hasCurrent = false;
        currentSample = 0f;
        nextSample = 0f;
    }
}
