using MeltySynth;
using TinyAudio;

#nullable enable

namespace Aeon.Emulator.Sound
{
    internal sealed class MeltySynthMidiMapper : MidiDevice
    {
        private readonly Synthesizer synthesizer;
        private readonly AudioPlayer audioPlayer;
        private bool disposed;

        public MeltySynthMidiMapper(string soundFontPath)
        {
            if (string.IsNullOrEmpty(soundFontPath))
                throw new ArgumentNullException(nameof(soundFontPath));

            this.audioPlayer = Audio.CreatePlayer(true);
            this.synthesizer = new Synthesizer(soundFontPath, this.audioPlayer.Format.SampleRate);
            this.audioPlayer.BeginPlayback(this.HandleBufferNeeded);
        }

        public override void Pause()
        {
            this.audioPlayer.StopPlayback();
        }
        public override void Resume()
        {
            this.audioPlayer.BeginPlayback(this.HandleBufferNeeded);
        }

        protected override void PlayShortMessage(uint message)
        {
            this.synthesizer.ProcessMidiMessage((int)message & 0xF, (int)message & 0xF0, (byte)(message >>> 8), (byte)(message >>> 16));
        }
        protected override void PlaySysex(ReadOnlySpan<byte> data)
        {
        }
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (!disposing)
                    this.audioPlayer.Dispose();

                this.disposed = true;
            }

            base.Dispose(disposing);
        }

        private void HandleBufferNeeded(Span<float> buffer, out int samplesWritten)
        {
            this.synthesizer.RenderInterleaved(buffer);
            samplesWritten = buffer.Length;
        }
    }
}
