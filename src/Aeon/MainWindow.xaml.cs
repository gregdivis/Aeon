using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Aeon.DiskImages;
using Aeon.Emulator.Configuration;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Microsoft.Win32;

namespace Aeon.Emulator.Launcher
{
    public sealed partial class MainWindow : Window, System.Windows.Forms.IWin32Window
    {
        private PerformanceWindow performanceWindow;
        private AeonConfiguration currentConfig;
        private bool hasActivated;
        private PaletteDialog paletteWindow;
        private readonly Lazy<WindowInteropHelper> interopHelper;

        public MainWindow()
        {
            this.interopHelper = new Lazy<WindowInteropHelper>(() => new WindowInteropHelper(this));
            this.InitializeComponent();
        }

        IntPtr System.Windows.Forms.IWin32Window.Handle => this.interopHelper.Value.Handle;

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!this.hasActivated)
            {
                var args = App.Current.Args;

                if (args.Count > 0)
                    QuickLaunch(args[0]);

                this.hasActivated = true;
            }

            if (emulatorDisplay != null && this.WindowState != WindowState.Minimized)
                emulatorDisplay.Focus();
        }

        private void ApplyConfiguration(AeonConfiguration config)
        {
            var globalConfig = GlobalConfiguration.Load();

            this.emulatorDisplay.EmulatorHost = new EmulatorHost(
                new VirtualMachineInitializationOptions
                {
                    AdditionalDevices =
                    [
                        _ => new Sound.PCSpeaker.InternalSpeaker(),
                        vm => new Sound.Blaster.SoundBlaster(vm),
                        _ => new Sound.FM.FmSoundCard(),
                        _ => new Sound.GeneralMidi(new Sound.GeneralMidiOptions(config.MidiEngine ?? globalConfig.MidiEngine ?? Sound.MidiEngine.MidiMapper, globalConfig.SoundfontPath, globalConfig.Mt32RomsPath))
                    ]
                }
            )
            {
                EventSynchronizer = new WpfSynchronizer(this.Dispatcher)
            };

            foreach (var (letter, info) in config.Drives)
            {
                var driveLetter = ParseDriveLetter(letter);

                var vmDrive = this.emulatorDisplay.EmulatorHost.VirtualMachine.FileSystem.Drives[driveLetter];
                vmDrive.DriveType = info.Type;
                vmDrive.VolumeLabel = info.Label;
                if (info.FreeSpace != null)
                    vmDrive.FreeSpace = info.FreeSpace.GetValueOrDefault();

                if (!string.IsNullOrEmpty(info.HostPath))
                {
                    vmDrive.Mapping = info.ReadOnly ? new MappedFolder(info.HostPath) : new WritableMappedFolder(info.HostPath);
                }
                else if (!string.IsNullOrEmpty(info.ImagePath))
                {
                    if (Path.GetExtension(info.ImagePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
                        vmDrive.Mapping = new ISOImage(info.ImagePath);
                    else if (Path.GetExtension(info.ImagePath).Equals(".cue", StringComparison.OrdinalIgnoreCase))
                        vmDrive.Mapping = new CueSheetImage(info.ImagePath);
                    else
                        throw new FormatException();
                }
                else
                {
                    throw new FormatException();
                }

                vmDrive.HasCommandInterpreter = vmDrive.DriveType == DriveType.Fixed;
            }

            this.emulatorDisplay.EmulatorHost.VirtualMachine.FileSystem.WorkingDirectory = new VirtualPath(config.StartupPath);

            emulatorDisplay.EmulationSpeed = config.EmulationSpeed ?? 100_000_000;
            emulatorDisplay.MouseInputMode = config.IsMouseAbsolute.GetValueOrDefault() ? MouseInputMode.Absolute : MouseInputMode.Relative;
            if (!string.IsNullOrEmpty(config.Title))
                this.Title = config.Title;

            static DriveLetter ParseDriveLetter(string s)
            {
                if (string.IsNullOrEmpty(s))
                    throw new ArgumentNullException(nameof(s));
                if (s.Length != 1)
                    throw new FormatException();

                return new DriveLetter(s[0]);
            }
        }
        private void LaunchCurrentConfig()
        {
            ApplyConfiguration(this.currentConfig);
            if (!string.IsNullOrEmpty(this.currentConfig.Launch))
            {
                var launchTargets = this.currentConfig.Launch.Split([' ', '\t'], 2, StringSplitOptions.RemoveEmptyEntries);
                if (launchTargets.Length == 1)
                    this.emulatorDisplay.EmulatorHost.LoadProgram(launchTargets[0]);
                else
                    this.emulatorDisplay.EmulatorHost.LoadProgram(launchTargets[0], launchTargets[1]);
            }
            else
            {
                this.emulatorDisplay.EmulatorHost.LoadProgram("COMMAND.COM");
            }

            this.emulatorDisplay.EmulatorHost.Run();
        }
        private void QuickLaunch(string fileName)
        {
            bool hasConfig = fileName.EndsWith(".AeonConfig", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".AeonPack", StringComparison.OrdinalIgnoreCase);
            if (hasConfig)
                this.currentConfig = AeonConfiguration.Load(fileName);
            else
                this.currentConfig = AeonConfiguration.GetQuickLaunchConfiguration(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));

            this.LaunchCurrentConfig();
        }
        private TaskDialogItem ShowTaskDialog(string title, string caption, params TaskDialogItem[] items)
        {
            var taskDialog = new TaskDialog { Owner = this, Items = items, Icon = this.Icon, Title = title, Caption = caption };
            if (taskDialog.ShowDialog() == true)
                return taskDialog.SelectedItem;
            else
                return null;
        }

        private void QuickLaunch_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Programs (*.exe,*.com;*.AeonConfig;*.AeonPack)|*.exe;*.com;*.AeonConfig;*.AeonPack|All files (*.*)|*.*",
                Title = "Run DOS program..."
            };

            if (fileDialog.ShowDialog(this) == true)
                this.QuickLaunch(fileDialog.FileName);
        }
        private void CommandPrompt_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder for C:\\ drive...",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.currentConfig = AeonConfiguration.GetQuickLaunchConfiguration(dialog.SelectedPath, null);
                this.LaunchCurrentConfig();
            }
        }
        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e) => this.Close();
        private void MapDrives_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.emulatorDisplay != null)
            {
                var state = this.emulatorDisplay.EmulatorState;
                e.CanExecute = state == EmulatorState.Running || state == EmulatorState.Paused;
            }
        }
        private void EmulatorDisplay_EmulatorStateChanged(object sender, RoutedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
            if (this.emulatorDisplay.EmulatorState == EmulatorState.ProgramExited && this.currentConfig != null)
                this.Close();
        }
        private void SlowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (emulatorDisplay != null)
            {
                int newSpeed = Math.Max(EmulatorHost.MinimumSpeed, emulatorDisplay.EmulationSpeed - 100_000);
                if (newSpeed != emulatorDisplay.EmulationSpeed)
                    emulatorDisplay.EmulationSpeed = newSpeed;
            }
        }
        private void FasterButton_Click(object sender, RoutedEventArgs e)
        {
            if (emulatorDisplay != null)
            {
                int newSpeed = emulatorDisplay.EmulationSpeed + 100_000;
                if (newSpeed != emulatorDisplay.EmulationSpeed)
                    emulatorDisplay.EmulationSpeed = newSpeed;
            }
        }
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (emulatorDisplay != null && emulatorDisplay.DisplayBitmap != null)
                e.CanExecute = true;
        }
        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (emulatorDisplay != null)
            {
                var bmp = emulatorDisplay.DisplayBitmap;
                if (bmp != null)
                    Clipboard.SetImage(bmp);
            }
        }
        private void FullScreen_Executed(object sener, ExecutedRoutedEventArgs e)
        {
            if (this.WindowStyle != WindowStyle.None)
            {
                this.menuContainer.Visibility = Visibility.Collapsed;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                this.SetCurrentValue(BackgroundProperty, Brushes.Black);
            }
            else
            {
                this.menuContainer.Visibility = Visibility.Visible;
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.SetCurrentValue(BackgroundProperty, this.FindResource("backgroundGradient"));
            }
        }
        private void EmulatorDisplay_EmulationError(object sender, EmulationErrorRoutedEventArgs e)
        {
            var end = new TaskDialogItem("End Program", "Terminates the current emulation session.");
            var selection = ShowTaskDialog("Emulation Error", "An error occurred which caused the emulator to halt: " + e.Message + " What would you like to do?", end);
        }
        private void EmulatorDisplay_CurrentProcessChanged(object sender, RoutedEventArgs e)
        {
            if (this.currentConfig == null || string.IsNullOrEmpty(this.currentConfig.Title))
            {
                var process = emulatorDisplay.CurrentProcess;
                if (process != null)
                    this.Title = $"{process} - Aeon";
                else
                    this.Title = "Aeon";
            }
        }
        private void PerformanceWindow_Click(object sender, RoutedEventArgs e)
        {
            if (performanceWindow != null)
                performanceWindow.Activate();
            else
            {
                performanceWindow = new PerformanceWindow();
                performanceWindow.Closed += this.PerformanceWindow_Closed;
                performanceWindow.Owner = this;
                performanceWindow.EmulatorDisplay = emulatorDisplay;
                performanceWindow.Show();
            }
        }
        private void PerformanceWindow_Closed(object sender, EventArgs e)
        {
            if (performanceWindow != null)
            {
                performanceWindow.Closed -= this.PerformanceWindow_Closed;
                performanceWindow = null;
            }
        }
        private void ShowPalette_Click(object sender, RoutedEventArgs e)
        {
            if (this.paletteWindow != null)
            {
                this.paletteWindow.Activate();
            }
            else
            {
                this.paletteWindow = new PaletteDialog { Owner = this, EmulatorDisplay = this.emulatorDisplay, Icon = this.Icon };
                this.paletteWindow.Closed += PaletteWindow_Closed;
                paletteWindow.Show();
            }
        }
        private void PaletteWindow_Closed(object sender, EventArgs e)
        {
            if (this.paletteWindow != null)
            {
                this.paletteWindow.Closed -= this.PaletteWindow_Closed;
                this.paletteWindow = null;
            }
        }
    }
}
