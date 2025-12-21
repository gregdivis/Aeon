using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Sound;
using Aeon.Emulator.Sound.Blaster;
using Aeon.Emulator.Sound.FM;
using Aeon.Emulator.Sound.PCSpeaker;

namespace Aeon.Emulator.Configuration;

/// <summary>
/// Contains extension methods for <see cref="EmulatorHost"/>.
/// </summary>
public static class EmulatorHostExtensions
{
    extension(EmulatorHost)
    {
        /// <summary>
        /// Creates a new <see cref="EmulatorHost"/> instance using the global configuration and optionally additional
        /// configuration specified by the <paramref name="config"/> argument.
        /// </summary>
        /// <param name="config">Additional configuration.</param>
        /// <returns><see cref="EmulatorHost"/> instance configured using the specified options.</returns>
        public static EmulatorHost CreateWithConfig(AeonConfiguration config)
        {
            var emulator = new EmulatorHost(
                new VirtualMachineInitializationOptions
                {
                    PhysicalMemorySize = config.PhysicalMemorySize ?? 16,
                    AdditionalDevices =
                    [
                        _ => new InternalSpeaker(),
                        vm => new SoundBlaster(vm),
                        _ => new FmSoundCard(),
                        _ => new GeneralMidi(new GeneralMidiOptions(config.MidiEngine ?? MidiEngine.MidiMapper, config.SoundfontPath, config.Mt32RomsPath))
                    ]
                }
            );

            try
            {
                var fs = emulator.VirtualMachine.FileSystem;

                if (config?.Drives is not null)
                {
                    foreach (var (letter, driveConfig) in config.Drives)
                    {
                        var driveLetter = DriveLetter.Parse(letter);
                        var drive = fs.Drives[driveLetter];

                        if (string.IsNullOrEmpty(driveConfig.HostPath) || !Directory.Exists(driveConfig.HostPath))
                            throw new ArgumentException($"Host path for drive ({driveLetter}) was not found.");

                        if (driveConfig.ReadOnly)
                            drive.Mapping = new MappedFolder(driveConfig.HostPath);
                        else
                            drive.Mapping = new WritableMappedFolder(driveConfig.HostPath);

                        drive.DriveType = driveConfig.Type;
                        if (driveConfig.FreeSpace.HasValue)
                            drive.FreeSpace = driveConfig.FreeSpace.Value;

                        if (!string.IsNullOrEmpty(driveConfig.Label))
                            drive.VolumeLabel = driveConfig.Label;

                        if (drive.DriveType == DriveType.Fixed)
                            drive.HasCommandInterpreter = true;
                    }
                }

                if (config?.EmulationSpeed is not null)
                    emulator.EmulationSpeed = config.EmulationSpeed.Value;

                if (!string.IsNullOrEmpty(config?.StartupPath))
                    fs.WorkingDirectory = VirtualPath.TryParse(config.StartupPath) ?? throw new ArgumentException("Invalid startup path.");

                if (!string.IsNullOrEmpty(config?.Launch))
                {
                    var launchTargets = config.Launch.Split([' ', '\t'], 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (launchTargets.Length == 1)
                        emulator.LoadProgram(launchTargets[0]);
                    else
                        emulator.LoadProgram(launchTargets[0], launchTargets[1]);
                }
                else
                {
                    emulator.LoadProgram("COMMAND.COM");
                }

                return emulator;
            }
            catch
            {
                emulator.Dispose();
                throw;
            }
        }
    }
}
