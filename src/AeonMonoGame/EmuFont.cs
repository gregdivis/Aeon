using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aeon.Emulator.Launcher;

internal static class EmuFontExtensions
{
    extension(SpriteBatch spriteBatch)
    {
        public void DrawString(Texture2D font, ReadOnlySpan<char> text, int x, int y) => spriteBatch.DrawString(font, text, new Vector2(x, y), Color.White);
        public void DrawString(Texture2D font, ReadOnlySpan<char> text, Vector2 position) => spriteBatch.DrawString(font, text, position, Color.White);
        public void DrawString(Texture2D font, ReadOnlySpan<char> text, Vector2 position, Color color)
        {
            ArgumentNullException.ThrowIfNull(spriteBatch);
            ArgumentNullException.ThrowIfNull(font);

            for (int i = 0; i < text.Length; i++)
            {
                int index = char.IsAscii(text[i]) ? text[i] : '?';
                spriteBatch.Draw(font, position, new Rectangle(index % 16 * 8, index / 16 * 16, 8, 16), color);
                position.X += 8;
            }
        }
    }

    extension(Texture2D)
    {
        public static Texture2D CreateEmuFont(GraphicsDevice graphicsDevice, ReadOnlySpan<byte> bitmask)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);

            var fontData = new uint[256 * 8 * 16];
            int pitch = 16 * 8;

            for (int c = 0; c < 256; c++)
            {
                int x = c % 16;
                int y = c / 16;
                int destOffset = (pitch * y * 16) + (x * 8);
                WriteTileTexture(bitmask.Slice(c * 16, 16), fontData.AsSpan(destOffset), pitch);
            }

            var fontTexture = new Texture2D(graphicsDevice, 16 * 8, 16 * 16);
            fontTexture.SetData(fontData);
            return fontTexture;
        }
    }

    private static void WriteTileTexture(ReadOnlySpan<byte> src, Span<uint> dest, int pitch)
    {
        for (int y = 0; y < 16; y++)
        {
            uint row = src[y];

            for (int x = 0; x < 8; x++)
            {
                if ((row & (1 << (7 - x))) != 0)
                    dest[(y * pitch) + x] = 0xFFFFFFFFu;
            }
        }
    }
}
