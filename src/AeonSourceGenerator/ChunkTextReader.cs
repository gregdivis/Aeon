#nullable enable

namespace Aeon.SourceGenerator
{
    internal sealed class ChunkTextReader : TextReader
    {
        private readonly Queue<string> chunks;
        private string? current;
        private int pos;

        public ChunkTextReader(Queue<string> chunks) => this.chunks = chunks;

        public override int Peek()
        {
            if (!this.EnsureCurrent())
                return -1;

            return this.current![this.pos];
        }
        public override int Read()
        {
            if (!this.EnsureCurrent())
                return -1;

            return this.current![this.pos++];
        }
        public override int Read(char[] buffer, int index, int count)
        {
            if (count == 0 || !this.EnsureCurrent())
                return 0;

            count = Math.Min(count, this.current!.Length - this.pos);
            this.current.CopyTo(this.pos, buffer, index, count);
            this.pos += count;
            return count;
        }

        private bool EnsureCurrent()
        {
            if (this.current == null || this.pos >= this.current.Length)
            {
                if (this.chunks.Count == 0)
                    return false;

                this.current = this.chunks.Dequeue();
                this.pos = 0;
            }

            return true;
        }
    }
}
