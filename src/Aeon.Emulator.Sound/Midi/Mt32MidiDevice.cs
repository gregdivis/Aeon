using System;

namespace Aeon.Emulator.Sound
{
    internal sealed class Mt32MidiDevice : MidiDevice
    {
        private readonly Lazy<Mt32Player> player;
        private bool disposed;

        public Mt32MidiDevice(string romsPath)
        {
            if (string.IsNullOrWhiteSpace(romsPath))
                throw new ArgumentNullException(nameof(romsPath));

            this.player = new Lazy<Mt32Player>(() => new(romsPath));
        }

        public override void Pause()
        {
            if (this.player.IsValueCreated)
                this.player.Value.Pause();
        }

        public override void Resume()
        {
            if (this.player.IsValueCreated)
                this.player.Value.Resume();
        }

        protected override void PlayShortMessage(uint message) => this.player.Value.PlayShortMessage(message);
        protected override void PlaySysex(ReadOnlySpan<byte> data) => this.player.Value.PlaySysex(data);

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && this.player.IsValueCreated)
                    this.player.Value.Dispose();

                this.disposed = true;
            }
        }
    }
}
