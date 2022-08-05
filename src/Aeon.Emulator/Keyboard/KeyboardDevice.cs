using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

#nullable disable

namespace Aeon.Emulator.Keyboard
{
    /// <summary>
    /// Emulates a physical keyboard device and handles related interrupts.
    /// </summary>
    internal sealed class KeyboardDevice : IInterruptHandler, IInputPort, IOutputPort
    {
        private const ushort LeftShiftUp = 0xAA;
        private const ushort RightShiftUp = 0xB6;
        private const ushort CtrlUp = 0x9D;
        private const ushort AltUp = 0xB8;
        private const ushort InsertUp = 0xD2;
        private const int InitialRepeatDelay = 250;
        private const int RepeatDelay = 30;

        private VirtualMachine vm;
        private readonly ConcurrentQueue<byte> hardwareQueue = new();
        private readonly List<byte> internalBuffer = new();
        private readonly ConcurrentQueue<ushort> typeBuffer = new();
        private KeyModifiers modifiers;
        private bool leftShiftDown;
        private bool rightShiftDown;
        private readonly SortedList<Keys, bool> pressedKeys = new();
        private int expectedInputByteCount;
        private volatile Keys lastKey;
        private Timer autoRepeatTimer;
        private readonly object autoRepeatTimerLock = new();

        public KeyboardDevice()
        {
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
                this.pressedKeys[k] = false;
        }

        /// <summary>
        /// Gets a value indicating whether the type-ahead buffer has at least one character in it.
        /// </summary>
        public bool HasTypeAheadDataAvailable => !typeBuffer.IsEmpty;
        /// <summary>
        /// Gets a value indicating whether the hardware queue is empty.
        /// </summary>
        public bool IsHardwareQueueEmpty => hardwareQueue.IsEmpty;

        /// <summary>
        /// Gets or sets the current byte in the hardware queue.
        /// </summary>
        private byte? CurrentHardwareByte { get; set; }

        /// <summary>
        /// Simulates a key press on the emulated keyboard.
        /// </summary>
        /// <param name="key">Key pressed on the keyboard.</param>
        public void PressKey(Keys key)
        {
            lock (this.pressedKeys)
            {
                if (!this.pressedKeys[key])
                {
                    this.pressedKeys[key] = true;
                    this.HardwareEnqueue(key, true);
                    this.lastKey = key;
                    lock (this.autoRepeatTimerLock)
                    {
                        if (this.autoRepeatTimer != null)
                            this.autoRepeatTimer.Change(InitialRepeatDelay, Timeout.Infinite);
                        else
                            this.autoRepeatTimer = new Timer(this.AutoRepeatTrigger, null, InitialRepeatDelay, Timeout.Infinite);
                    }
                }
            }
        }
        /// <summary>
        /// Simulates a key release on the emulated keyboard.
        /// </summary>
        /// <param name="key">Key released on the keyboard.</param>
        public void ReleaseKey(Keys key)
        {
            lock (this.pressedKeys)
            {
                this.lastKey = default;
                if (this.pressedKeys[key])
                {
                    this.pressedKeys[key] = false;
                    this.HardwareEnqueue(key, false);
                }
            }
        }
        /// <summary>
        /// Reads a single character from the type-ahead buffer.
        /// </summary>
        /// <returns>Character read from the type-ahead buffer if one is available; otherwize zero.</returns>
        public ushort DequeueTypeAhead()
        {
            return typeBuffer.Dequeue((ushort)0);
        }
        /// <summary>
        /// Returns the character read from the type-ahead buffer if one is available.
        /// </summary>
        /// <returns>Character read from the type-ahead buffer if one is available; otherwise null.</returns>
        public ushort? TryDequeueTypeAhead()
        {
            if (typeBuffer.TryDequeue(out ushort result))
                return result;
            else
                return null;
        }
        /// <summary>
        /// Dequeues the next hardware input byte.
        /// Must be called when IRQ 1 is handled.
        /// </summary>
        public void BeginHardwareInterrupt()
        {
            if (hardwareQueue.TryDequeue(out byte value))
                this.CurrentHardwareByte = value;
            else
                this.CurrentHardwareByte = null;
        }

        private void ProcessScanCode(uint scanCode)
        {
            bool isModifier = true;

            switch (scanCode)
            {
                case (uint)Keys.LeftShift:
                    leftShiftDown = true;
                    modifiers |= KeyModifiers.Shift;
                    break;

                case (uint)Keys.RightShift:
                    rightShiftDown = true;
                    modifiers |= KeyModifiers.Shift;
                    break;

                case (uint)Keys.Ctrl:
                case (uint)Keys.RightCtrl:
                    modifiers |= KeyModifiers.Ctrl;
                    break;

                case (uint)Keys.Alt:
                case (uint)Keys.RightAlt:
                    modifiers |= KeyModifiers.Alt;
                    break;

                case (uint)Keys.CapsLock:
                    modifiers ^= KeyModifiers.CapsLock;
                    break;

                case (uint)Keys.NumLock:
                    modifiers ^= KeyModifiers.NumLock;
                    break;

                case (uint)Keys.ScrollLock:
                    modifiers ^= KeyModifiers.ScrollLock;
                    break;

                case (uint)Keys.Insert:
                    modifiers |= KeyModifiers.Insert;
                    break;

                case LeftShiftUp:
                    leftShiftDown = false;
                    if (!leftShiftDown && !rightShiftDown)
                        modifiers &= ~KeyModifiers.Shift;
                    break;

                case RightShiftUp:
                    rightShiftDown = false;
                    if (!leftShiftDown && !rightShiftDown)
                        modifiers &= ~KeyModifiers.Shift;
                    break;

                case CtrlUp:
                    modifiers &= ~KeyModifiers.Ctrl;
                    break;

                case AltUp:
                    modifiers &= ~KeyModifiers.Alt;
                    break;

                case InsertUp:
                    modifiers &= ~KeyModifiers.Insert;
                    break;

                default:
                    isModifier = false;
                    break;
            }

            if (!isModifier)
            {
                ushort keyCode = ScanCodeConverter.ConvertToKeyboardCode((byte)(scanCode & 0xFF), modifiers);
                if (keyCode != 0)
                {
                    vm.Processor.CX = (short)keyCode;
                    vm.Processor.AH = Functions.StoreKeyCodeInBuffer;
                    vm.RaiseInterrupt(0x16);
                }
            }
        }
        /// <summary>
        /// Handles the 9h hardware interrupt.
        /// </summary>
        private void HandleInt9h()
        {
            // Send end of interrupt signal.
            vm.WritePortByte(0x20, 0x20);

            byte? nullableValue = this.CurrentHardwareByte;
            if (nullableValue == null)
                return;

            byte value = (byte)nullableValue;
            if (expectedInputByteCount == 0)
            {
                if (value == 0xE0)
                    expectedInputByteCount = 1;
                else if (value == 0xE1)
                    expectedInputByteCount = 5;
                else
                {
                    ProcessScanCode(value);
                    return;
                }
            }
            else
                expectedInputByteCount--;

            internalBuffer.Add(value);

            if (expectedInputByteCount == 0)
            {
                if (internalBuffer[0] != 0xE1)
                {
                    uint scanCode = (uint)(internalBuffer[1] | (internalBuffer[0] << 8));
                    ProcessScanCode(scanCode);
                }

                internalBuffer.Clear();
            }
        }
        /// <summary>
        /// Handles the 16h BIOS interrupt.
        /// </summary>
        private void HandleInt16h()
        {
            switch (vm.Processor.AH)
            {
                case Functions.ReadCharacter:
                case Functions.ReadExtendedCharacter:
                    ReadCharacter();
                    break;

                case Functions.CheckForCharacter:
                case Functions.CheckForExtendedCharacter:
                    CheckForCharacter();
                    SaveFlags(EFlags.Zero);
                    break;

                case Functions.GetShiftFlags:
                    GetShiftFlags();
                    break;

                case Functions.StoreKeyCodeInBuffer:
                    typeBuffer.Enqueue((ushort)vm.Processor.CX);
                    break;

                case Functions.GetExtendedShiftFlags:
                    GetShiftFlags();
                    break;

                default:
                    System.Diagnostics.Debug.Write("Unknown int16 command.");
                    break;
            }
        }
        /// <summary>
        /// Reads a single character from the type-ahead buffer.
        /// Moves Processor.EIP back to reissue this call until something is returned.
        /// </summary>
        private void ReadCharacter()
        {
            if (!typeBuffer.IsEmpty)
                vm.Processor.AX = (short)DequeueTypeAhead();
            else
            {
                // Run this interrupt handler again if there is nothing to read yet.
                vm.Processor.EIP -= 3;
                vm.Processor.Flags.InterruptEnable = true;
            }
        }
        /// <summary>
        /// Reads a single character from the type-ahead buffer if one is available.
        /// </summary>
        private void CheckForCharacter()
        {
            if (!typeBuffer.IsEmpty)
            {
                vm.Processor.AX = (short)typeBuffer.Peek((ushort)0);
                vm.Processor.Flags.Zero = false;
            }
            else
            {
                vm.Processor.AX = 0;
                vm.Processor.Flags.Zero = true;
            }
        }
        /// <summary>
        /// Places the scancode for a key press/release in the emulated keyboard buffer.
        /// </summary>
        /// <param name="key">Key pressed or released.</param>
        /// <param name="isPressed">Value indicating whether key is pressed.</param>
        private void HardwareEnqueue(Keys key, bool isPressed)
        {
            if (key >= Keys.Esc && key <= Keys.KeypadPeriod)
            {
                if (isPressed)
                    hardwareQueue.Enqueue((byte)key);
                else
                    hardwareQueue.Enqueue((byte)((byte)key + 0x80));
            }
            else if (key == Keys.Pause)
            {
                if (isPressed)
                {
                    hardwareQueue.Enqueue(0xE1);
                    hardwareQueue.Enqueue(0x1D);
                    hardwareQueue.Enqueue(0x45);
                    hardwareQueue.Enqueue(0xE1);
                    hardwareQueue.Enqueue(0x9D);
                    hardwareQueue.Enqueue(0xC5);
                }
            }
            else if (((int)key & 0xE0) == 0xE0)
            {
                if (isPressed)
                {
                    hardwareQueue.Enqueue(0xE0);
                    hardwareQueue.Enqueue((byte)(((int)key >> 8) & 0xFF));
                }
                else
                    hardwareQueue.Enqueue((byte)((((int)key >> 8) & 0xFF) + 0x80));
            }
        }
        private void SaveFlags(EFlags modified)
        {
            var oldFlags = (EFlags)vm.PhysicalMemory.GetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4));
            oldFlags &= ~modified;
            vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)(oldFlags | (vm.Processor.Flags.Value & modified)));
        }
        private void GetShiftFlags()
        {
            byte value = 0;

            if ((modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
                value |= 2;
            if ((modifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl)
                value |= 4;
            if ((modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
                value |= 8;

            vm.Processor.AL = value;
        }
        private void AutoRepeatTrigger(object obj)
        {
            lock (this.pressedKeys)
            {
                var key = this.lastKey;

                if (this.pressedKeys[key])
                {
                    this.HardwareEnqueue(key, true);
                    lock (this.autoRepeatTimerLock)
                    {
                        this.autoRepeatTimer.Change(RepeatDelay, Timeout.Infinite);
                    }
                }
            }
        }

        IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => new[] { new InterruptHandlerInfo(0x09, Registers.AX | Registers.CX), (byte)0x16 };
        void IInterruptHandler.HandleInterrupt(int interrupt)
        {
            if (interrupt == 0x09)
                HandleInt9h();
            else if (interrupt == 0x16)
                HandleInt16h();
        }

        IEnumerable<int> IInputPort.InputPorts => new[] { 0x60, 0x64 };
        byte IInputPort.ReadByte(int port)
        {
            return port switch
            {
                0x60 => this.CurrentHardwareByte ?? 0,
                0x64 => 0,
                _ => throw new ArgumentException("Invalid port number.")
            };
        }
        ushort IInputPort.ReadWord(int port) => throw new NotSupportedException();

        IEnumerable<int> IOutputPort.OutputPorts => new[] { 0x60, 0x64 };
        void IOutputPort.WriteByte(int port, byte value)
        {
            if (port == 0x60)
                vm.PhysicalMemory.EnableA20 = !vm.PhysicalMemory.EnableA20;

            System.Diagnostics.Debug.WriteLine(string.Format("Keyboard port {0:X}h -> {1:X2}h", port, value));
        }
        void IOutputPort.WriteWord(int port, ushort value) => throw new NotSupportedException();

        void IVirtualDevice.DeviceRegistered(VirtualMachine vm) => this.vm = vm;
    }
}
