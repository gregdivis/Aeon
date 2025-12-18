using System.Collections.Concurrent;
using System.Diagnostics;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator;

/// <summary>
/// Hosts a <see cref="Aeon.Emulator.VirtualMachine"/> instance and provides additional services.
/// </summary>
public sealed class EmulatorHost : IDisposable, IAsyncDisposable
{
    private Task? processorTask;
    private volatile EmulatorState targetState;
    private readonly ConcurrentQueue<MouseEvent> mouseQueue = new();
    private EmulatorState currentState;
    private bool disposed;
    private long totalInstructions;
    private readonly SortedSet<Keys> keysPresssed = [];
    private int emulationSpeed = 10_000_000;

    /// <summary>
    /// The smallest number that may be assigned to the EmulationSpeed property.
    /// </summary>
    public const int MinimumSpeed = 1_000_000;

    /// <summary>
    /// Initializes a new instance of the EmulatorHost class.
    /// </summary>
    public EmulatorHost() : this(new VirtualMachine())
    {
    }
    public EmulatorHost(VirtualMachineInitializationOptions? options)
        : this(new VirtualMachine(options))
    {
    }
    /// <summary>
    /// Initializes a new instance of the EmulatorHost class.
    /// </summary>
    /// <param name="virtualMachine">VirtualMachine instance to host.</param>
    /// <param name="instructionLog">Log which will be used to record instructions.</param>
    public EmulatorHost(VirtualMachine virtualMachine)
    {
        this.VirtualMachine = virtualMachine ?? throw new ArgumentNullException(nameof(virtualMachine));
        this.VirtualMachine.VideoModeChanged += (s, e) => this.OnVideoModeChanged(e);
        this.VirtualMachine.MouseMoveByEmulator += (s, e) => this.OnMouseMoveByEmulator(e);
        this.VirtualMachine.MouseMove += (s, e) => this.OnMouseMove(e);
        this.VirtualMachine.MouseVisibilityChanged += (s, e) => this.OnMouseVisibilityChanged(e);
        this.VirtualMachine.CursorVisibilityChanged += (s, e) => this.OnCursorVisibilityChanged(e);
        this.VirtualMachine.CurrentProcessChanged += (s, e) => this.OnCurrentProcessChanged(e);
    }

    /// <summary>
    /// Occurs when the emulated display mode has changed.
    /// </summary>
    public event EventHandler? VideoModeChanged;
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
    /// Occurs when the emulator state has changed.
    /// </summary>
    public event EventHandler? StateChanged;
    /// <summary>
    /// Occurs when the emulator has halted due to an error.
    /// </summary>
    public event EventHandler<ErrorEventArgs>? Error;

    /// <summary>
    /// Gets the current state of the emulated system.
    /// </summary>
    public EmulatorState State
    {
        get => currentState;
        private set
        {
            if (value != currentState)
            {
                currentState = value;
                ThreadPool.QueueUserWorkItem(state => OnStateChanged(EventArgs.Empty));
            }
        }
    }
    /// <summary>
    /// Gets the hosted VirtualMachine instance.
    /// </summary>
    public VirtualMachine VirtualMachine { get; }
    /// <summary>
    /// Gets the total number of instructions executed.
    /// </summary>
    public long TotalInstructions => totalInstructions;
    /// <summary>
    /// Gets or sets the current emulation speed.
    /// </summary>
    public int EmulationSpeed
    {
        get => this.emulationSpeed;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, MinimumSpeed);
            this.emulationSpeed = value;
        }
    }
    /// <summary>
    /// Gets or sets the object to use for raising events.
    /// </summary>
    public IEventSynchronizer? EventSynchronizer { get; set; }

    /// <summary>
    /// Loads an executable program image into the emulator.
    /// </summary>
    /// <param name="fileName">Name of executable file to load.</param>
    public void LoadProgram(string fileName) => this.LoadProgram(fileName, null);
    /// <summary>
    /// Loads an executable program image into the emulator.
    /// </summary>
    /// <param name="fileName">Name of executable file to load.</param>
    /// <param name="commandLineArguments">Command line arguments for the program.</param>
    public void LoadProgram(string fileName, string? commandLineArguments)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        if (commandLineArguments != null && commandLineArguments.Length > 127)
            throw new ArgumentException("Command line length must not exceed 127 characters.");

        var image = ProgramImage.Load(fileName, VirtualMachine);
        this.VirtualMachine.LoadImage(image, commandLineArguments);
        this.State = EmulatorState.Ready;
    }
    /// <summary>
    /// Starts or resumes emulation.
    /// </summary>
    public void Run()
    {
        if (this.State == EmulatorState.NoProgram)
            throw new InvalidOperationException("No program has been loaded.");
        if (this.State == EmulatorState.ProgramExited)
            throw new InvalidOperationException("The program has completed.");

        if (this.State == EmulatorState.Ready)
        {
            var comspec = this.VirtualMachine.FileSystem.CommandInterpreterPath;
            if (comspec != null)
                this.VirtualMachine.EnvironmentVariables["COMSPEC"] = comspec.ToString();

            this.processorTask = Task.Run(() => this.ProcessorThreadMainAsync(false));
        }
        else if (this.State == EmulatorState.Paused)
        {
            this.targetState = EmulatorState.Running;
            this.processorTask = Task.Run(() => this.ProcessorThreadMainAsync(true));
        }
    }
    /// <summary>
    /// Pauses emulation.
    /// </summary>
    public void Pause() => this.PauseAsync().Wait();
    /// <summary>
    /// Pauses emulation.
    /// </summary>
    public Task PauseAsync()
    {
        if (this.State != EmulatorState.Running)
            throw new InvalidOperationException("No program is running.");

        this.targetState = EmulatorState.Paused;
        return this.processorTask ?? Task.CompletedTask;
    }
    /// <summary>
    /// Immediately stops emulation and places the emulator in a halted state.
    /// </summary>
    public void Halt()
    {
        this.State = EmulatorState.Halted;
        this.targetState = EmulatorState.Halted;
    }
    /// <summary>
    /// Presses a key on the emulated keyboard.
    /// </summary>
    /// <param name="key">Key to press.</param>
    public void PressKey(Keys key)
    {
        if (this.keysPresssed.Add(key))
            this.VirtualMachine.PressKey(key);
    }
    /// <summary>
    /// Releases a key on the emulated keyboard.
    /// </summary>
    /// <param name="key">Key to release.</param>
    public void ReleaseKey(Keys key)
    {
        if (this.keysPresssed.Remove(key))
            this.VirtualMachine.ReleaseKey(key);
    }
    /// <summary>
    /// Releases all currently pressed keyboard keys.
    /// </summary>
    public void ReleaseAllKeys()
    {
        var pressedKeys = this.keysPresssed.ToList();
        foreach (var key in pressedKeys)
            this.ReleaseKey(key);
    }
    /// <summary>
    /// Signals that a mouse input event has occurred.
    /// </summary>
    /// <param name="mouseEvent">Mouse input event that has occurred.</param>
    public void MouseEvent(MouseEvent mouseEvent)
    {
        ArgumentNullException.ThrowIfNull(mouseEvent);
        this.mouseQueue.Enqueue(mouseEvent);
    }
    /// <summary>
    /// Releases resources used by the emulator.
    /// </summary>
    public void Dispose() => this.Dispose(true);
    /// <summary>
    /// Releases resources used by the emulator.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!this.disposed)
        {
            this.Halt();
            var task = this.processorTask;
            if (task != null)
                await task.ConfigureAwait(false);
            this.VirtualMachine?.Dispose();
            this.disposed = true;
        }
    }

    private void EmulateInstructions(int count)
    {
        var vm = this.VirtualMachine;
        vm.PerformDmaTransfers();

        var p = vm.Processor;

        try
        {
            vm.UpdateRealTimeClock();

            if (p.Flags.InterruptEnable & !p.TemporaryInterruptMask)
            {
                while (p.InPrefix)
                    vm.Emulate();

                // check flags again in case prefixed instruction changed them
                if (p.Flags.InterruptEnable & !p.TemporaryInterruptMask)
                    this.CheckHardwareInterrupts();
            }

            vm.Emulate(count);
        }
        catch (EmulatedException ex)
        {
            if (!vm.RaiseException(ex))
                throw;
        }
        catch (EnableInstructionTrapException)
        {
            vm.Emulate();
            while (p.InPrefix)
                vm.Emulate();

            if (p.Flags.Trap)
            {
                vm.RaiseInterrupt(1);
                p.Flags.Trap = false;
            }
        }
    }
    private void CheckHardwareInterrupts()
    {
        if (!this.RaiseMouseEvent())
        {
            var vm = this.VirtualMachine;

            if (!vm.Keyboard.IsHardwareQueueEmpty)
                vm.InterruptController.RaiseHardwareInterrupt(1);

            int irq = vm.InterruptController.AcknowledgeRequest();
            if (irq >= 0)
            {
                vm.RaiseInterrupt((byte)irq);

                if (irq == vm.InterruptController.BaseInterruptVector1 + 1)
                    vm.PrepareForKeyboardHandler();
            }
        }
    }
    private void OnVideoModeChanged(EventArgs e)
    {
        var videoModeChanged = this.VideoModeChanged;
        if (videoModeChanged != null)
            this.RaiseEvent(videoModeChanged, e);
    }
    private void OnMouseMoveByEmulator(MouseMoveEventArgs e)
    {
        var mouseMove = this.MouseMoveByEmulator;
        if (mouseMove != null)
            this.RaiseEvent(mouseMove, e);
    }
    private void OnMouseMove(MouseMoveEventArgs e)
    {
        var mouseMove = this.MouseMove;
        if (mouseMove != null)
            this.RaiseEvent(mouseMove, e);
    }
    private void OnMouseVisibilityChanged(EventArgs e)
    {
        var visChanged = this.MouseVisibilityChanged;
        if (visChanged != null)
            this.RaiseEvent(visChanged, e);
    }
    private void OnCursorVisibilityChanged(EventArgs e)
    {
        var visChanged = this.CursorVisibilityChanged;
        if (visChanged != null)
            this.RaiseEvent(visChanged, e);
    }
    private void OnCurrentProcessChanged(EventArgs e)
    {
        var processChanged = this.CurrentProcessChanged;
        if (processChanged != null)
            this.RaiseEvent(processChanged, e);
    }
    private void OnStateChanged(EventArgs e)
    {
        var stateChanged = this.StateChanged;
        if (stateChanged != null)
            this.RaiseEvent(stateChanged, e);
    }
    private void OnError(ErrorEventArgs e)
    {
        var error = this.Error;
        if (error != null)
            this.RaiseEvent(error, e);
    }
    private void RaiseEvent(Delegate method, EventArgs e)
    {
        var sync = this.EventSynchronizer;
        if (sync == null)
            method.DynamicInvoke(this, e);
        else
            sync.BeginInvoke(method, this, e);
    }
    private void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                this.Halt();
                this.processorTask?.GetAwaiter().GetResult();
                this.VirtualMachine?.Dispose();
            }

            this.disposed = true;
        }
    }

    private async Task ProcessorThreadMainAsync(bool resume)
    {
        try
        {
            this.State = EmulatorState.Running;
            this.State = await this.EmulationLoopAsync(resume).ConfigureAwait(false);
        }
        catch (EndOfProgramException)
        {
            this.State = EmulatorState.ProgramExited;
            return;
        }
        catch (NotImplementedException ex)
        {
            this.State = EmulatorState.Halted;
            this.OnError(new ErrorEventArgs(ex.Message));
            return;
        }
        catch (Exception ex)
        {
            this.State = EmulatorState.Halted;
            this.OnError(new ErrorEventArgs("Unknown error: " + ex.Message));
            return;
        }
    }
    private async Task<EmulatorState> EmulationLoopAsync(bool resume)
    {
        var speedTimer = new Stopwatch();
        var vm = this.VirtualMachine;

        if (resume)
        {
            foreach (var device in vm.Devices)
                await device.ResumeAsync().ConfigureAwait(false);
        }
        else
        {
            vm.InterruptTimer.Reset();
        }

        while (true)
        {
            this.EmulationTightLoop(speedTimer);

            if (this.targetState == EmulatorState.Paused || this.targetState == EmulatorState.Halted)
            {
                foreach (var device in vm.Devices)
                    await device.PauseAsync().ConfigureAwait(false);

                return this.targetState;
            }
        }
    }
    private void EmulationTightLoop(Stopwatch speedTimer)
    {
        const int InstructionBatchCount = 500;
        const int Iterations = 5;
        const int IterationInstructionCount = InstructionBatchCount * Iterations;

        var vm = this.VirtualMachine;
        var interruptTimer = vm.InterruptTimer;

        speedTimer.Restart();

        for (int i = 0; i < Iterations; i++)
        {
            if (interruptTimer.IsIntervalComplete)
            {
                // Raise the hardware timer interrupt.
                vm.InterruptController.RaiseHardwareInterrupt(0);
                interruptTimer.Reset();
            }

            this.EmulateInstructions(InstructionBatchCount);

            Interlocked.Add(ref totalInstructions, InstructionBatchCount);

            if (this.targetState == EmulatorState.Halted)
                return;
        }

        double ticksPerInst = TimeSpan.TicksPerSecond / (double)this.emulationSpeed;
        long targetTicks = (long)(ticksPerInst * IterationInstructionCount);

        while (speedTimer.ElapsedTicks < targetTicks)
        {
            Thread.SpinWait(10);
        }
    }

    private bool RaiseMouseEvent()
    {
        if (this.mouseQueue.TryDequeue(out var mouseEvent))
        {
            this.VirtualMachine.MouseEvent(mouseEvent);
            return true;
        }
        else
        {
            return false;
        }
    }
}
