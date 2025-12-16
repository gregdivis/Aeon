using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aeon.Emulator.Decoding;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Interrupts;
using Aeon.Emulator.Memory;
using Aeon.Emulator.RuntimeExceptions;
using Aeon.Emulator.Video.Rendering;

namespace Aeon.Emulator;

/// <summary>
/// Emulates the functions of an x86 system.
/// </summary>
public sealed class VirtualMachine : IDisposable
{
    private static bool instructionSetInitialized;
    private static readonly Lock globalInitLock = new();

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
    private readonly SparseArray<ushort, IInputPort> inputPorts;
    private readonly SparseArray<ushort, IOutputPort> outputPorts;
    private readonly DefaultPortHandler defaultPortHandler = new();
    private readonly IVirtualDevice[] allDevices;
    private readonly ExpandedMemoryManager emm;
    private readonly ExtendedMemoryManager xmm;
    private readonly List<DmaChannel> dmaDeviceChannels = [];
    private readonly ICallbackProvider[] callbackProviders;
    private readonly MultiplexInterruptHandler multiplexHandler;
    private readonly RealTimeClockHandler realTimeClock;
    private bool disposed;
    private bool showMouse;
    private bool showCursor = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualMachine"/> class.
    /// </summary>
    public VirtualMachine() : this(null)
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualMachine"/> class.
    /// </summary>
    /// <param name="physicalMemorySize">Physical memory size in megabytes.</param>
    public VirtualMachine(VirtualMachineInitializationOptions? options)
    {
        options ??= new VirtualMachineInitializationOptions();

        lock (globalInitLock)
        {
            if (!instructionSetInitialized)
            {
                InstructionSet.Initialize();
                instructionSetInitialized = true;
            }
        }

        this.PhysicalMemory = new PhysicalMemory(options.PhysicalMemorySize * 1024 * 1024);
        this.Keyboard = new Keyboard.KeyboardDevice(this);
        this.ConsoleIn = new ConsoleInStream(this.Keyboard);
        this.Video = new Video.VideoHandler(this);
        this.ConsoleOut = new ConsoleOutStream(this.Video.TextConsole);
        this.Dos = new Dos.DosHandler(this);
        this.Mouse = new Mouse.MouseHandler(this);
        this.InterruptTimer = new InterruptTimer();
        this.emm = new ExpandedMemoryManager(this);
        this.xmm = new ExtendedMemoryManager(this);
        this.DmaController = new DmaController();
        this.Console = new VirtualConsole(this);
        this.multiplexHandler = new MultiplexInterruptHandler(this.Processor);
        this.realTimeClock = new RealTimeClockHandler(this);
        this.PhysicalMemory.Video = this.Video;

        this.Dos.InitializationComplete();

        var inputPorts = new SortedList<ushort, IInputPort>();
        var outputPorts = new SortedList<ushort, IOutputPort>();
        var callbacks = new List<ICallbackProvider>();
        var allDevices = new List<IVirtualDevice>();

        RegisterDevice(this.Dos);
        RegisterDevice(this.Video);
        RegisterDevice(this.Video.Vbe);
        RegisterDevice(this.Keyboard);
        RegisterDevice(this.Mouse);
        RegisterDevice(this.realTimeClock);
        RegisterDevice(new ErrorHandler());
        RegisterDevice(this.InterruptController);
        RegisterDevice(this.InterruptTimer);
        RegisterDevice(this.emm);
        RegisterDevice(this.DmaController);
        RegisterDevice(this.multiplexHandler);
        RegisterDevice(new BiosServices.SystemServices(this));
        RegisterDevice(this.xmm);
        RegisterDevice(new Dos.CD.Mscdex(this));
        RegisterDevice(new LowLevelDisk.LowLevelDiskInterface(this));
        foreach (var getDevice in options.AdditionalDevices)
            RegisterDevice(getDevice(this));

        this.inputPorts = new SparseArray<ushort, IInputPort>(inputPorts);
        this.outputPorts = new SparseArray<ushort, IOutputPort>(outputPorts);
        this.callbackProviders = [.. callbacks];
        this.allDevices = [.. allDevices];

        this.PhysicalMemory.AddTimerInterruptHandler();

        this.PhysicalMemory.ReserveBaseMemory();

        var comspec = this.FileSystem.CommandInterpreterPath;
        if (comspec != null)
            this.EnvironmentVariables["COMSPEC"] = comspec.ToString();

        void RegisterDevice(IVirtualDevice virtualDevice)
        {
            if (virtualDevice is IInterruptHandler interruptHandler)
            {
                foreach (var interrupt in interruptHandler.HandledInterrupts)
                {
                    interruptHandlers[interrupt.Interrupt] = interruptHandler;
                    PhysicalMemory.AddInterruptHandler(interrupt.Interrupt, interrupt.SavedRegisters, interrupt.IsHookable, interrupt.ClearInterruptFlag);
                }
            }

            if (virtualDevice is IMultiplexInterruptHandler multiplex)
                this.multiplexHandler.Handlers.Add(multiplex);

            if (virtualDevice is IInputPort inputPort)
            {
                foreach (var port in inputPort.InputPorts)
                    inputPorts[port] = inputPort;
            }

            if (virtualDevice is IOutputPort outputPort)
            {
                foreach (var port in outputPort.OutputPorts)
                    outputPorts[port] = outputPort;
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
                int id = callbacks.Count;
                callbackProvider.CallbackAddress = this.PhysicalMemory.AddCallbackHandler((byte)id, callbackProvider.IsHookable);
                callbacks.Add(callbackProvider);
            }

            allDevices.Add(virtualDevice);
            virtualDevice.DeviceRegistered(this);
        }
    }

    /// <summary>
    /// Occurs when the emulated display mode has changed.
    /// </summary>
    public event EventHandler<VideoModeChangedEventArgs>? VideoModeChanged;
    /// <summary>
    /// Occurs when the emulator sets the mouse position.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs>? MouseMoveByEmulator;
    /// <summary>
    /// Occurs when the internal mouse position has changed.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs>? MouseMove;
    /// <summary>
    /// Occurs when the mouse cursor is shown or hidden.
    /// </summary>
    public event EventHandler? MouseVisibilityChanged;
    /// <summary>
    /// Occurs when the text-mode cursor is shown or hidden.
    /// </summary>
    public event EventHandler? CursorVisibilityChanged;
    /// <summary>
    /// Occurs when the current process has changed.
    /// </summary>
    public event EventHandler? CurrentProcessChanged;
    /// <summary>
    /// Occurs when an informational message has been written to the log.
    /// </summary>
    public event EventHandler<MessageEventArgs>? MessageLogged;

    /// <summary>
    /// Gets the virtual file system used by the virtual machine.
    /// </summary>
    public FileSystem FileSystem { get; } = new FileSystem();
    /// <summary>
    /// Gets information about the current emulated video mode.
    /// </summary>
    public Video.VideoMode VideoMode => this.Video?.CurrentMode!;
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
    public EnvironmentVariables EnvironmentVariables { get; } = [];
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
    internal bool BigStackPointer { get; set; }

    /// <summary>
    /// Returns a <see cref="VideoRenderer"/> which can be used to generate RGBA bitmaps from the current video mode.
    /// </summary>
    /// <typeparam name="TPixelFormat">Output pixel format.</typeparam>
    /// <returns><see cref="VideoRenderer"/> instance for the current display mode or <see langword="null"/> if no renderer is available or the state is invalid.</returns>
    public VideoRenderer? GetRenderer<TPixelFormat>() where TPixelFormat : IOutputPixelFormat => VideoRenderer.Create<TPixelFormat>(this);

    /// <summary>
    /// Loads an executable file into the virtual machine.
    /// </summary>
    /// <param name="image">Executable file to load.</param>
    public void LoadImage(ProgramImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        Dos.LoadImage(image);
    }
    /// <summary>
    /// Loads an executable file into the virtual machine.
    /// </summary>
    /// <param name="image">Executable file to load.</param>
    /// <param name="commandLineArguments">Command line arguments for the program.</param>
    public void LoadImage(ProgramImage image, string? commandLineArguments, string? stdOut = null)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (commandLineArguments != null && commandLineArguments.Length > 255)
            throw new ArgumentException("Command line length must not exceed 255 characters.");

        Dos.LoadImage(image, commandLineArgs: commandLineArguments, stdOut: stdOut);
    }
    /// <summary>
    /// Raises a hardware/software interrupt on the virtual machine.
    /// </summary>
    /// <param name="interrupt">Interrupt to raise.</param>
    public void RaiseInterrupt(byte interrupt)
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

                uint cpl = this.Processor.CS & 3u;
                uint rpl = interruptGate.Selector & 3u;

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
        ArgumentNullException.ThrowIfNull(mouseEvent);
        mouseEvent.RaiseEvent(this.Mouse);
    }
    /// <summary>
    /// Returns an object containing information about current conventional memory usage.
    /// </summary>
    /// <returns>Information about current conventional memory usage.</returns>
    public ConventionalMemoryInfo GetConventionalMemoryUsage() => Dos.GetAllocations();
    /// <summary>
    /// Returns an object containing information about current expanded memory usage.
    /// </summary>
    /// <returns>Information about current expanded memory usage.</returns>
    public ExpandedMemoryInfo GetExpandedMemoryUsage() => new(emm.AllocatedPages);
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
    /// Updates the emulated BIOS clock with the number of 55 msec ticks since midnight.
    /// </summary>
    /// <remarks>
    /// This should be called periodically to keep the BIOS clock value correct.
    /// </remarks>
    public void UpdateRealTimeClock() => this.realTimeClock.Update();
    /// <summary>
    /// Raises the appropriate interrupt handler for a runtime exception if possible.
    /// </summary>
    /// <param name="exception">Runtime exception to raise.</param>
    /// <returns>Value indicating whether exception was handled.</returns>
    public bool RaiseException(EmulatedException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

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

    internal void CallInterruptHandler(byte interrupt)
    {
        // ensure we avoid a bounds check here as the interruptHandlers array will always contain 256 items
        var handler = Unsafe.Add(ref MemoryMarshal.GetReference(this.interruptHandlers), interrupt);
        if (handler != null)
            handler.HandleInterrupt(interrupt);
        else
            ThrowHelper.ThrowNoInterruptHandlerException(interrupt);
    }
    internal void CallCallback(byte id)
    {
        var p = this.callbackProviders;
        if (id < p.Length)
            p[id].InvokeCallback();
        else
            ThrowHelper.ThrowNoCallbackHandlerException(id);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushToStack(ushort value) => this.PushToStackGeneric(value);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushToStack32(uint value) => this.PushToStackGeneric(value);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ushort PopFromStack() => this.PopFromStack<ushort>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal uint PopFromStack32() => this.PopFromStack<uint>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PushToStackGeneric<TValue>(TValue value) where TValue : unmanaged
    {
        var p = this.Processor;
        uint address = p.GetSegmentBasePointer((int)SegmentIndex.SS);
        address = this.BigStackPointer ? AdjustStack<uint>(p, address) : AdjustStack<ushort>(p, address);
        this.PhysicalMemory.Set(address, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint AdjustStack<TAddress>(Processor p, uint baseAddress) where TAddress : unmanaged, IBinaryInteger<TAddress>
        {
            ref TAddress sp = ref Unsafe.As<uint, TAddress>(ref p.ESP);
            sp -= TAddress.CreateTruncating(Unsafe.SizeOf<TValue>());
            return baseAddress + uint.CreateTruncating(sp);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TValue PopFromStack<TValue>() where TValue : unmanaged
    {
        return this.BigStackPointer ? Pop<uint>(this.Processor, this.PhysicalMemory) : Pop<ushort>(this.Processor, this.PhysicalMemory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static TValue Pop<TAddress>(Processor processor, PhysicalMemory physicalMemory) where TAddress : unmanaged, IBinaryInteger<TAddress>
        {
            ref TAddress sp = ref Unsafe.As<uint, TAddress>(ref processor.ESP);
            uint address = processor.GetSegmentBasePointer((int)SegmentIndex.SS) + uint.CreateTruncating(sp);
            var value = physicalMemory.Get<TValue>(address);
            sp += TAddress.CreateTruncating(Unsafe.SizeOf<TValue>());
            return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddToStackPointer(uint value)
    {
        if (!this.BigStackPointer)
            this.Processor.SP += (ushort)value;
        else
            this.Processor.ESP += value;
    }
    internal ushort PeekStack16()
    {
        uint address;
        unsafe
        {
            if (!this.BigStackPointer)
                address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP;
            else
                address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP;
        }

        return this.PhysicalMemory.GetUInt16(address);
    }
    internal uint PeekStack32()
    {
        uint address;
        unsafe
        {
            if (!this.BigStackPointer)
                address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.SP;
            else
                address = this.Processor.segmentBases[(int)SegmentIndex.SS] + this.Processor.ESP;
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
    internal byte ReadPortByte(ushort port)
    {
        if (this.inputPorts!.TryGetValue(port, out var inputPort))
            return inputPort.ReadByte(port);
        else
            return this.defaultPortHandler.ReadByte(port);
    }
    internal ushort ReadPortWord(ushort port)
    {
        if (this.inputPorts!.TryGetValue(port, out var inputPort))
            return inputPort.ReadWord(port);
        else
            return 0xFFFF;
    }
    internal void WritePortByte(ushort port, byte value)
    {
        if (!this.outputPorts!.TryGetValue(port, out var outputPort))
            defaultPortHandler.WriteByte(port, value);
        else
            outputPort.WriteByte(port, value);
    }
    internal void WritePortWord(ushort port, ushort value)
    {
        if (!this.outputPorts!.TryGetValue(port, out var outputPort))
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSegmentRegister(SegmentIndex segment, ushort value)
    {
        var processor = this.Processor;
        ref ushort segmentRegisterPtr = ref processor.GetSegmentRegisterPointer((int)segment);
        ref uint basePtr = ref processor.GetSegmentBasePointer((int)segment);

        if (!processor.CR0.HasFlag(CR0.ProtectedModeEnable))
        {
            segmentRegisterPtr = value;
            basePtr = (uint)value << 4;
            this.BigStackPointer = false;
        }
        else
        {
            var descriptor = this.PhysicalMemory.GetDescriptor(value);
            if (value == 0 || descriptor.DescriptorType == DescriptorType.Segment)
            {
                var segmentDescriptor = (SegmentDescriptor)descriptor;

                if (value <= 3u || segmentDescriptor.IsPresent)
                {
                    segmentRegisterPtr = value;
                    basePtr = segmentDescriptor.Base;

                    if (segment == SegmentIndex.CS)
                    {
                        if ((segmentDescriptor.Attributes2 & SegmentDescriptor.BigMode) == 0)
                            processor.GlobalSize = 0;
                        else
                            processor.GlobalSize = 3;
                    }
                    else if (segment == SegmentIndex.SS)
                    {
                        this.BigStackPointer = (segmentDescriptor.Attributes2 & SegmentDescriptor.BigMode) != 0;
                        processor.TemporaryInterruptMask = true;
                    }
                }
                else
                {
                    ThrowHelper.ThrowSegmentNotPresentException(value);
                }
            }
            else
            {
                ThrowHelper.ThrowGeneralProtectionFaultException(value);
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
    /// Raises the <see cref="VideoModeChanged" /> event.
    /// </summary>
    /// <param name="e">Empty EventArgs instance.</param>
    internal void OnVideoModeChanged(VideoModeChangedEventArgs e)
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
        if (!this.disposed)
        {
            if (disposing)
            {
                foreach (var device in this.allDevices)
                {
                    if (device is IDisposable d)
                        d.Dispose();
                }

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
