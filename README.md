# What is Aeon?
Aeon is an x86 with DOS emulator written in 100% C#. It was originally started in 2008 as an experiment
in developing a high performance emulator fully in C#/.NET. So basically, it's like [DOSBox](https://www.dosbox.com/)
but with worse compatibility and only for Windows.

# Who should use Aeon?
C# developers might be interested in this, or fans of retro gaming that want to experiment with a different
emulator. If you're just looking to get an old game running, just use DOSBox :)

# Downloads
See the [Releases](https://github.com/gregdivis/Aeon/releases) page for the latest builds, or
get the source and build it yourself (see building instructions below).

Aeon doesn't have an installer, but does require that the .NET 6 runtime is installed.
You can download it from Microsoft at [https://dotnet.microsoft.com/download/dotnet/6.0](https://dotnet.microsoft.com/download/dotnet/6.0).

# Usage
The easiest way to get started is just to use the "quick launch program" button in the toolbar, and
browse to a DOS .exe or .com file. Launching a program this way will create a virtual environment with
the program's directory mounted as the C: drive in the emulated system.

It's also possible to quick launch a command prompt in a directory if you'd like to pass in arguments
before launching the program. Batch files are supported.

You can set up the emulated environment with more detail by creating a json configuration file with
a `.AeonConfig` extension and launching it with the quick launch program button. This format isn't
documented yet, but you can find a few basic samples [in the repo](https://github.com/gregdivis/Aeon/tree/master/examples).

# Capabilities
Aeon aims to emulate the hardware and software environment of a typical 486DX PC, which was pretty common in the early 1990s.
The following is currently emulated:

 - CPU
   - Core x86 Instruction Set
     - Nearly all instructions are implemented, but there are still some gaps (generally, I've only implemented new instructions as I find old programs that use them)
   - x87 FPU (floating point unit/instructions)
     - Not emulated with true precision (Aeon uses 64-bit floating point math rather than the 80-bit format used in the original x87)
 - Memory
   - Real Mode Memory Model
   - Protected Mode Memory Model
     - Still a number of bugs in this, but it is adequate to run most DOS applications that used common DPMI extenders
 - Video
   - Text modes: 80x25, 40x25
   - Graphical modes:
     - CGA (320x200 4-color mode 04h)
     - EGA (320x200, 640x200, 640x320 16-color modes 0Dh, 0Eh, 10h)
     - VGA (640x480 4-color, 320x200 256-color modes 12h, 13h)
     - Unchained 13h VGA (mode X)
     - SVGA VBE 2.0 (linear and windowed)
   - Display filtering: Scale2x, Scale3x
 - BIOS/System
   - 8259 interrupt controller
   - 8253/8254 interrupt timer
 - Peripherals
   - PS2 keyboard + interrupt handler
   - PS2 mouse + interrupt handler + mouse driver
   - Game port
     - Limited, currently only using DirectInput/XInput
 - DOS
   - Roughly equivalent to MS-DOS 5.0
   - Command/batch interpreter
   - Mountable drives:
     - Host directory
     - ISO image
     - BIN/CUE image
     - Host CD drive
 - Sound
   - Internal PC speaker
     - Timer-based waveform generation only, no direct access
   - OPL3/Ymf262 FM sythesis (Sound Blaster, Adlib)
   - Sound Blaster 16 DSP
     - Primarily Single/auto DMA mode
   - General MIDI using any of:
     - Windows MIDI Mapper
     - [MeltySynth](https://github.com/sinshu/meltysynth) SoundFont-based MIDI synthesis (requires SoundFont)
     - [mt32emu](https://github.com/munt/munt) Roland MT-32 emulation (requires MT32 roms)


# Building
You can build Aeon using Visual Studio 2022. It has a couple NuGet dependencies that should be fetched
automatically on build.

**Important**: Aeon will be *extremely* slow if you build in Debug configuration, and even in a Release
configuration if you have a debugger attached, as it relies heavily on inlining, intrinsics, and other
JIT compiler optimizations that get suppressed in Debug mode or with a debugger attached.
