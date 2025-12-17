using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Aeon.Emulator.Video.Rendering;

namespace Aeon.Emulator.Launcher;

public sealed partial class EmulatorDisplay : ContentControl
{
    private static readonly DependencyPropertyKey EmulatorStatePropertyKey = DependencyProperty.RegisterReadOnly(nameof(EmulatorState), typeof(EmulatorState), typeof(EmulatorDisplay), new PropertyMetadata(EmulatorState.NoProgram));
    private static readonly DependencyPropertyKey IsMouseCursorCapturedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsMouseCursorCaptured), typeof(bool), typeof(EmulatorDisplay), new PropertyMetadata(false));
    private static readonly DependencyPropertyKey CurrentProcessPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CurrentProcess), typeof(Dos.DosProcess), typeof(EmulatorDisplay), new PropertyMetadata(null));

    public static readonly DependencyProperty EmulatorStateProperty = EmulatorStatePropertyKey.DependencyProperty;
    public static readonly DependencyProperty CurrentProcessProperty = CurrentProcessPropertyKey.DependencyProperty;
    public static readonly DependencyProperty MouseInputModeProperty = DependencyProperty.Register(nameof(MouseInputMode), typeof(MouseInputMode), typeof(EmulatorDisplay), new PropertyMetadata(MouseInputMode.Relative));
    public static readonly DependencyProperty IsMouseCursorCapturedProperty = IsMouseCursorCapturedPropertyKey.DependencyProperty;
    public static readonly DependencyProperty EmulationSpeedProperty = DependencyProperty.Register(nameof(EmulationSpeed), typeof(int), typeof(EmulatorDisplay), new PropertyMetadata(20_000_000, OnEmulationSpeedChanged), EmulationSpeedChangedValidate);
    public static readonly DependencyProperty IsAspectRatioLockedProperty = DependencyProperty.Register(nameof(IsAspectRatioLocked), typeof(bool), typeof(EmulatorDisplay), new PropertyMetadata(true, OnIsAspectRatioLockedChanged));
    public static readonly DependencyProperty ScalingAlgorithmProperty = DependencyProperty.Register(nameof(ScalingAlgorithm), typeof(ScalingAlgorithm), typeof(EmulatorDisplay), new PropertyMetadata(ScalingAlgorithm.None, OnScalingAlgorithmChanged));
    public static readonly RoutedEvent EmulatorStateChangedEvent = EventManager.RegisterRoutedEvent(nameof(EmulatorStateChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EmulatorDisplay));
    public static readonly RoutedEvent EmulationErrorEvent = EventManager.RegisterRoutedEvent(nameof(EmulationError), RoutingStrategy.Bubble, typeof(EmulationErrorRoutedEventHandler), typeof(EmulatorDisplay));
    public static readonly RoutedEvent CurrentProcessChangedEvent = EventManager.RegisterRoutedEvent(nameof(CurrentProcessChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EmulatorDisplay));
    public static readonly RoutedCommand FullScreenCommand = new();

    private EmulatorHost emulator;
    private bool mouseJustCaptured;
    private bool isMouseCaptured;
    private Point centerPoint;
    private DispatcherTimer timer;
    private readonly EventHandler updateHandler;
    private readonly SimpleCommand resumeCommand;
    private readonly SimpleCommand pauseCommand;
    private VideoRenderer aeonRenderer;
    private FastBitmap wpfRenderTarget;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmulatorDisplay"/> class.
    /// </summary>
    public EmulatorDisplay()
    {
        updateHandler = new EventHandler(this.GraphicalUpdate);
        this.resumeCommand = new SimpleCommand(() => this.EmulatorState == EmulatorState.Paused, () => { this.EmulatorHost.Run(); });
        this.pauseCommand = new SimpleCommand(() => this.EmulatorState == EmulatorState.Running, () => { this.EmulatorHost.Pause(); });
        this.InitializeComponent();
    }

    /// <summary>
    /// Occurs when the emulator's state has changed.
    /// </summary>
    public event RoutedEventHandler EmulatorStateChanged
    {
        add { this.AddHandler(EmulatorStateChangedEvent, value); }
        remove { this.RemoveHandler(EmulatorStateChangedEvent, value); }
    }
    /// <summary>
    /// Occurs when an error in emulation causes the emulator to halt.
    /// </summary>
    public event EmulationErrorRoutedEventHandler EmulationError
    {
        add { this.AddHandler(EmulationErrorEvent, value); }
        remove { this.RemoveHandler(EmulationErrorEvent, value); }
    }
    /// <summary>
    /// Occurs when the current process has changed.
    /// </summary>
    public event RoutedEventHandler CurrentProcessChanged
    {
        add { this.AddHandler(CurrentProcessChangedEvent, value); }
        remove { this.RemoveHandler(CurrentProcessChangedEvent, value); }
    }

    /// <summary>
    /// Gets or sets the emulator to display.
    /// </summary>
    public EmulatorHost EmulatorHost
    {
        get => this.emulator;
        set
        {
            if (!ReferenceEquals(this.emulator, value))
            {
                if (this.emulator is not null)
                {
                    this.emulator.VideoModeChanged -= this.HandleModeChange;
                    this.emulator.StateChanged -= this.Emulator_StateChanged;
                    this.emulator.Error -= this.Emulator_Error;
                    this.emulator.CurrentProcessChanged -= this.Emulator_CurrentProcessChanged;
                    this.timer.Stop();
                    this.emulator.Dispose();
                }

                this.emulator = value;

                if (this.emulator is not null)
                {
                    this.emulator.VideoModeChanged += this.HandleModeChange;
                    this.emulator.StateChanged += this.Emulator_StateChanged;
                    this.emulator.Error += this.Emulator_Error;
                    this.emulator.CurrentProcessChanged += this.Emulator_CurrentProcessChanged;
                    this.emulator.EmulationSpeed = this.EmulationSpeed;
                    this.timer.Start();
                    this.InitializePresenter();
                }
            }
        }
    }
    /// <summary>
    /// Gets the current state of the emulator.  This is a dependency property.
    /// </summary>
    public EmulatorState EmulatorState => (EmulatorState)this.GetValue(EmulatorStateProperty);
    /// <summary>
    /// Gets or sets a value indicating whether the correct aspect ratio is maintained. This is a dependency property.
    /// </summary>
    public bool IsAspectRatioLocked
    {
        get => (bool)this.GetValue(IsAspectRatioLockedProperty);
        set => this.SetValue(IsAspectRatioLockedProperty, value);
    }
    /// <summary>
    /// Gets or sets a value indicating the type of mouse input provided.  This is a dependency property.
    /// </summary>
    public MouseInputMode MouseInputMode
    {
        get => (MouseInputMode)this.GetValue(MouseInputModeProperty);
        set => this.SetValue(MouseInputModeProperty, value);
    }
    /// <summary>
    /// Gets a value indicating whether the emulator has captured mouse input.  This is a dependency property.
    /// </summary>
    public bool IsMouseCursorCaptured => (bool)this.GetValue(IsMouseCursorCapturedProperty);
    /// <summary>
    /// Gets or sets the emulation speed.  This is a dependency property.
    /// </summary>
    public int EmulationSpeed
    {
        get => (int)this.GetValue(EmulationSpeedProperty);
        set => this.SetValue(EmulationSpeedProperty, value);
    }
    /// <summary>
    /// Gets or sets the scaling algorithm. This is a dependency property.
    /// </summary>
    public ScalingAlgorithm ScalingAlgorithm
    {
        get => (ScalingAlgorithm)this.GetValue(ScalingAlgorithmProperty);
        set => this.SetValue(ScalingAlgorithmProperty, value);
    }
    /// <summary>
    /// Gets the BitmapSource used for rendering the output display.
    /// </summary>
    public BitmapSource DisplayBitmap => this.wpfRenderTarget?.InteropBitmap;
    /// <summary>
    /// Gets information about the current process. This is a dependency property.
    /// </summary>
    public Emulator.Dos.DosProcess CurrentProcess => (Emulator.Dos.DosProcess)this.GetValue(CurrentProcessProperty);
    /// <summary>
    /// Gets the command used to resume the emulator from a paused state.
    /// </summary>
    public ICommand ResumeCommand => this.resumeCommand;
    /// <summary>
    /// Gets the command used to pause the emulator.
    /// </summary>
    public ICommand PauseCommand => this.pauseCommand;

    protected override void OnInitialized(EventArgs e)
    {
        this.timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0 / 60.0), DispatcherPriority.Render, updateHandler, this.Dispatcher);
        base.OnInitialized(e);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.System && e.SystemKey == Key.Enter && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            FullScreenCommand.Execute(null, this);

        if (this.emulator != null && this.emulator.State == EmulatorState.Running)
        {
            if (e.Key == Key.F12 && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            {
                this.isMouseCaptured = false;
                this.SetValue(IsMouseCursorCapturedPropertyKey, false);
            }
            else
            {
                var key = e.Key != Key.System ? e.Key.ToEmulatorKey() : e.SystemKey.ToEmulatorKey();
                if (key != Keys.Null)
                    emulator.PressKey(key);
            }

            e.Handled = true;
        }

        base.OnKeyDown(e);
    }
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (this.emulator != null && this.emulator.State == EmulatorState.Running)
        {
            var key = e.Key != Key.System ? e.Key.ToEmulatorKey() : e.SystemKey.ToEmulatorKey();
            if (key != Keys.Null)
                this.emulator.ReleaseKey(key);

            e.Handled = true;
        }

        base.OnKeyUp(e);
    }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        this.isMouseCaptured = false;
        this.SetValue(IsMouseCursorCapturedPropertyKey, false);
        if (this.emulator != null && this.emulator.State == EmulatorState.Running)
            this.emulator.ReleaseAllKeys();

        base.OnLostKeyboardFocus(e);
    }
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent != null)
            throw new InvalidOperationException("EmulatorDisplay does not support content.");

        base.OnContentChanged(oldContent, newContent);
    }

    private void GraphicalUpdate(object sender, EventArgs e)
    {
        if (this.emulator != null)
        {
            var presenter = this.aeonRenderer;
            if (presenter == null)
                return;

            this.EnsureRenderTarget(presenter);

            presenter.Draw(this.wpfRenderTarget.PixelBufferSpan);
            this.wpfRenderTarget.InteropBitmap.Invalidate();
        }
    }
    private void HandleModeChange(object sender, EventArgs e) => this.InitializePresenter();
    private void InitializePresenter()
    {
        this.displayImage.Source = null;
        if (this.emulator == null)
            return;

        this.aeonRenderer = this.emulator.VirtualMachine.GetRenderer<PixelFormatBGRA>();
        this.EnsureRenderTarget(this.aeonRenderer);

        int pixelWidth = this.aeonRenderer.Width;
        int pixelHeight = this.aeonRenderer.Height;
        this.displayImage.Source = this.wpfRenderTarget.InteropBitmap;
        this.displayImage.Width = pixelWidth;
        this.displayImage.Height = pixelHeight;
        this.displayArea.Width = pixelWidth;
        this.displayArea.Height = pixelHeight;

        this.centerPoint.X = pixelWidth / 2;
        this.centerPoint.Y = pixelHeight / 2;
    }
    private void EnsureRenderTarget(VideoRenderer presenter)
    {
        if (this.wpfRenderTarget == null || presenter.Width != this.wpfRenderTarget.InteropBitmap.PixelWidth || presenter.Height != this.wpfRenderTarget.InteropBitmap.PixelHeight)
        {
            this.wpfRenderTarget?.Dispose();
            this.wpfRenderTarget = new FastBitmap(presenter.Width, presenter.Height);
        }
    }

    private static void OnEmulationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var obj = (EmulatorDisplay)d;
        if (obj.emulator != null)
            obj.emulator.EmulationSpeed = (int)e.NewValue;
    }
    private static bool EmulationSpeedChangedValidate(object value)
    {
        int n = (int)value;
        return n >= EmulatorHost.MinimumSpeed;
    }
    private static void OnIsAspectRatioLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var obj = (EmulatorDisplay)d;
        bool value = (bool)e.NewValue;
        obj.outerViewbox.Stretch = value ? Stretch.Uniform : Stretch.Fill;
    }
    private static void OnScalingAlgorithmChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var obj = (EmulatorDisplay)d;
        obj.InitializePresenter();
    }

    private void Emulator_StateChanged(object sender, EventArgs e)
    {
        if (this.emulator != null)
        {
            this.SetValue(EmulatorStatePropertyKey, this.emulator.State);
            this.resumeCommand.UpdateState();
            this.pauseCommand.UpdateState();
            this.RaiseEvent(new RoutedEventArgs(EmulatorStateChangedEvent));
        }
    }
    private void Emulator_Error(object sender, ErrorEventArgs e) => this.RaiseEvent(new EmulationErrorRoutedEventArgs(EmulationErrorEvent, e.Message));
    private void Emulator_CurrentProcessChanged(object sender, EventArgs e)
    {
        if (this.emulator != null)
            this.SetValue(CurrentProcessPropertyKey, this.emulator.VirtualMachine.CurrentProcess);
        else
            this.SetValue(CurrentProcessPropertyKey, null);

        this.RaiseEvent(new RoutedEventArgs(CurrentProcessChangedEvent));
    }
    private void DisplayImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (this.emulator != null && this.emulator.State == EmulatorState.Running)
        {
            if (!this.isMouseCaptured && this.MouseInputMode == MouseInputMode.Relative)
            {
                this.SetValue(IsMouseCursorCapturedPropertyKey, true);
                this.mouseJustCaptured = true;
                this.isMouseCaptured = true;

                this.centerPoint.X = displayImage.Width / 2;
                this.centerPoint.Y = displayImage.Height / 2;
                return;
            }

            var button = e.ChangedButton.ToEmulatorButtons();
            if (button != MouseButtons.None)
            {
                var mouseEvent = new MouseButtonDownEvent(button);
                this.emulator.MouseEvent(mouseEvent);
            }
        }
    }
    private void DisplayImage_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (this.emulator != null && this.emulator.State == EmulatorState.Running)
        {
            if (this.mouseJustCaptured)
            {
                this.mouseJustCaptured = false;
                return;
            }

            var button = e.ChangedButton.ToEmulatorButtons();
            if (button != MouseButtons.None)
            {
                var mouseEvent = new MouseButtonUpEvent(button);
                this.emulator.MouseEvent(mouseEvent);
            }
        }
    }
    private void DisplayImage_MouseMove(object sender, MouseEventArgs e)
    {
        if (this.emulator != null && this.emulator.State == EmulatorState.Running)
        {
            if (this.MouseInputMode == MouseInputMode.Absolute)
            {
                var pos = e.GetPosition(displayImage);
                this.emulator.MouseEvent(new MouseMoveAbsoluteEvent((int)(pos.X / 1), (int)(pos.Y / 1)));
            }
            else if (this.isMouseCaptured)
            {
                var deltaPos = System.Windows.Input.Mouse.GetPosition(this.displayImage);

                int dx = (int)(deltaPos.X - this.centerPoint.X) / 1;
                int dy = (int)(deltaPos.Y - this.centerPoint.Y) / 1;

                if (dx != 0 || dy != 0)
                {
                    this.emulator.MouseEvent(new MouseMoveRelativeEvent(dx, dy));
                    var p = this.displayImage.PointToScreen(centerPoint);
                    _ = NativeMethods.SetCursorPos((int)p.X, (int)p.Y);
                }
            }
        }
    }
}

public delegate void EmulationErrorRoutedEventHandler(object sender, EmulationErrorRoutedEventArgs e);
