using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Aeon.Emulator.DebugSupport;
using Aeon.Emulator.Decoding;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Interrupts;
using Aeon.Emulator.Memory;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator
{
    /// <summary>
    /// Emulates the functions of an x86 system.
    /// </summary>
    public sealed class VirtualMachine : IDisposable, IMachineCodeSource
    {
        private static bool instructionSetInitialized;
        private static readonly object globalInitLock = new();

        /// <summary>
        /// The emulated physical memory of the virtual machine.
        /// </summary>
        public readonly PhysicalMemory PhysicalMemory;
        /// <summary>
        /// The emulated processor of the virtual machine.
        /// </summary>
        public readonly Processor Processor = new();
        /// <summary>
        /// The emulated programmable interrupt controller of the virtual machine.
        /// </summary>
        public readonly InterruptController InterruptController = new();
        /// <summary>
        /// The emulated programmable interval timer of the virtual machine.
        /// </summary>
        public readonly InterruptTimer InterruptTimer;

        private readonly IInterruptHandler[] interruptHandlers = new IInterruptHandler[256];
        private readonly SortedList<ushort, IInputPort> inputPorts = new();
        private readonly SortedList<ushort, IOutputPort> outputPorts = new();
        private readonly DefaultPortHandler defaultPortHandler = new();
        private readonly List<IVirtualDevice> allDevices = new();
        private readonly ExpandedMemoryManager emm;
        private readonly ExtendedMemoryManager xmm;
        private readonly List<DmaChannel> dmaDeviceChannels = new();
        private readonly SortedList<uint, ICallbackProvider> callbackProviders = new();
        private readonly MultiplexInterruptHandler multiplexHandler;
        private bool disposed;
        private bool showMouse;
        private bool showCursor = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualMachine"/> class.
        /// </summary>
        public VirtualMachine() : this(new VirtualMachineStartupOptions())
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualMachine"/> class.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        public VirtualMachine(VirtualMachineStartupOptions options)
        {
            if (options.PhysicalMemory < 1 || options.PhysicalMemory > 2048)
                throw new ArgumentException("Invalid amount of physical memory specified.");

            lock (globalInitLock)
            {
                if (!instructionSetInitialized)
                {
                    InstructionSet.Initialize();
                    instructionSetInitialized = true;
                }
            }

            this.PhysicalMemory = new PhysicalMemory(options.PhysicalMemory * 1024 * 1024);
            this.Keyboard = new Keyboard.KeyboardDevice();
            this.ConsoleIn = new ConsoleInStream(this.Keyboard);
            this.Video = new Video.VideoHandler(this);
            this.ConsoleOut = new ConsoleOutStream(this.Video.TextConsole);
            this.Dos = new Dos.DosHandler(this);
            this.Mouse = new Mouse.MouseHandler();
            this.InterruptTimer = new InterruptTimer();
            if (options.EmsEnabled)
                this.emm = new ExpandedMemoryManager();

            this.xmm = new ExtendedMemoryManager();
            this.DmaController = new DmaController();
            this.Console = new VirtualConsole(this);
            this.multiplexHandler = new MultiplexInterruptHandler();
            this.PhysicalMemory.Video = this.Video;
            this.PhysicalMemory.Ems = this.emm;

            this.Dos.InitializationComplete();
            this.Dos.EmmHack = options.EmsEnabled;

            this.RegisterVirtualDevice(this.Dos);
            this.RegisterVirtualDevice(this.Video);
            this.RegisterVirtualDevice(this.Keyboard);
            this.RegisterVirtualDevice(this.Mouse);
            this.RegisterVirtualDevice(new RealTimeClockHandler());
            this.RegisterVirtualDevice(new ErrorHandler());
            this.RegisterVirtualDevice(this.InterruptController);
            this.RegisterVirtualDevice(this.InterruptTimer);
            this.RegisterVirtualDevice(this.xmm);
            if (options.EmsEnabled)
                this.RegisterVirtualDevice(this.emm);
            this.RegisterVirtualDevice(this.DmaController);
            this.RegisterVirtualDevice(this.multiplexHandler);
            this.RegisterVirtualDevice(new BiosServices.SystemServices());
            this.RegisterVirtualDevice(new Dos.CD.Mscdex());
            this.RegisterVirtualDevice(new LowLevelDisk.LowLevelDiskInterface());

            this.PhysicalMemory.AddTimerInterruptHandler();
        }

        /// <summary>
        /// Occurs when the emulated display mode has changed.
        /// </summary>
        public event EventHandler VideoModeChanged;
        /// <summary>
        /// Occurs when the emulator sets the mouse position.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseMoveByEmulator;
        /// <summary>
        /// Occurs when the internal mouse position has changed.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        /// <summary>
        /// Occurs when the mouse cursor is shown or hidden.
        /// </summary>
        public event EventHandler MouseVisibilityChanged;
        /// <summary>
        /// Occurs when the text-mode cursor is shown or hidden.
        /// </summary>
        public event EventHandler CursorVisibilityChanged;
        /// <summary>
        /// Occurs when the current process has changed.
        /// </summary>
        public event EventHandler CurrentProcessChanged;
        /// <summary>
        /// Occurs when an informational message has been written to the log.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageLogged;

        /// <summary>
        /// Gets the virtual file system used by the virtual machine.
        /// </summary>
        public FileSystem FileSystem { get; } = new FileSystem();
        /// <summary>
        /// Gets information about the current emulated video mode.
        /// </summary>
        public Video.VideoMode VideoMode => this.Video?.CurrentMode;
        /// <summary>
        /// Gets a collection of all virtual devices registered with the virtual machine.
        /// </summary>
        public IEnumerable<IVirtualDevice> Devices => allDevices.AsReadOnly();
        /// <summary>
        /// Gets a value indicating whether the mouse cursor should be displayed.
        /// </summary>
        public bool IsMouseVisible
        {
            get => this.showMouse;
            internal set
            {
                if (showMouse != value)
                {
                    showMouse = value;
                    OnMouseVisibilityChanged(EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets the current position of the mouse.
        /// </summary>
        public Video.Point MousePosition => Mouse.Position;
        /// <summary>
        /// Gets the current DOS environment variables.
        /// </summary>
        public EnvironmentVariables EnvironmentVariables { get; } = new EnvironmentVariables();
        /// <summary>
        /// Gets the DMA controller for the virtual machine.
        /// </summary>
        public DmaController DmaController { get; }
        /// <summary>
        /// Gets the current position of the cursor.
        /// </summary>
        public Video.Point CursorPosition
        {
            get
            {
                var console = this.Video.TextConsole;
                return console != null ? console.CursorPosition : new Video.Point();
            }
        }
        /// <summary>
        /// Gets a value indicating whether the text-mode cursor is visible.
        /// </summary>
        public bool IsCursorVisible
        {
            get => this.showCursor;
            internal set
            {
                if (showCursor != value)
                {
                    showCursor = value;
                    OnCursorVisibilityChanged(EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets the stream used for writing data to the emulated console.
        /// </summary>
        public ConsoleOutStream ConsoleOut { get; }
        /// <summary>
        /// Gets the stream used for reading data from the emulated keyboard.
        /// </summary>
        public ConsoleInStream ConsoleIn { get; }
        /// <summary>
        /// Gets the console associated with the virtual machine.
        /// </summary>
        public VirtualConsole Console { get; }
        /// <summary>
        /// Gets information about the current process.
        /// </summary>
        public Dos.DosProcess CurrentProcess => this.Dos.CurrentProcess;

        internal Keyboard.KeyboardDevice Keyboard { get; }
        internal Mouse.MouseHandler Mouse { get; }
        internal Dos.DosHandler Dos { get; }
        internal Video.VideoHandler Video { get; }
        internal ExtendedMemoryManager ExtendedMemory => this.xmm;
        internal bool BigStackPointer { get; set; }

        /// <summary>
        /// Loads an executable file into the virtual machine.
        /// </summary>
        /// <param name="image">Executable file to load.</param>
        public void LoadImage(ProgramImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Dos.LoadImage(image);
        }
        /// <summary>
        /// Loads an executable file into the virtual machine.
        /// </summary>
        /// <param name="image">Executable file to load.</param>
        /// <param name="commandLineArguments">Command line arguments for the program.</param>
        public void LoadImage(ProgramImage image, string commandLineArguments, string stdOut = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (commandLineArguments != null && commandLineArguments.Length > 255)
                throw new ArgumentException("Command line length must not exceed 255 characters.");

            Dos.LoadImage(image, commandLineArgs: commandLineArguments, stdOut: stdOut);
        }
        /// <summary>
        /// Raises a hardware/software interrupt on the virtual machine.
        /// </summary>
        /// <param name="interrupt">Interrupt to raise.</param>
        /// <param name="soft">Value indicating whether the interrupt was raised from software.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RaiseInterrupt(byte interrupt, bool soft = false)
        {
            if (!this.Processor.CR0.HasFlag(CR0.ProtectedModeEnable))     // Real mode
            {
                var address = this.PhysicalMemory.GetRealModeInterruptAddress(interrupt);
                if (address.Segment == 0 && address.Offset == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Unhandled real-mode interrupt");
                    return;
                }

                this.PushToStack((ushort)this.Processor.Flags.Value, this.Processor.CS, this.Processor.IP);

                this.Processor.EIP = address.Offset;
                this.WriteSegmentRegister(SegmentIndex.CS, address.Segment);

                this.Processor.Flags.Trap = false;
                this.Processor.Flags.InterruptEnable = false;
            }
            else        // Protected mode
            {
                var descriptor = this.PhysicalMemory.GetInterruptDescriptor(interrupt);
                if (descriptor.DescriptorType == DescriptorType.InterruptGate || descriptor.DescriptorType == DescriptorType.TrapGate)
                {
                    var interruptGate = (InterruptDescriptor)descriptor;
                    uint wordSize = interruptGate.Is32Bit ? 4u : 2u;
                    uint cpl = this.Processor.CPL;
                    uint rpl = interruptGate.Selector & 3u;
                    uint dpl = interruptGate.PrivilegeLevel;

                    if (soft && (cpl > dpl || cpl > rpl))
                    {
                        this.RaiseException(new GeneralProtectionFaultException(interrupt * 8 + 2));
                        return;
                    }

                    if (this.Processor.Flags.Virtual8086Mode)
                    {
                        var oldFlags = this.Processor.Flags.Value;
                        ushort oldSS = this.Processor.SS;
                        uint oldESP = this.Processor.ESP;

                        this.Processor.Flags.Virtual8086Mode = false;
                        ushort newSS = this.GetPrivilegedSS(rpl, wordSize);
                        uint newESP = this.GetPrivilegedESP(rpl, wordSize);

                        WriteSegmentRegister(SegmentIndex.SS, newSS);
                        this.Processor.ESP = newESP;

                        this.PushToStack32(this.Processor.GS);
                        this.PushToStack32(this.Processor.FS);
                        this.PushToStack32(this.Processor.DS);
                        this.PushToStack32(this.Processor.ES);
                        this.PushToStack32(oldSS);
                        this.PushToStack32(oldESP);

                        if (wordSize == 4u)
                        {
                            this.PushToStack32((uint)oldFlags);
                            this.PushToStack32(this.Processor.CS);
                            this.PushToStack32(this.Processor.EIP);
                        }
                        else
                        {
                            this.PushToStack((ushort)oldFlags);
                            this.PushToStack(this.Processor.CS);
                            this.PushToStack(this.Processor.IP);
                        }

                        this.WriteSegmentRegister(SegmentIndex.DS, 0);
                        this.WriteSegmentRegister(SegmentIndex.ES, 0);
                        this.WriteSegmentRegister(SegmentIndex.FS, 0);
                        this.WriteSegmentRegister(SegmentIndex.GS, 0);
                    }
                    else
                    {
                        if (cpl > rpl)
                        {
                            ushort oldSS = this.Processor.SS;
                            uint oldESP = this.Processor.ESP;

                            ushort newSS = this.GetPrivilegedSS(rpl, wordSize);
                            uint newESP = this.GetPrivilegedESP(rpl, wordSize);

                            WriteSegmentRegister(SegmentIndex.SS, newSS);
                            this.Processor.ESP = newESP;

                            if (wordSize == 4u)
                                this.PushToStack32(oldSS, oldESP);
                            else
                                this.PushToStack(oldSS, (ushort)oldESP);
                        }
                        else if (cpl < rpl)
                        {
                            throw new InvalidOperationException();
                        }

                        if (wordSize == 4u)
                            this.PushToStack32((uint)this.Processor.Flags.Value, this.Processor.CS, this.Processor.EIP);
                        else
                            this.PushToStack((ushort)this.Processor.Flags.Value, this.Processor.CS, this.Processor.IP);
                    }

                    this.Processor.EIP = interruptGate.Offset;
                    WriteSegmentRegister(SegmentIndex.CS, interruptGate.Selector);

                    // Disable interrupts if not a trap.
                    if (!interruptGate.IsTrap)
                        this.Processor.Flags.InterruptEnable = false;
                }
                else
                {
                    var desc = (InterruptDescriptor)descriptor;
                    this.TaskSwitch32(desc.Selector, false, true);
                }
            }
        }
        /// <summary>
        /// Emulates the next instruction.
        /// </summary>
        public void Emulate() => InstructionSet.Emulate(this, 1);
        /// <summary>
        /// Emulates multiple instructions.
        /// </summary>
        /// <param name="count">Number of instructions to emulate.</param>
        public void Emulate(int count) => InstructionSet.Emulate(this, (uint)count);
        /// <summary>
        /// Emulates the next instruction with logging enabled.
        /// </summary>
        /// <param name="log">Log to which instructions will be written.</param>
        public void Emulate(InstructionLog log) => InstructionSet.Emulate(this, log);
        /// <summary>
        /// Presses a key on the emulated keyboard.
        /// </summary>
        /// <param name="key">Key to press.</param>
        public void PressKey(Keys key)
        {
            var process = this.CurrentProcess;
            if (process != null)
                this.Keyboard.PressKey(key);
        }
        /// <summary>
        /// Releases a key on the emulated keyboard.
        /// </summary>
        /// <param name="key">Key to release.</param>
        public void ReleaseKey(Keys key)
        {
            var process = this.CurrentProcess;
            if (process != null)
                this.Keyboard.ReleaseKey(key);
        }
        /// <summary>
        /// Notifies the virtual machine that a keyboard ISR is about to be invoked.
        /// </summary>
        /// <remarks>
        /// This must be called once before IRQ 1 is handled.
        /// </remarks>
        public void PrepareForKeyboardHandler() => Keyboard.BeginHardwareInterrupt();
        /// <summary>
        /// Signals that a mouse input event has occurred.
        /// </summary>
        /// <param name="mouseEvent">Mouse input event that occurred.</param>
        public void MouseEvent(MouseEvent mouseEvent)
        {
            if (mouseEvent == null)
                throw new ArgumentNullException(nameof(mouseEvent));

            mouseEvent.RaiseEvent(Mouse);
        }
        /// <summary>
        /// Registers a virtual device with the virtual machine.
        /// </summary>
        /// <param name="virtualDevice">Virtual device to register.</param>
        public void RegisterVirtualDevice(IVirtualDevice virtualDevice)
        {
            if (virtualDevice == null)
                throw new ArgumentNullException(nameof(virtualDevice));

            if (virtualDevice is IInterruptHandler interruptHandler)
            {
                foreach (var interrupt in interruptHandler.HandledInterrupts)
                {
                    interruptHandlers[interrupt.Interrupt] = interruptHandler;
                    PhysicalMemory.AddInterruptHandler((byte)interrupt.Interrupt, interrupt.SavedRegisters, interrupt.IsHookable, interrupt.ClearInterruptFlag);
                }
            }

            if (virtualDevice is IMultiplexInterruptHandler multiplex)
                this.multiplexHandler.Handlers.Add(multiplex);

            if (virtualDevice is IInputPort inputPort)
            {
                foreach (int port in inputPort.InputPorts)
                    inputPorts[(ushort)port] = inputPort;
            }

            if (virtualDevice is IOutputPort outputPort)
            {
                foreach (int port in outputPort.OutputPorts)
                    outputPorts[(ushort)port] = outputPort;
            }

            if (virtualDevice is IDmaDevice8 dmaDevice)
            {
                if (dmaDevice.Channel < 0 || dmaDevice.Channel >= DmaController.Channels.Count)
                    throw new ArgumentException("Invalid DMA channel on DMA device.");

                DmaController.Channels[dmaDevice.Channel].Device = dmaDevice;
                dmaDeviceChannels.Add(DmaController.Channels[dmaDevice.Channel]);
            }

            if (virtualDevice is ICallbackProvider callbackProvider)
            {
                int id = callbackProviders.Count;
                callbackProvider.CallbackAddress = this.PhysicalMemory.AddCallbackHandler((byte)id, callbackProvider.IsHookable);
                callbackProviders.Add((uint)id, callbackProvider);

                Span<byte> machineCode = stackalloc byte[3];
                machineCode[0] = 0x0F;
                machineCode[1] = 0x56;
                machineCode[2] = (byte)id;
                callbackProvider.SetRaiseCallbackInstruction(machineCode);
            }

            allDevices.Add(virtualDevice);
            virtualDevice.DeviceRegistered(this);
        }
        /// <summary>
        /// Informs the VirtualMachine instance that initialization is complete and no more devices will be added.
        /// This must be called prior to emulation.
        /// </summary>
        public void EndInitialization() => this.PhysicalMemory.ReserveBaseMemory();
        /// <summary>
        /// Returns an object containing information about current conventional memory usage.
        /// </summary>
        /// <returns>Information about current conventional memory usage.</returns>
        public ConventionalMemoryInfo GetConventionalMemoryUsage() => Dos.GetAllocations();
        /// <summary>
        /// Returns an object containing information about current expanded memory usage.
        /// </summary>
        /// <returns>Information about current expanded memory usage.</returns>
        public ExpandedMemoryInfo GetExpandedMemoryUsage() => new(this.emm?.AllocatedPages ?? 0);
        /// <summary>
        /// Returns an object containing information about current extended memory usage.
        /// </summary>
        /// <returns>Information about current extended memory usage.</returns>
        public ExtendedMemoryInfo GetExtendedMemoryUsage() => new(xmm.ExtendedMemorySize - (int)xmm.TotalFreeMemory, xmm.ExtendedMemorySize);
        /// <summary>
        /// Performs any pending DMA transfers.
        /// </summary>
        /// <remarks>
        /// This method must be called frequently in the main emulation loop for DMA transfers to function properly.
        /// </remarks>
        public void PerformDmaTransfers()
        {
            foreach (var channel in this.dmaDeviceChannels)
            {
                if (channel.IsActive && !channel.IsMasked)
                    channel.Transfer(this.PhysicalMemory);
            }
        }
        /// <summary>
        /// Raises the appropriate interrupt handler for a runtime exception if possible.
        /// </summary>
        /// <param name="exception">Runtime exception to raise.</param>
        /// <returns>Value indicating whether exception was handled.</returns>
        public bool RaiseException(EmulatedException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            System.Diagnostics.Debug.WriteLine(exception.Message);

            if (this.Processor.PrefixCount > 0)
            {
                this.Processor.EIP = this.Processor.StartEIP - this.Processor.PrefixCount;
                this.Processor.InstructionEpilog();
            }
            else
            {
                this.Processor.EIP = this.Processor.StartEIP;
            }

            exception.OnRaised(this);

            if (exception.Interrupt >= 0 && exception.Interrupt <= 255)
            {
                this.RaiseInterrupt((byte)exception.Interrupt);
                if (exception.ErrorCode != null)
                {
                    bool is32Bit;
                    var descriptor = this.PhysicalMemory.GetInterruptDescriptor((byte)exception.Interrupt);
                    if (descriptor.DescriptorType == DescriptorType.InterruptGate)
                        is32Bit = ((InterruptDescriptor)descriptor).Is32Bit;
                    else if (descriptor.DescriptorType == DescriptorType.TaskGate)
                        is32Bit = true;
                    else
                        throw new NotImplementedException();

                    if (is32Bit)
                        this.PushToStack32((uint)(int)exception.ErrorCode);
                    else
                        this.PushToStack((ushort)(int)exception.ErrorCode);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Releases resources used during emulation.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Writes a message to the emulator's log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="level">Level of the message.</param>
        public void WriteMessage(string message, MessageLevel level = MessageLevel.Debug) => this.MessageLogged?.Invoke(this, new MessageEventArgs(level, message));
        /// <summary>
        /// Writes a message to the emulator's log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="level">Level of the message.</param>
        public void WriteMessage(ReadOnlySpan<char> message, MessageLevel level = MessageLevel.Debug) => this.MessageLogged?.Invoke(this, new MessageEventArgs(level, new string(message)));

        /// <summary>
        /// Returns the logical base address for a given selector.
        /// </summary>
        /// <param name="selector">Selector whose base address is returned.</param>
        /// <returns>Base address of the selector if it is valid; otherwise null.</returns>
        uint? IMachineCodeSource.GetBaseAddress(ushort selector)
        {
            if (!this.Processor.CR0.HasFlag(CR0.ProtectedModeEnable) | this.Processor.Flags.Virtual8086Mode)
            {
                return (uint)selector << 4;
            }
            else if (selector != 0)
            {
                var descriptor = (SegmentDescriptor)this.PhysicalMemory.GetDescriptor(selector);
                if (descriptor.Base < this.PhysicalMemory.MemorySize)
                    return descriptor.Base;
            }

            return null;
        }
        /// <summary>
        /// Reads 16 bytes of data from the machine code source at the specified address.
        /// </summary>
        /// <param name="buffer">Buffer into which data is read. Must be at least 16 bytes long.</param>
        /// <param name="logicalAddress">Logical address in machine code source where instruction is read from.</param>
        /// <returns>Number of bytes actually read. Should normally return 16.</returns>
        int IMachineCodeSource.ReadInstruction(byte[] buffer, uint logicalAddress)
        {
            unsafe
            {
                fixed (byte* ptr = buffer)
                {
                    this.PhysicalMemory.FetchInstruction(logicalAddress, ptr);
                }
            }

            return 16;
        }

        internal void CallInterruptHandler(byte interrupt)
        {
            var handler = this.interruptHandlers[interrupt];
            if (handler != null)
                handler.HandleInterrupt(interrupt);
            else
                ThrowHelper.ThrowNoInterruptHandlerException(interrupt);
        }
        internal void CallCallback(byte id)
        {
            if (this.callbackProviders.TryGetValue(id, out var provider))
                provider.InvokeCallback();
            else
                ThrowHelper.ThrowNoCallbackHandlerException(id);
        }
        internal void PushToStack(ushort value)
        {
            var p = this.Processor;
            unsafe
            {
                uint address = p.segmentBases[(int)SegmentIndex.SS];

                if (!this.BigStackPointer)
                {
                    ref ushort sp = ref p.SP;
                    sp -= 2;
                    address += sp;
                }
                else
                {
                    ref uint esp = ref p.ESP;
                    esp -= 2;
                    address += esp;
                }

                this.PhysicalMemory.SetUInt16(address, value);
            }
        }
        internal void PushToStack(ushort value1, ushort value2)
        {
            this.PushToStack(value1);
            this.PushToStack(value2);
        }
        internal void PushToStack(ushort value1, ushort value2, ushort value3)
        {
            this.PushToStack(value1);
            this.PushToStack(value2);
            this.PushToStack(value3);
        }
        internal void PushToStack32(uint value)
        {
            var p = this.Processor;
            unsafe
            {
                uint address = p.segmentBases[(int)SegmentIndex.SS];

                if (!this.BigStackPointer)
                {
                    ref ushort sp = ref p.SP;
                    sp -= 4;
                    address += sp;
                }
                else
                {
                    ref uint esp = ref p.ESP;
                    esp -= 4;
                    address += esp;
                }

                this.PhysicalMemory.SetUInt32(address, value);
            }
        }
        internal void PushToStack32(uint value1, uint value2)
        {
            this.PushToStack32(value1);
            this.PushToStack32(value2);
        }
        internal void PushToStack32(uint value1, uint value2, uint value3)
        {
            this.PushToStack32(value1);
            this.PushToStack32(value2);
            this.PushToStack32(value3);
        }
        internal ushort PopFromStack()
        {
            ushort value;
            unsafe
            {
                if (!this.BigStackPointer)
                {
                    var sp = (ushort*)this.Processor.PSP;
                    uint address = this.Processor.segmentBases[(int)SegmentIndex.SS] + *sp;
                    value = this.PhysicalMemory.GetUInt16(address);
                    *sp += 2;
                }
                else
                {
                    var esp = (uint*)this.Processor.PSP;
                    uint address = this.Processor.segmentBases[(int)SegmentIndex.SS] + *esp;
                    value = this.PhysicalMemory.GetUInt16(address);
                    *esp += 2;
                }
            }

            return value;
        }
        internal uint PopFromStack32()
        {
            uint value;
            unsafe
            {
                if (!this.BigStackPointer)
                {
                    var sp = (ushort*)this.Processor.PSP;
                    uint address = this.Processor.segmentBases[(int)SegmentIndex.SS] + *sp;
                    value = this.PhysicalMemory.GetUInt32(address);
                    *sp += 4;
                }
                else
                {
                    var esp = (uint*)this.Processor.PSP;
                    uint address = this.Processor.segmentBases[(int)SegmentIndex.SS] + *esp;
                    value = this.PhysicalMemory.GetUInt32(address);
                    *esp += 4;
                }
            }

            return value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToStackPointer(uint value)
        {
            if (!this.BigStackPointer)
                this.Processor.SP += (ushort)value;
            else
                this.Processor.ESP += value;
        }
        internal ushort PeekStack16(int offset = 0)
        {
            uint address;
            unsafe
            {
                if (!this.BigStackPointer)
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP + (uint)offset;
                else
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP + (uint)offset;
            }

            return this.PhysicalMemory.GetUInt16(address);
        }
        internal uint PeekStack32(int offset = 0)
        {
            uint address;
            unsafe
            {
                if (!this.BigStackPointer)
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP + (uint)offset;
                else
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP + (uint)offset;
            }

            return this.PhysicalMemory.GetUInt32(address);
        }
        internal ulong PeekStack48()
        {
            uint address;
            unsafe
            {
                if (!this.BigStackPointer)
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP;
                else
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP;
            }

            return this.PhysicalMemory.GetUInt64(address);
        }
        internal void WriteStack16(int offset, ushort value)
        {
            uint address;
            unsafe
            {
                if (!this.BigStackPointer)
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP + (uint)offset;
                else
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP + (uint)offset;
            }

            this.PhysicalMemory.SetUInt16(address, value);
        }
        internal void WriteStack32(int offset, uint value)
        {
            uint address;
            unsafe
            {
                if (!this.BigStackPointer)
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP + (uint)offset;
                else
                    address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP + (uint)offset;
            }

            this.PhysicalMemory.SetUInt32(address, value);
        }

        internal byte ReadPortByte(ushort port)
        {
            if (inputPorts.TryGetValue(port, out var inputPort))
                return inputPort.ReadByte(port);
            else
                return this.defaultPortHandler.ReadByte(port);
        }
        internal ushort ReadPortWord(ushort port)
        {
            if (inputPorts.TryGetValue(port, out var inputPort))
                return inputPort.ReadWord(port);
            else
                return 0xFFFF;
        }
        internal void WritePortByte(ushort port, byte value)
        {
            if (!outputPorts.TryGetValue(port, out var outputPort))
                defaultPortHandler.WriteByte(port, value);
            else
                outputPort.WriteByte(port, value);
        }
        internal void WritePortWord(ushort port, ushort value)
        {
            if (!outputPorts.TryGetValue(port, out var outputPort))
                defaultPortHandler.WriteWord(port, value);
            else
                outputPort.WriteWord(port, value);
        }
        /// <summary>
        /// Writes a value to a segment register.
        /// </summary>
        /// <param name="segment">Index of the segment.</param>
        /// <param name="value">Value to write to the segment register.</param>
        /// <remarks>
        /// This method should be called any time a segment register is changed instead of
        /// setting the segment register on the processor directly. This method also updates
        /// the precalculated base address for the segment.
        /// </remarks>
        public void WriteSegmentRegister(SegmentIndex segment, ushort value)
        {
            ushort oldValue;
            unsafe
            {
                oldValue = *this.Processor.segmentRegisterPointers[(int)segment];
                *this.Processor.segmentRegisterPointers[(int)segment] = value;
            }

            try
            {
                this.UpdateSegment(segment);
            }
            catch (SegmentNotPresentException)
            {
                unsafe
                {
                    *this.Processor.segmentRegisterPointers[(int)segment] = oldValue;
                }

                throw;
            }
            catch (GeneralProtectionFaultException)
            {
                unsafe
                {
                    *this.Processor.segmentRegisterPointers[(int)segment] = oldValue;
                }

                throw;
            }
        }
        internal void UpdateSegment(SegmentIndex segment)
        {
            unsafe
            {
                ushort value = *this.Processor.segmentRegisterPointers[(int)segment];

                if (!this.Processor.CR0.HasFlag(CR0.ProtectedModeEnable) | this.Processor.Flags.Virtual8086Mode)
                {
                    this.Processor.segmentBases[(int)segment] = (uint)value << 4;
                    this.BigStackPointer = false;
                }
                else
                {
                    var descriptor = this.PhysicalMemory.GetDescriptor(value);
                    if (value != 0 && descriptor.DescriptorType != DescriptorType.Segment)
                    {
                        ThrowHelper.ThrowGeneralProtectionFaultException(value);
                    }
                    else
                    {
                        var segmentDescriptor = (SegmentDescriptor)descriptor;

                        if (value > 3u && !segmentDescriptor.IsPresent)
                        {
                            ThrowHelper.ThrowSegmentNotPresentException(value);
                        }
                        else
                        {
                            this.Processor.segmentBases[(int)segment] = segmentDescriptor.Base;

                            if (segment == SegmentIndex.CS)
                            {
                                this.Processor.CPL = segmentDescriptor.PrivilegeLevel;

                                if ((segmentDescriptor.Attributes2 & SegmentDescriptor.BigMode) == 0)
                                    this.Processor.GlobalSize = 0;
                                else
                                    this.Processor.GlobalSize = 3;
                            }
                            else if (segment == SegmentIndex.SS)
                            {
                                this.BigStackPointer = (segmentDescriptor.Attributes2 & SegmentDescriptor.BigMode) != 0;
                                this.Processor.TemporaryInterruptMask = true;
                            }
                        }
                    }
                }
            }
        }
        internal void TaskSwitch32(ushort selector, bool clearBusyFlag, bool? nestedTaskFlag)
        {
            if (selector == 0)
                throw new ArgumentException("Invalid null selector.", nameof(selector));

            var p = this.Processor;

            unsafe
            {
                var newDesc = (TaskSegmentDescriptor)this.PhysicalMemory.GetDescriptor(selector);
                var tss = (TaskStateSegment32*)this.PhysicalMemory.GetSafePointer(newDesc.Base, (uint)sizeof(TaskStateSegment32));

                var oldSelector = this.PhysicalMemory.TaskSelector;
                var oldDesc = (TaskSegmentDescriptor)this.PhysicalMemory.GetDescriptor(oldSelector);
                var oldTSS = (TaskStateSegment32*)this.PhysicalMemory.GetSafePointer(oldDesc.Base, (uint)sizeof(TaskStateSegment32));

                oldTSS->CS = p.CS;
                oldTSS->SS = p.SS;
                oldTSS->DS = p.DS;
                oldTSS->ES = p.ES;
                oldTSS->FS = p.FS;
                oldTSS->GS = p.GS;

                oldTSS->EIP = p.EIP;
                oldTSS->ESP = p.ESP;
                oldTSS->EAX = (uint)p.EAX;
                oldTSS->EBX = (uint)p.EBX;
                oldTSS->ECX = (uint)p.ECX;
                oldTSS->EDX = (uint)p.EDX;
                oldTSS->EBP = p.EBP;
                oldTSS->ESI = p.ESI;
                oldTSS->EDI = p.EDI;
                oldTSS->EFLAGS = p.Flags.Value;
                oldTSS->CR3 = p.CR3;
                oldTSS->LDTR = this.PhysicalMemory.LDTSelector;

                if (nestedTaskFlag == false)
                    oldTSS->EFLAGS &= ~EFlags.NestedTask;

                p.CR3 = tss->CR3;
                this.PhysicalMemory.DirectoryAddress = tss->CR3;
                this.PhysicalMemory.LDTSelector = tss->LDTR;
                p.Flags.Virtual8086Mode = tss->EFLAGS.HasFlag(EFlags.Virtual8086Mode);

                WriteSegmentRegister(SegmentIndex.CS, tss->CS);
                WriteSegmentRegister(SegmentIndex.SS, tss->SS);
                WriteSegmentRegister(SegmentIndex.DS, tss->DS);
                WriteSegmentRegister(SegmentIndex.ES, tss->ES);
                WriteSegmentRegister(SegmentIndex.FS, tss->FS);
                WriteSegmentRegister(SegmentIndex.GS, tss->GS);

                p.EIP = tss->EIP;
                p.ESP = tss->ESP;
                p.EAX = (int)tss->EAX;
                p.EBX = (int)tss->EBX;
                p.ECX = (int)tss->ECX;
                p.EDX = (int)tss->EDX;
                p.EBP = tss->EBP;
                p.ESI = tss->ESI;
                p.EDI = tss->EDI;
                p.Flags.Value = tss->EFLAGS | EFlags.Reserved1;

                if (nestedTaskFlag == true)
                {
                    p.Flags.NestedTask = true;
                    tss->LINK = oldSelector;
                }

                if (clearBusyFlag)
                    oldDesc.IsBusy = false;

                newDesc.IsBusy = true;

                this.PhysicalMemory.SetDescriptor(oldSelector, oldDesc);
                this.PhysicalMemory.SetDescriptor(selector, newDesc);
            }

            this.PhysicalMemory.TaskSelector = selector;
        }
        internal void TaskReturn()
        {
            unsafe
            {
                var selector = this.PhysicalMemory.TaskSelector;
                var desc = (TaskSegmentDescriptor)this.PhysicalMemory.GetDescriptor(selector);
                var tss = (TaskStateSegment32*)this.PhysicalMemory.GetSafePointer(desc.Base, (uint)sizeof(TaskStateSegment32));
                this.TaskSwitch32(tss->LINK, true, false);
            }
        }
        internal ushort GetPrivilegedSS(uint privilegeLevel, uint wordSize)
        {
            ushort tss = this.PhysicalMemory.TaskSelector;
            if (tss == 0)
                ThrowHelper.ThrowInvalidTaskSegmentSelectorException();

            var segmentDescriptor = (SegmentDescriptor)this.PhysicalMemory.GetDescriptor(tss);
            unsafe
            {
                return *(ushort*)this.PhysicalMemory.GetSafePointer(segmentDescriptor.Base + (wordSize * 2u) + (privilegeLevel * (wordSize * 2u)), 2u);
            }
        }
        internal uint GetPrivilegedESP(uint privilegeLevel, uint wordSize)
        {
            ushort tss = this.PhysicalMemory.TaskSelector;
            if (tss == 0)
                ThrowHelper.ThrowInvalidTaskSegmentSelectorException();

            var segmentDescriptor = (SegmentDescriptor)this.PhysicalMemory.GetDescriptor(tss);
            unsafe
            {
                byte* ptr = (byte*)this.PhysicalMemory.GetSafePointer(segmentDescriptor.Base + wordSize + (privilegeLevel * (wordSize * 2u)), wordSize);
                if (wordSize == 4u)
                    return *(uint*)ptr;
                else
                    return *(ushort*)ptr;
            }
        }
        /// <summary>
        /// Raises the VideoModeChanged event.
        /// </summary>
        /// <param name="e">Empty EventArgs instance.</param>
        internal void OnVideoModeChanged(EventArgs e)
        {
            this.VideoModeChanged?.Invoke(this, e);

            var mode = this.VideoMode;
            if (mode != null)
                this.IsCursorVisible = mode.HasCursor;
        }
        /// <summary>
        /// Raises the MouseMoveByEmulator event.
        /// </summary>
        /// <param name="e">MouseMoveEventArgs instance with information about the mouse movement.</param>
        internal void OnMouseMoveByEmulator(MouseMoveEventArgs e) => this.MouseMoveByEmulator?.Invoke(this, e);
        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">MouseMoveEventArgs instance with information about the mouse movement.</param>
        internal void OnMouseMove(MouseMoveEventArgs e) => this.MouseMove?.Invoke(this, e);
        /// <summary>
        /// Raises the CurrentProcessChanged event.
        /// </summary>
        /// <param name="e">Empty EventArgs instance.</param>
        internal void OnCurrentProcessChanged(EventArgs e) => this.CurrentProcessChanged?.Invoke(this, e);

        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if (disposing)
                {
                    foreach (var device in this.allDevices)
                    {
                        if (device is IDisposable d)
                            d.Dispose();
                    }

                    this.allDevices.Clear();
                    this.PhysicalMemory.InternalDispose();
                }

                this.disposed = true;
            }
        }
        /// <summary>
        /// Raises the MouseVisibilityChanged event.
        /// </summary>
        /// <param name="e">Empty EventArgs instance.</param>
        private void OnMouseVisibilityChanged(EventArgs e) => this.MouseVisibilityChanged?.Invoke(this, e);
        /// <summary>
        /// Raises the CursorVisibilityChanged event.
        /// </summary>
        /// <param name="e">Empty EventArgs instance.</param>
        private void OnCursorVisibilityChanged(EventArgs e) => this.CursorVisibilityChanged?.Invoke(this, e);
    }
}
