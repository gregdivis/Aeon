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
        private readonly ButtonPressTracker buttonTracker = new ButtonPressTracker();

        /// <summary>
        /// Gets the current position of the mouse cursor.
        /// </summary>
        public Video.Point Position => new Video.Point(currentState.X, currentState.Y);
        /// <summary>
        /// Gets or sets a value indicating whether the default virtual width should be used.
        /// </summary>
        public bool UseDefaultVirtualWidth { get; set; } = true;

        /// <summary>
        /// Notifies the mouse handler that the mouse was moved and relative coordinates are specified.
        /// </summary>
        /// <param name="deltaX">Relative horizontal movement.</param>
        /// <param name="deltaY">Relative vertical movement.</param>
        public void MouseMoveRelative(int deltaX, int deltaY)
        {
            motionCounterX += deltaX;
            motionCounterY += deltaY;

            callbackMotionCounterX += deltaX;
            callbackMotionCounterY += deltaY;

            currentState.X += deltaX;
            currentState.Y += deltaY;

            MouseMoved();
        }
        /// <summary>
        /// Notifies the mouse handler that the mouse was moved and absolute coordinates are specified.
        /// </summary>
        /// <param name="newX">New horizontal position.</param>
        /// <param name="newY">New vertical position.</param>
        public void MouseMoveAbsolute(uint newX, uint newY)
        {
            motionCounterX += (int)newX - currentState.X;
            motionCounterY += (int)newY - currentState.Y;

            callbackMotionCounterX += (int)newX - currentState.X;
            callbackMotionCounterY += (int)newY - currentState.Y;

            currentState.X = (int)newX;
            currentState.Y = (int)newY;

            MouseMoved();
        }
        /// <summary>
        /// Notifies the mouse handler the a mouse button was pressed.
        /// </summary>
        /// <param name="buttons">Button which was pressed.</param>
        public void MouseButtonDown(MouseButtons buttons)
        {
            buttonTracker.ButtonPress(buttons, currentState.X, currentState.Y);

            currentState.PressedButtons |= buttons;
            var mask = GetCallbackReasonPress(buttons);

            if ((mask & callbackMask) != 0)
            {
                reason = mask;
                vm.RaiseInterrupt(CallbackInterrupt);
            }
        }
        /// <summary>
        /// Notifies the mouse handler that a mouse button was released.
        /// </summary>
        /// <param name="buttons">Button which was released.</param>
        public void MouseButtonUp(MouseButtons buttons)
        {
            buttonTracker.ButtonRelease(buttons, currentState.X, currentState.Y);

            currentState.PressedButtons &= ~buttons;
            var mask = GetCallbackReasonRelease(buttons);

            if ((mask & callbackMask) != 0)
            {
                reason = mask;
                vm.RaiseInterrupt(CallbackInterrupt);
            }
        }

        /// <summary>
        /// Calls the user-specified callback function.
        /// </summary>
        private void RaiseCallback()
        {
            int scaledX = GetScaledX(currentState.X);
            int scaledY = GetScaledY(currentState.Y);

            vm.Processor.AX = (short)reason;
            vm.Processor.BX = (short)currentState.PressedButtons;
            vm.Processor.CX = (short)scaledX;
            vm.Processor.DX = (short)scaledY;
            vm.Processor.SI = (ushort)callbackMotionCounterX;
            vm.Processor.DI = (ushort)callbackMotionCounterY;
            callbackState = currentState;
            reason = CallbackMask.Disabled;

            Instructions.Call.FarAbsoluteCall(vm, (uint)((callbackSegment << 16) | callbackOffset));
        }
        /// <summary>
        /// Performs common handling of a move event.
        /// </summary>
        private void MouseMoved()
        {
            currentState.X = Math.Max(currentState.X, 0);
            currentState.X = Math.Min(currentState.X, vm.VideoMode.Width - 1);
            currentState.Y = Math.Max(currentState.Y, 0);
            currentState.Y = Math.Min(currentState.Y, vm.VideoMode.Height - 1);

            if ((callbackMask & CallbackMask.Move) != 0)
            {
                reason = CallbackMask.Move;
                vm.RaiseInterrupt(CallbackInterrupt);
            }

            vm.OnMouseMove(new MouseMoveEventArgs(currentState.X, currentState.Y));
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

        IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => new[] { 0x33, new InterruptHandlerInfo(CallbackInterrupt, Registers.AX | Registers.BX | Registers.CX | Registers.DX | Registers.BP | Registers.SI | Registers.DI | Registers.DS | Registers.ES, false, true) };
        void IInterruptHandler.HandleInterrupt(int interrupt)
        {
            if (interrupt == CallbackInterrupt)
            {
                this.RaiseCallback();
                return;
            }

            switch ((ushort)vm.Processor.AX)
            {
                case Functions.Reset:
                    vm.Processor.AX = -1; // Indicates mouse installed.
                    vm.Processor.BX = 3; // Three-button mouse.
                    showCount = 0;  // Mouse cursor invisible.
                    vm.IsMouseVisible = false;
                    minX = 0;
                    minY = 0;
                    maxY = this.UseDefaultVirtualWidth ? 479 : 199;
                    maxX = this.UseDefaultVirtualWidth ? 639 : 319;
                    callbackMotionCounterX = 0;
                    callbackMotionCounterY = 0;
                    break;

                case Functions.SoftwareReset:
                    vm.Processor.AX = -1; // Indicates mouse installed.
                    vm.Processor.BX = 3; // Three-button mouse.
                    break;

                case Functions.EnableMouseDriver:
                    // Always return success.
                    vm.Processor.AX = 0x20;
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
                    vm.Processor.CX = (short)GetScaledX(currentState.X);
                    vm.Processor.DX = (short)GetScaledY(currentState.Y);
                    vm.Processor.BX = (short)currentState.PressedButtons;
                    break;

                case Functions.SetCursorPosition:
                    // Bad things can happen if we don't filter out the useless messages.
                    if (currentState.X != GetRealX(vm.Processor.CX) || currentState.Y != GetRealY(vm.Processor.DX))
                    {
                        currentState.X = GetRealX(vm.Processor.CX);
                        currentState.Y = GetRealY(vm.Processor.DX);
                        var args = new MouseMoveEventArgs(currentState.X, currentState.Y);
                        vm.OnMouseMove(args);
                        vm.OnMouseMoveByEmulator(args);
                    }
                    break;

                case Functions.SetHorizontalRange:
                    minX = vm.Processor.CX;
                    maxX = vm.Processor.DX;
                    break;

                case Functions.SetVerticalRange:
                    minY = vm.Processor.CX;
                    maxY = vm.Processor.DX;
                    break;

                case Functions.GetMotionCounters:
                    vm.Processor.CX = (short)motionCounterX;
                    vm.Processor.DX = (short)motionCounterY;
                    motionCounterX = 0;
                    motionCounterY = 0;
                    break;

                case Functions.SetCallbackParameters:
                    callbackMask = (CallbackMask)vm.Processor.CX;
                    callbackSegment = vm.Processor.ES;
                    callbackOffset = (ushort)vm.Processor.DX;
                    break;

                case Functions.GetButtonPressData:
                    var pressInfo = buttonTracker.GetButtonPressInfo(vm.Processor.BX);
                    vm.Processor.BX = (short)(pressInfo.Count & 0x7FFFu);
                    vm.Processor.CX = (short)GetScaledX(pressInfo.X);
                    vm.Processor.DX = (short)GetScaledY(pressInfo.Y);
                    break;

                case Functions.GetButtonReleaseData:
                    var releaseInfo = buttonTracker.GetButtonReleaseInfo(vm.Processor.BX);
                    vm.Processor.BX = (short)(releaseInfo.Count & 0x7FFFu);
                    vm.Processor.CX = (short)GetScaledX(releaseInfo.X);
                    vm.Processor.DX = (short)GetScaledY(releaseInfo.Y);
                    break;

                case Functions.ExchangeCallbacks:
                    var newMask = (CallbackMask)vm.Processor.CX;
                    var newSegment = vm.Processor.ES;
                    var newOffset = (ushort)vm.Processor.DX;

                    vm.Processor.CX = (short)callbackMask;
                    vm.WriteSegmentRegister(SegmentIndex.ES, callbackSegment);
                    vm.Processor.DX = (short)callbackOffset;

                    callbackMask = newMask;
                    callbackSegment = newSegment;
                    callbackOffset = newOffset;
                    break;

                case Functions.SetMickeyPixelRatio:
                    // Ignore this for now.
                    break;

                case Functions.GetDriverStateStorageSize:
                    vm.Processor.AX = 0x200;
                    break;

                case Functions.SaveDriverState:
                case Functions.RestoreDriverState:
                    // Ignore this for now.
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Mouse function {vm.Processor.AX:X2}h not implemented.");
                    break;
            }
        }

        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm) => this.vm = vm;

        void IDisposable.Dispose()
        {
        }
    }
}
