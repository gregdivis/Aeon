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
before launching the program. Aeon's DOS command interpreter is very limited, but can perform basic tasks.
Batch files are not currently supported.

You can set up the emulated environment with more detail by creating a json configuration file with
a `.AeonConfig` extension and launching it with the quick launch program button. This format isn't
documented yet, but you can find a few basic samples [in the repo](https://github.com/gregdivis/Aeon/tree/master/examples).

# Building
You can build Aeon using Visual Studio 2022. It has a couple NuGet dependencies that should be fetched
automatically on build.

**Important**: Aeon will be *extremely* slow if you build in Debug configuration, and even in a Release
configuration if you have a debugger attached, as it relies heavily on inlining, intrinsics, and other
JIT compiler optimizations that get suppressed in Debug mode or with a debugger attached.
