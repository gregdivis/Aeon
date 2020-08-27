using System;
using System.Collections.Generic;

namespace Aeon.Emulator.Mouse
{
    /// <summary>
    /// Provides the emulated mouse driver interface.
    /// </summary>
    internal sealed class MouseHandler : IInterruptHandler
    {
        private const byte CallbackInterrupt = 0x45;

        private VirtualMachine vm;
        private MouseState currentState;
        private MouseState callbackState;
        private CallbackMask callbackMask;
        private ushort callbackSegment;
        private ushort callbackOffset;
        private CallbackMask reason;
        private int showCount;
        private int motionCounterX;
        private int motionCounterY;
        private int callbackMotionCounterX;
        private int callbackMotionCounterY;
        private int minX;
        private int maxX = 639;
        private int minY;
        private int maxY = 199;
        private int mickeyRatioX = 8;
        private int mickeyRatioY = 16;
        private readonly ButtonPressTracker buttonTracker = new ButtonPressTracker();

        /// <summary>
        /// Gets the current position of the mouse cursor.
        /// </summary>
        public Video.Point Position => new Video.Point(currentState.X, currentState.Y);

        /// <summary>
        /// Notifies the mouse handler that the mouse was moved and relative coordinates are specified.
        /// </summary>
        /// <param name="deltaX">Relative horizontal movement.</param>
        /// <param name="deltaY">Relative vertical movement.</param>
        public void MouseMoveRelative(int deltaX, int deltaY)
        {
            this.motionCounterX += deltaX;
            this.motionCounterY += deltaY;

            this.callbackMotionCounterX += deltaX;
            this.callbackMotionCounterY += deltaY;

            this.currentState.X += deltaX;
            this.currentState.Y += deltaY;

            this.MouseMoved();
        }
        /// <summary>
        /// Notifies the mouse handler that the mouse was moved and absolute coordinates are specified.
        /// </summary>
        /// <param name="newX">New horizontal position.</param>
        /// <param name="newY">New vertical position.</param>
        public void MouseMoveAbsolute(uint newX, uint newY)
        {
            this.motionCounterX += (int)newX - this.currentState.X;
            this.motionCounterY += (int)newY - this.currentState.Y;

            this.callbackMotionCounterX += (int)newX - this.currentState.X;
            this.callbackMotionCounterY += (int)newY - this.currentState.Y;

            this.currentState.X = (int)newX;
            this.currentState.Y = (int)newY;

            this.MouseMoved();
        }
        /// <summary>
        /// Notifies the mouse handler the a mouse button was pressed.
        /// </summary>
        /// <param name="buttons">Button which was pressed.</param>
        public void MouseButtonDown(MouseButtons buttons)
        {
            this.buttonTracker.ButtonPress(buttons, this.currentState.X, this.currentState.Y);

            this.currentState.PressedButtons |= buttons;
            var mask = GetCallbackReasonPress(buttons);

            if ((mask & this.callbackMask) != 0)
            {
                this.reason = mask;
                this.vm.RaiseInterrupt(CallbackInterrupt);
            }
        }
        /// <summary>
        /// Notifies the mouse handler that a mouse button was released.
        /// </summary>
        /// <param name="buttons">Button which was released.</param>
        public void MouseButtonUp(MouseButtons buttons)
        {
            this.buttonTracker.ButtonRelease(buttons, this.currentState.X, this.currentState.Y);

            this.currentState.PressedButtons &= ~buttons;
            var mask = GetCallbackReasonRelease(buttons);

            if ((mask & this.callbackMask) != 0)
            {
                this.reason = mask;
                this.vm.RaiseInterrupt(CallbackInterrupt);
            }
        }

        /// <summary>
        /// Calls the user-specified callback function.
        /// </summary>
        private void RaiseCallback()
        {
            int scaledX = GetScaledX(this.currentState.X);
            int scaledY = GetScaledY(this.currentState.Y);

            var p = this.vm.Processor;
            p.AX = (short)reason;
            p.BX = (short)currentState.PressedButtons;
            p.CX = (short)scaledX;
            p.DX = (short)scaledY;
            p.SI = (ushort)callbackMotionCounterX;
            p.DI = (ushort)callbackMotionCounterY;
            this.callbackState = currentState;
            this.reason = CallbackMask.Disabled;

            Instructions.Call.FarAbsoluteCall(vm, (uint)((this.callbackSegment << 16) | this.callbackOffset));
        }
        /// <summary>
        /// Performs common handling of a move event.
        /// </summary>
        private void MouseMoved()
        {
            this.currentState.X = Math.Max(this.currentState.X, 0);
            this.currentState.X = Math.Min(this.currentState.X, this.vm.VideoMode.Width - 1);
            this.currentState.Y = Math.Max(this.currentState.Y, 0);
            this.currentState.Y = Math.Min(this.currentState.Y, this.vm.VideoMode.Height - 1);

            if (callbackMask.HasFlag(CallbackMask.Move))
            {
                this.reason = CallbackMask.Move;
                this.vm.RaiseInterrupt(CallbackInterrupt);
            }

            this.vm.OnMouseMove(new MouseMoveEventArgs(this.currentState.X, this.currentState.Y));
        }
        /// <summary>
        /// Returns the virtual horizontal cursor position.
        /// </summary>
        /// <param name="realX">Actual horizontal position.</param>
        /// <returns>Virtual horizontal cursor position.</returns>
        private int GetScaledX(int realX)
        {
            int screenWidth = this.vm.VideoMode.Width;
            int virtualWidth = this.maxX - this.minX + 1;
            var ratio = virtualWidth / (double)screenWidth;
            return (int)(realX * ratio) + minX;
        }
        /// <summary>
        /// Returns the virtual vertical cursor position.
        /// </summary>
        /// <param name="realY">Actual vertical cursor position.</param>
        /// <returns>Virtual vertical cursor position.</returns>
        private int GetScaledY(int realY)
        {
            int screenHeight = this.vm.VideoMode.OriginalHeight;
            int virtualHeight = this.maxY - this.minY + 1;
            var ratio = virtualHeight / (double)screenHeight;
            return (int)(realY * ratio) + this.minY;
        }
        /// <summary>
        /// Returns the actual horizontal cursor position.
        /// </summary>
        /// <param name="scaledX">Scaled value to convert.</param>
        /// <returns>Actual horizontal cursor position.</returns>
        private int GetRealX(int scaledX)
        {
            int screenWidth = this.vm.VideoMode.Width;
            int virtualWidth = this.maxX - this.minX + 1;
            var ratio = screenWidth / (double)virtualWidth;
            return (int)((scaledX - this.minX) * ratio);
        }
        /// <summary>
        /// Returns the actual vertical cursor position.
        /// </summary>
        /// <param name="scaledX">Scaled value to convert.</param>
        /// <returns>Actual vertical cursor position.</returns>
        private int GetRealY(int scaledY)
        {
            int screenHeight = this.vm.VideoMode.Height;
            int virtualHeight = this.maxY - this.minY + 1;
            var ratio = (double)screenHeight / virtualHeight;
            return (int)((scaledY - this.minY) * ratio);
        }

        /// <summary>
        /// Converts a MouseButtons value to a corresponding CallbackMask for a button press.
        /// </summary>
        /// <param name="button">Button to convert.</param>
        /// <returns>Converted CallbackMask value.</returns>
        private static CallbackMask GetCallbackReasonPress(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => CallbackMask.LeftButtonDown,
                MouseButtons.Right => CallbackMask.RightButtonDown,
                MouseButtons.Middle => CallbackMask.MiddleButtonDown,
                _ => CallbackMask.Disabled,
            };
        }
        /// <summary>
        /// Converts a MouseButtons value to a corresponding CallbackMask for a button release.
        /// </summary>
        /// <param name="button">Button to convert.</param>
        /// <returns>Converted CallbackMask value.</returns>
        private static CallbackMask GetCallbackReasonRelease(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => CallbackMask.LeftButtonUp,
                MouseButtons.Right => CallbackMask.RightButtonUp,
                MouseButtons.Middle => CallbackMask.MiddleButtonUp,
                _ => CallbackMask.Disabled,
            };
        }

        private void HandleVideoModeChanged(object sender, EventArgs e) => this.SetDefaultMax();
        private void SetDefaultMax()
        {
            var mode = this.vm.VideoMode;
            if (mode != null)
            {
                this.maxX = mode.MouseWidth - 1;
                this.maxY = mode.PixelHeight - 1;
            }
        }

        IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => new[] { 0x33, new InterruptHandlerInfo(CallbackInterrupt, Registers.AX | Registers.BX | Registers.CX | Registers.DX | Registers.BP | Registers.SI | Registers.DI | Registers.DS | Registers.ES, false, true) };
        void IInterruptHandler.HandleInterrupt(int interrupt)
        {
            if (interrupt == CallbackInterrupt)
            {
                this.RaiseCallback();
                return;
            }

            var p = this.vm.Processor;

            switch ((ushort)p.AX)
            {
                case Functions.Reset:
                    p.AX = -1; // Indicates mouse installed.
                    p.BX = 3; // Three-button mouse.
                    this.showCount = 0;  // Mouse cursor invisible.
                    this.vm.IsMouseVisible = false;
                    this.minX = 0;
                    this.minY = 0;
                    this.SetDefaultMax();
                    this.callbackMotionCounterX = 0;
                    this.callbackMotionCounterY = 0;
                    this.mickeyRatioX = 8;
                    this.mickeyRatioX = 16;
                    this.callbackMask = CallbackMask.Disabled;
                    this.callbackSegment = 0;
                    this.callbackOffset = 0;
                    break;

                case Functions.SoftwareReset:
                    p.AX = -1; // Indicates mouse installed.
                    p.BX = 3; // Three-button mouse.
                    break;

                case Functions.EnableMouseDriver:
                    // Always return success.
                    p.AX = 0x20;
                    break;

                case Functions.ShowCursor:
                    showCount++;
                    if (showCount == 1)
                        vm.IsMouseVisible = true;
                    break;

                case Functions.HideCursor:
                    showCount--;
                    if (showCount == 0)
                        vm.IsMouseVisible = false;
                    break;

                case Functions.GetPositionAndStatus:
                    p.CX = (short)GetScaledX(currentState.X);
                    p.DX = (short)GetScaledY(currentState.Y);
                    p.BX = (short)currentState.PressedButtons;
                    break;

                case Functions.SetCursorPosition:
                    // Bad things can happen if we don't filter out the useless messages.
                    if (currentState.X != GetRealX(p.CX) || currentState.Y != GetRealY(p.DX))
                    {
                        currentState.X = GetRealX(p.CX);
                        currentState.Y = GetRealY(p.DX);
                        var args = new MouseMoveEventArgs(currentState.X, currentState.Y);
                        vm.OnMouseMove(args);
                        vm.OnMouseMoveByEmulator(args);
                    }
                    break;

                case Functions.SetHorizontalRange:
                    minX = p.CX;
                    maxX = p.DX;
                    break;

                case Functions.SetVerticalRange:
                    minY = p.CX;
                    maxY = p.DX;
                    break;

                case Functions.GetMotionCounters:
                    p.CX = (short)motionCounterX;
                    p.DX = (short)motionCounterY;
                    motionCounterX = 0;
                    motionCounterY = 0;
                    break;

                case Functions.SetCallbackParameters:
                    callbackMask = (CallbackMask)p.CX;
                    callbackSegment = p.ES;
                    callbackOffset = (ushort)p.DX;
                    break;

                case Functions.GetButtonPressData:
                    var pressInfo = buttonTracker.GetButtonPressInfo(p.BX);
                    p.BX = (short)(pressInfo.Count & 0x7FFFu);
                    p.CX = (short)GetScaledX(pressInfo.X);
                    p.DX = (short)GetScaledY(pressInfo.Y);
                    break;

                case Functions.GetButtonReleaseData:
                    var releaseInfo = buttonTracker.GetButtonReleaseInfo(p.BX);
                    p.BX = (short)(releaseInfo.Count & 0x7FFFu);
                    p.CX = (short)GetScaledX(releaseInfo.X);
                    p.DX = (short)GetScaledY(releaseInfo.Y);
                    break;

                case Functions.ExchangeCallbacks:
                    var newMask = (CallbackMask)p.CX;
                    var newSegment = p.ES;
                    var newOffset = (ushort)p.DX;

                    vm.Processor.CX = (short)callbackMask;
                    vm.WriteSegmentRegister(SegmentIndex.ES, callbackSegment);
                    vm.Processor.DX = (short)callbackOffset;

                    callbackMask = newMask;
                    callbackSegment = newSegment;
                    callbackOffset = newOffset;
                    break;

                case Functions.SetMickeyPixelRatio:
                    this.mickeyRatioX = (ushort)p.CX;
                    this.mickeyRatioY = (ushort)p.DX;
                    break;

                case Functions.GetDriverStateStorageSize:
                    p.AX = 0x200;
                    break;

                case Functions.SaveDriverState:
                case Functions.RestoreDriverState:
                    // Ignore this for now.
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Mouse function {p.AX:X2}h not implemented.");
                    break;
            }
        }

        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
            this.vm = vm;
            this.vm.VideoModeChanged += this.HandleVideoModeChanged;
        }
        void IDisposable.Dispose()
        {
        }
    }
}
