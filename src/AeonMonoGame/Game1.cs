using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading;
using Aeon.Emulator;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Sound.Blaster;
using Aeon.Emulator.Sound.FM;
using Aeon.Emulator.Sound.PCSpeaker;
using Aeon.Emulator.Video;
using Aeon.Emulator.Video.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AeonMonoGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private EmulatorHost emulator;
    private Presenter presenter;
    private Texture2D framebufferTexture;
    private SpriteFont messageFont;
    private byte[] framebuffer;
    private readonly Microsoft.Xna.Framework.Input.Keys[] pressedKeysBuffer = new Microsoft.Xna.Framework.Input.Keys[8];
    private readonly HashSet<Microsoft.Xna.Framework.Input.Keys> pressedKeys = [];
    private volatile int videoModeChanges;
    private long instructionCounterStartValue;
    private long instructionsPerSecond;
    private readonly Stopwatch ipsCounter = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        this.emulator = new EmulatorHost();
        this.videoModeChanges = 1;
        this.UpdateVideoMode();
        this.emulator.VideoModeChanged += (s, e) => Interlocked.Increment(ref this.videoModeChanges);
        var vm = this.emulator.VirtualMachine;
        vm.RegisterVirtualDevice(new InternalSpeaker());
        vm.RegisterVirtualDevice(new SoundBlaster(vm));
        vm.RegisterVirtualDevice(new FmSoundCard());

        this.emulator.VirtualMachine.FileSystem.Drives[DriveLetter.C].Mapping = new WritableMappedFolder(@"C:\DOS");
        this.emulator.VirtualMachine.FileSystem.Drives[DriveLetter.C].HasCommandInterpreter = true;
        this.emulator.VirtualMachine.FileSystem.Drives[DriveLetter.C].DriveType = DriveType.Fixed;
        this.emulator.EmulationSpeed = int.MaxValue;
        this.emulator.LoadProgram("COMMAND.COM");

        this.emulator.Run();

        base.Initialize();
    }

    private void Emulator_VideoModeChanged(object sender, EventArgs e) => this.UpdateVideoMode();

    protected override void LoadContent() 
    {
        this._spriteBatch = new SpriteBatch(GraphicsDevice);
        this.messageFont = this.Content.Load<SpriteFont>("PressStart2P");
    }

    protected override void Update(GameTime gameTime)
    {
        if (this.emulator.State == EmulatorState.ProgramExited)
        {
            this.Exit();
            return;
        }

        var state = Keyboard.GetState();
        int count = state.GetPressedKeyCount();
        if (count > 0)
        {
            state.GetPressedKeys(this.pressedKeysBuffer);
            for (int i = 0; i < count; i++)
            {
                var key = this.pressedKeysBuffer[i];
                if (this.pressedKeys.Add(key) && key.TryConvert(out var aeonKey))
                    this.emulator.PressKey(aeonKey);
            }
        }

        List<Microsoft.Xna.Framework.Input.Keys> released = null;

        foreach (var pressed in this.pressedKeys)
        {
            bool found = false;

            for (int i = 0; i < count; i++)
            {
                if (this.pressedKeysBuffer[i] == pressed)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                released ??= [];
                released.Add(pressed);
            }
        }

        if (released is not null)
        {
            foreach (var key in released)
            {
                this.pressedKeys.Remove(key);
                if (key.TryConvert(out var aeonKey))
                    this.emulator.ReleaseKey(aeonKey);
            }
        }

        if (!this.ipsCounter.IsRunning)
        {
            this.ipsCounter.Start();
            this.instructionCounterStartValue = this.emulator.TotalInstructions;
        }
        else if (this.ipsCounter.Elapsed >= TimeSpan.FromSeconds(1))
        {
            this.ipsCounter.Stop();
            this.instructionsPerSecond = (long)((this.emulator.TotalInstructions - this.instructionCounterStartValue) / this.ipsCounter.Elapsed.TotalSeconds);
            this.ipsCounter.Restart();
            this.instructionCounterStartValue = this.emulator.TotalInstructions;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (this.videoModeChanges > 0)
        {
            this.UpdateVideoMode();
            Interlocked.Decrement(ref this.videoModeChanges);
        }

        this.presenter.Update(this.framebuffer);
        SwapRedBlue(this.framebuffer);
        this.framebufferTexture.SetData(this.framebuffer);

        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _spriteBatch.Draw(framebufferTexture,
                         destinationRectangle: new Rectangle(0, 0,
                                                             GraphicsDevice.Viewport.Width,
                                                             GraphicsDevice.Viewport.Height),
                         color: Color.White);

        if (this.instructionsPerSecond > 0)
            _spriteBatch.DrawString(this.messageFont, $"IPS: {this.instructionsPerSecond:#,#}", new Vector2(GraphicsDevice.Viewport.Width - 300, GraphicsDevice.Viewport.Height - 20), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }



    [StructLayout(LayoutKind.Sequential, Size = 4)]
    private readonly struct Bgra
    {
        public Bgra(byte r, byte g, byte b, byte a)
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
            this.Alpha = a;
        }

        public readonly byte Red;
        public readonly byte Green;
        public readonly byte Blue;
        public readonly byte Alpha;
    }

    public static void SwapRedBlue(Span<byte> bgraBuffer)
    {
        if (Vector512.IsHardwareAccelerated && bgraBuffer.Length >= Vector512<byte>.Count)
        {
            var indexes = Vector512.Create(BitShufflePattern);
            var vectorBuffer = MemoryMarshal.Cast<byte, Vector512<int>>(bgraBuffer);
            for (int i = 0; i < vectorBuffer.Length; i++)
                vectorBuffer[i] = Vector512.Shuffle(vectorBuffer[i].AsByte(), indexes).AsInt32();
        }
        else if (Vector256.IsHardwareAccelerated && bgraBuffer.Length >= Vector256<byte>.Count)
        {
            var indexes = Vector256.Create(BitShufflePattern);
            var vectorBuffer = MemoryMarshal.Cast<byte, Vector256<int>>(bgraBuffer);
            for (int i = 0; i < vectorBuffer.Length; i++)
                vectorBuffer[i] = Vector256.Shuffle(vectorBuffer[i].AsByte(), indexes).AsInt32();
        }
        else if (Vector128.IsHardwareAccelerated && bgraBuffer.Length >= Vector128<byte>.Count)
        {
            var indexes = Vector128.Create(BitShufflePattern);
            var vectorBuffer = MemoryMarshal.Cast<byte, Vector128<int>>(bgraBuffer);
            for (int i = 0; i < vectorBuffer.Length; i++)
                vectorBuffer[i] = Vector128.Shuffle(vectorBuffer[i].AsByte(), indexes).AsInt32();
        }
        else
        {
            var uintBuffer = MemoryMarshal.Cast<byte, Bgra>(bgraBuffer);
            for (int i = 0; i < uintBuffer.Length; i++)
            {
                var value = uintBuffer[i];
                uintBuffer[i] = new Bgra(value.Blue, value.Green, value.Red, value.Alpha);
            }
        }
    }

    private static ReadOnlySpan<byte> BitShufflePattern =>
    [
        2, 1, 0, 3,
        6, 5, 4, 7,
        10, 9, 8, 11,
        14, 13, 12, 15,
        18, 17, 16, 19,
        22, 21, 20, 23,
        26, 25, 24, 27,
        30, 29, 28, 31,
        34, 33, 32, 35,
        38, 37, 36, 39,
        42, 41, 40, 43,
        46, 45, 44, 47,
        50, 49, 48, 51,
        54, 53, 52, 55,
        58, 57, 56, 59,
        62, 61, 60, 63
    ];

    private void UpdateVideoMode()
    {
        this.presenter?.Dispose();
        this.presenter = this.GetPresenter(this.emulator.VirtualMachine.VideoMode);
        if (this.framebufferTexture is null || this.presenter.TargetWidth != this.framebufferTexture.Width || this.presenter.TargetHeight != this.framebufferTexture.Height)
        {
            this.framebufferTexture?.Dispose();
            this.framebufferTexture = new Texture2D(this.GraphicsDevice, this.presenter.TargetWidth, this.presenter.TargetHeight, false, SurfaceFormat.Color);
            if (this.framebuffer is null || this.framebuffer.Length != this.presenter.TargetWidth * this.presenter.TargetHeight * 4)
                this.framebuffer = new byte[this.presenter.TargetWidth * this.presenter.TargetHeight * 4];
        }
    }

    private Presenter GetPresenter(VideoMode videoMode)
    {
        if (this.emulator is null)
            return null;

        if (videoMode.VideoModeType == VideoModeType.Text)
        {
            return new TextPresenter(videoMode) { VirtualMachine = this.emulator.VirtualMachine };
        }
        else
        {
            return videoMode.BitsPerPixel switch
            {
                2 => new GraphicsPresenter2(videoMode),
                4 => new GraphicsPresenter4(videoMode),
                8 when videoMode.IsPlanar => new GraphicsPresenterX(videoMode),
                8 when !videoMode.IsPlanar => new GraphicsPresenter8(videoMode),
                16 => new GraphicsPresenter16(videoMode),
                _ => null
            };
        }
    }
}
