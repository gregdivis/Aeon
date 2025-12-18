using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aeon.Emulator.Video.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aeon.Emulator.Launcher;

public sealed class AeonGame : Game
{
    private SpriteBatch? _spriteBatch;
    private EmulatorHost emulator;
    private Texture2D? framebufferTexture;
    private Texture2D? messageFont;
    private VideoRenderTarget? renderTarget;
    private readonly Microsoft.Xna.Framework.Input.Keys[] pressedKeysBuffer = new Microsoft.Xna.Framework.Input.Keys[8];
    private readonly HashSet<Microsoft.Xna.Framework.Input.Keys> pressedKeys = [];
    private volatile int videoModeChanges;
    private long instructionCounterStartValue;
    private long instructionsPerSecond;
    private readonly Stopwatch ipsCounter = new();
    private MouseState previousMouseState;
    private bool mouseAbsoluteMode;
    private bool mouseCaptured;
    private bool hasFocus;
    private readonly bool showIps;
    private string? currentProcess;
    private bool currentProcessChanged;

    public AeonGame(EmulatorHost emulator, bool showIps)
    {
        this.emulator = emulator;
        _ = new GraphicsDeviceManager(this);
        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = false;
        this.showIps = showIps;
    }

    protected override void Initialize()
    {
        this.Window.Title = "Aeon";
        this.videoModeChanges = 1;
        this.UpdateVideoMode();
        this.emulator.VideoModeChanged += (s, e) => Interlocked.Increment(ref this.videoModeChanges);
        this.emulator.CurrentProcessChanged += (s, e) =>
        {
            this.currentProcess = this.emulator.VirtualMachine.CurrentProcess?.ImageName;
            this.currentProcessChanged = true;
        };

        this.currentProcess = this.emulator.VirtualMachine.CurrentProcess?.ImageName;
        this.currentProcessChanged = true;

        this.emulator.Run();

        base.Initialize();
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
        this.hasFocus = true;
        base.OnActivated(sender, args);
    }

    protected override void OnDeactivated(object sender, EventArgs args)
    {
        this.hasFocus = false;
        base.OnDeactivated(sender, args);
    }

    protected override void LoadContent() 
    {
        this._spriteBatch = new SpriteBatch(GraphicsDevice);
        this.messageFont = Texture2D.CreateEmuFont(this.GraphicsDevice, Fonts.VGA8x16);
    }

    private void RaiseMouseEvents(EmulatorHost emulator, MouseState p, MouseState c)
    {
        if (p.LeftButton == ButtonState.Released && c.LeftButton == ButtonState.Pressed)
            emulator.MouseEvent(new MouseButtonDownEvent(MouseButtons.Left));
        else if (p.LeftButton == ButtonState.Pressed && c.LeftButton == ButtonState.Released)
            emulator.MouseEvent(new MouseButtonUpEvent(MouseButtons.Left));

        if (p.MiddleButton == ButtonState.Released && c.MiddleButton == ButtonState.Pressed)
            emulator.MouseEvent(new MouseButtonDownEvent(MouseButtons.Middle));
        else if (p.MiddleButton == ButtonState.Pressed && c.MiddleButton == ButtonState.Released)
            emulator.MouseEvent(new MouseButtonUpEvent(MouseButtons.Middle));

        if (p.RightButton == ButtonState.Released && c.RightButton == ButtonState.Pressed)
            emulator.MouseEvent(new MouseButtonDownEvent(MouseButtons.Right));
        else if (p.RightButton == ButtonState.Pressed && c.RightButton == ButtonState.Released)
            emulator.MouseEvent(new MouseButtonUpEvent(MouseButtons.Right));

        if (this.renderTarget is not null && (p.X != c.X || p.Y != c.Y))
        {
            double xRatio = (double)this.renderTarget.Width / this.GraphicsDevice.Viewport.Width;
            double yRatio = (double)this.renderTarget.Height / this.GraphicsDevice.Viewport.Height;
            int virtualX = (int)(xRatio * c.X);
            int virtualY = (int)(yRatio * c.Y);

            if (this.mouseAbsoluteMode)
            {
                if (virtualX >= 0 && virtualX < emulator.VirtualMachine.VideoMode!.PixelWidth && virtualY >= 0 && virtualY < emulator.VirtualMachine.VideoMode.PixelHeight)
                    emulator.MouseEvent(new MouseMoveAbsoluteEvent(virtualX, virtualY));
            }
            else if (this.mouseCaptured)
            {
                int deltaX = virtualX - (this.renderTarget.Width / 2);
                int deltaY = virtualY - (this.renderTarget.Height / 2);
                emulator.MouseEvent(new MouseMoveRelativeEvent(deltaX, deltaY));
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(this.GraphicsDevice.Viewport.Width / 2, this.GraphicsDevice.Viewport.Height / 2);
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (this.emulator.State == EmulatorState.ProgramExited)
        {
            this.Exit();
            return;
        }

        if (this.currentProcessChanged)
        {
            var name = this.currentProcess;
            if (string.IsNullOrEmpty(name))
                this.Window.Title = "Aeon";
            else
                this.Window.Title = $"{name} - Aeon";
        }

        var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState(this.Window);
        if (this.previousMouseState != mouseState && (this.hasFocus || this.mouseAbsoluteMode))
        {
            RaiseMouseEvents(this.emulator, this.previousMouseState, mouseState);
            this.previousMouseState = mouseState;
        }

        if (this.hasFocus)
        {
            var state = Microsoft.Xna.Framework.Input.Keyboard.GetState();
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

            List<Microsoft.Xna.Framework.Input.Keys>? released = null;

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
        if (this.videoModeChanges > 0 || this.renderTarget is null || this.framebufferTexture is null)
        {
            this.UpdateVideoMode();
            Interlocked.Decrement(ref this.videoModeChanges);
        }

        this.renderTarget.Update();
        this.framebufferTexture.SetData(this.renderTarget.TargetData);

        this.GraphicsDevice.Clear(Color.Black);

        if (this._spriteBatch is not null)
        {
            this._spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
            this._spriteBatch.Draw(framebufferTexture,
                             destinationRectangle: new Rectangle(0, 0,
                                                                 GraphicsDevice.Viewport.Width,
                                                                 GraphicsDevice.Viewport.Height),
                             color: Color.White);

            if (this.showIps && this.instructionsPerSecond > 0)
                _spriteBatch.DrawString(this.messageFont!, $"IPS: {this.instructionsPerSecond:#,#}", new Vector2(GraphicsDevice.Viewport.Width - 180, GraphicsDevice.Viewport.Height - 20), Color.White);

            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }

    [MemberNotNull(nameof(renderTarget))]
    [MemberNotNull(nameof(framebufferTexture))]
    private void UpdateVideoMode()
    {
        var newRenderTarget = (this.emulator.VirtualMachine.GetRenderer<PixelFormatRGBA>()?.CreateRenderTarget())
            ?? throw new NotSupportedException("Video mode not supported.");

        if (this.framebufferTexture is null || newRenderTarget.Width != this.framebufferTexture.Width || newRenderTarget.Height != this.framebufferTexture.Height)
        {
            this.framebufferTexture?.Dispose();
            this.framebufferTexture = new Texture2D(this.GraphicsDevice, newRenderTarget.Width, newRenderTarget.Height, false, SurfaceFormat.Color);
        }

        this.renderTarget = newRenderTarget;
    }
}
