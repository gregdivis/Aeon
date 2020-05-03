using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Aeon.Emulator;
using Aeon.Emulator.DebugSupport;
using Aeon.Emulator.Video;
using Aeon.Presentation.Rendering;

namespace Aeon.Presentation
{
    /// <summary>
    /// Interaction logic for EmulatorDisplay.xaml
    /// </summary>
    public sealed partial class EmulatorDisplay : ContentControl
    {
        private static readonly DependencyPropertyKey EmulatorStatePropertyKey = DependencyProperty.RegisterReadOnly("EmulatorState", typeof(EmulatorState), typeof(EmulatorDisplay), new PropertyMetadata(EmulatorState.NoProgram));
        private static readonly DependencyPropertyKey IsMouseCursorCapturedPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseCursorCaptured", typeof(bool), typeof(EmulatorDisplay), new PropertyMetadata(false));
        private static readonly DependencyPropertyKey CurrentProcessPropertyKey = DependencyProperty.RegisterReadOnly("CurrentProcess", typeof(Aeon.Emulator.Dos.DosProcess), typeof(EmulatorDisplay), new PropertyMetadata(null));

        public static readonly DependencyProperty EmulatorStateProperty = EmulatorStatePropertyKey.DependencyProperty;
        public static readonly DependencyProperty CurrentProcessProperty = CurrentProcessPropertyKey.DependencyProperty;
        public static readonly DependencyProperty MouseInputModeProperty = DependencyProperty.Register("MouseInputMode", typeof(MouseInputMode), typeof(EmulatorDisplay), new PropertyMetadata(MouseInputMode.Relative));
        public static readonly DependencyProperty IsMouseCursorCapturedProperty = IsMouseCursorCapturedPropertyKey.DependencyProperty;
        public static readonly DependencyProperty EmulationSpeedProperty = DependencyProperty.Register("EmulationSpeed", typeof(int), typeof(EmulatorDisplay), new PropertyMetadata(20_000_000, OnEmulationSpeedChanged), EmulationSpeedChangedValidate);
        public static readonly DependencyProperty IsAspectRatioLockedProperty = DependencyProperty.Register("IsAspectRatioLocked", typeof(bool), typeof(EmulatorDisplay), new PropertyMetadata(true, OnIsAspectRatioLockedChanged));
        public static readonly DependencyProperty ScalingAlgorithmProperty = DependencyProperty.Register("ScalingAlgorithm", typeof(ScalingAlgorithm), typeof(EmulatorDisplay), new PropertyMetadata(ScalingAlgorithm.None, OnScalingAlgorithmChanged));
        public static readonly RoutedEvent EmulatorStateChangedEvent = EventManager.RegisterRoutedEvent("EmulatorStateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EmulatorDisplay));
        public static readonly RoutedEvent EmulationErrorEvent = EventManager.RegisterRoutedEvent("EmulationError", RoutingStrategy.Bubble, typeof(EmulationErrorRoutedEventHandler), typeof(EmulatorDisplay));
        public static readonly RoutedEvent CurrentProcessChangedEvent = EventManager.RegisterRoutedEvent("CurrentProcessChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EmulatorDisplay));
        public static readonly RoutedCommand FullScreenCommand = new RoutedCommand();

        private EmulatorHost emulator;
        private bool mouseJustCaptured;
        private bool isMouseCaptured;
        private System.Windows.Point centerPoint;
        private DispatcherTimer timer;
        private readonly EventHandler updateHandler;
        private int cursorBlink;
        private Emulator.Video.Point cursorPosition = new Emulator.Video.Point(0, 1);
        private readonly SimpleCommand resumeCommand;
        private readonly SimpleCommand pauseCommand;
        private Scaler currentScaler;
        private Presenter currentPresenter;
        private int physicalMemorySize = 16;

        /// <summary>
        /// Initializes a new instance of the EmulatorDisplay class.
        /// </summary>
        public EmulatorDisplay()
        {
            updateHandler = new EventHandler(GraphicalUpdate);
            this.resumeCommand = new SimpleCommand(() => this.EmulatorState == EmulatorState.Paused, () => { this.EmulatorHost.Run(); });
            this.pauseCommand = new SimpleCommand(() => this.EmulatorState == EmulatorState.Running, () => { this.EmulatorHost.Pause(); });

            InitializeComponent();
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
            get
            {
                if (this.emulator == null)
                {
                    this.emulator = new EmulatorHost(this.physicalMemorySize);
                    this.emulator.EventSynchronizer = new WpfSynchronizer(this.Dispatcher);
                    this.emulator.VideoModeChanged += this.HandleModeChange;
                    this.emulator.StateChanged += this.Emulator_StateChanged;
                    this.emulator.MouseVisibilityChanged += this.Emulator_MouseVisibilityChanged;
                    this.emulator.MouseMove += this.Emulator_MouseMove;
                    this.emulator.Error += this.Emulator_Error;
                    this.emulator.CurrentProcessChanged += this.Emulator_CurrentProcessChanged;

                    this.emulator.EmulationSpeed = this.EmulationSpeed;
                    this.timer.Start();
                    this.InitializePresenter();
                }

                return this.emulator;
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
        public BitmapSource DisplayBitmap => this.currentScaler?.Output.InteropBitmap;
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

        /// <summary>
        /// Disposes the current emulator and returns the control to its default state.
        /// </summary>
        public void ResetEmulator(int physicalMemory = 16)
        {
            this.physicalMemorySize = physicalMemory;
            if (this.emulator != null)
            {
                this.emulator.VideoModeChanged -= this.HandleModeChange;
                this.emulator.StateChanged -= this.Emulator_StateChanged;
                this.emulator.MouseVisibilityChanged -= this.Emulator_MouseVisibilityChanged;
                this.emulator.MouseMove -= this.Emulator_MouseMove;
                this.emulator.Error -= this.Emulator_Error;
                this.emulator.CurrentProcessChanged -= this.Emulator_CurrentProcessChanged;
                this.mouseImage.Visibility = Visibility.Collapsed;
                this.cursorRectangle.Visibility = Visibility.Collapsed;
                this.timer.Stop();

                this.emulator.Dispose();
                this.emulator = null;
            }
        }

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
                this.currentPresenter?.Update();
                this.currentScaler?.Apply();
                this.currentScaler?.Output.Invalidate();

                if (this.emulator.VirtualMachine.IsCursorVisible)
                {
                    this.cursorBlink = (this.cursorBlink + 1) % 16;
                    if (this.cursorBlink == 8)
                        this.cursorRectangle.Visibility = Visibility.Visible;
                    else if (cursorBlink == 0)
                        this.cursorRectangle.Visibility = Visibility.Collapsed;

                    var cursorPosition = this.emulator.VirtualMachine.CursorPosition;
                    if (cursorPosition != this.cursorPosition)
                    {
                        this.cursorPosition = cursorPosition;
                        Canvas.SetLeft(this.cursorRectangle, cursorPosition.X * 8);
                        Canvas.SetTop(this.cursorRectangle, (cursorPosition.Y * emulator.VirtualMachine.VideoMode.FontHeight) + emulator.VirtualMachine.VideoMode.FontHeight - 2);
                    }
                }
                else if (this.cursorRectangle.Visibility == Visibility.Visible)
                {
                    this.cursorRectangle.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void HandleModeChange(object sender, EventArgs e) => this.InitializePresenter();
        private void InitializePresenter()
        {
            this.currentScaler?.Dispose();
            this.displayImage.Source = null;
            this.currentPresenter = null;
            this.currentScaler = null;

            if (this.emulator == null)
                return;

            var videoMode = this.emulator.VirtualMachine.VideoMode;
            this.currentScaler = this.GetScaler(videoMode);
            this.currentPresenter = this.GetPresenter(videoMode, this.currentScaler);

            int pixelWidth = this.currentScaler.TargetWidth;
            int pixelHeight = this.currentScaler.TargetHeight;
            this.displayImage.Source = this.currentScaler.Output.InteropBitmap;
            this.displayImage.Width = pixelWidth;
            this.displayImage.Height = pixelHeight;
            this.displayArea.Width = pixelWidth;
            this.displayArea.Height = pixelHeight;

            this.centerPoint.X = pixelWidth / 2;
            this.centerPoint.Y = pixelHeight / 2;
        }
        private void MoveMouseCursor(int x, int y)
        {
            Canvas.SetLeft(mouseImage, x * this.currentScaler.WidthRatio);
            Canvas.SetTop(mouseImage, y * this.currentScaler.HeightRatio);
        }
        private Scaler GetScaler(VideoMode videoMode)
        {
            return this.ScalingAlgorithm switch
            {
                ScalingAlgorithm.Scale2x => new Scale2x(videoMode.PixelWidth, videoMode.PixelHeight),
                ScalingAlgorithm.Scale3x => new Scale3x(videoMode.PixelWidth, videoMode.PixelHeight),
                _ => new NopScaler(videoMode.PixelWidth, videoMode.PixelHeight)
            };
        }
        private Presenter GetPresenter(VideoMode videoMode, Scaler scaler)
        {
            if (this.emulator == null)
                return null;

            if (videoMode.VideoModeType == VideoModeType.Text)
            {
                return new TextPresenter(scaler.VideoModeRenderTarget, videoMode);
            }
            else
            {
                switch (videoMode.BitsPerPixel)
                {
                    case 4:
                        return new GraphicsPresenter4(scaler.VideoModeRenderTarget, videoMode);

                    case 8:
                        if (!videoMode.IsPlanar)
                            return new GraphicsPresenter8(scaler.VideoModeRenderTarget, videoMode);
                        else
                            return new GraphicsPresenterX(scaler.VideoModeRenderTarget, videoMode);

                    case 16:
                        return new GraphicsPresenter16(scaler.VideoModeRenderTarget, videoMode);
                }
            }

            return null;
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
        private void Emulator_MouseVisibilityChanged(object sender, EventArgs e)
        {
            this.mouseImage.Visibility = this.emulator.VirtualMachine.IsMouseVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        private void Emulator_MouseMove(object sender, MouseMoveEventArgs e) => this.MoveMouseCursor(e.X, e.Y);
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
                    this.emulator.MouseEvent(new MouseMoveAbsoluteEvent((int)(pos.X / this.currentScaler.WidthRatio), (int)(pos.Y / this.currentScaler.HeightRatio)));
                }
                else if (this.isMouseCaptured)
                {
                    var deltaPos = Mouse.GetPosition(this.displayImage);

                    int dx = (int)(deltaPos.X - this.centerPoint.X) / this.currentScaler.WidthRatio;
                    int dy = (int)(deltaPos.Y - this.centerPoint.Y) / this.currentScaler.HeightRatio;

                    if (dx != 0 || dy != 0)
                    {
                        this.emulator.MouseEvent(new MouseMoveRelativeEvent(dx, dy));
                        var p = this.displayImage.PointToScreen(centerPoint);
                        SafeNativeMethods.SetCursorPos((int)p.X, (int)p.Y);
                    }
                }
            }
        }
    }

    public delegate void EmulationErrorRoutedEventHandler(object sender, EmulationErrorRoutedEventArgs e);
}
