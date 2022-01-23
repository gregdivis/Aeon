using System;
using System.Collections.Generic;
using System.Threading;

namespace Aeon.Emulator.LowLevelDisk
{
    internal sealed class LowLevelDiskInterface : IInterruptHandler, IDisposable
    {
        private VirtualMachine vm;
        private readonly Timer diskMotorTimer;

        public LowLevelDiskInterface() => this.diskMotorTimer = new Timer(this.UpdateDiskMotorTimer);

        public IEnumerable<InterruptHandlerInfo> HandledInterrupts => new InterruptHandlerInfo[] { 0x13 };

        public void HandleInterrupt(int interrupt)
        {
            switch (vm.Processor.AH)
            {
                case Functions.GetDriveParameters:
                    GetDriveParameters();
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Low-level disk function {vm.Processor.AH:X2}h not implemented.");
                    break;
            }
        }
        public void Pause() => this.diskMotorTimer.Change(Timeout.Infinite, Timeout.Infinite);
        public void Resume() => this.diskMotorTimer.Change(5000, Timeout.Infinite);
        public void DeviceRegistered(VirtualMachine vm)
        {
            this.vm = vm;
            this.diskMotorTimer.Change(5000, Timeout.Infinite);
        }
        public void Dispose() => this.diskMotorTimer.Dispose();

        private void UpdateDiskMotorTimer(object state)
        {
            this.vm.PhysicalMemory.Bios.DiskMotorTimer = 0;
            this.diskMotorTimer.Change(5000, Timeout.Infinite);
        }
        private void GetDriveParameters()
        {
            //if((vm.Processor.DL & 0x80) != 0) // Hard drive
            //{
            //    var drive = vm.FileSystem.Drives.Where(d => d.DriveType == DriveType.Fixed).FirstOrDefault();
            //    if(drive != null)
            //    {
            //        vm.Processor.AX = 0;
            //        vm.Processor.CH = (byte)(drive.MagneticDriveInfo.Cylinders & 0xFF);
            //        vm.Processor.CL = (byte)(drive.MagneticDriveInfo.Sectors & 0x3F);
            //        vm.Processor.CL |= (byte)((drive.MagneticDriveInfo.Cylinders >> 2) & 0xC0);
            //        vm.Processor.DH = (byte)drive.MagneticDriveInfo.Heads;
            //        vm.Processor.DL = 1;
            //    }
            //}

            vm.Processor.AX = 0x07FF;
            vm.Processor.Flags.Carry = true;
            SaveFlags(EFlags.Carry);
        }
        private void SaveFlags(EFlags modified)
        {
            var oldFlags = (EFlags)vm.PhysicalMemory.GetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4));
            oldFlags &= ~modified;
            vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)(oldFlags | (vm.Processor.Flags.Value & modified)));
        }
    }
}
