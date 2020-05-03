using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Dos.CD
{
    /// <summary>
    /// Emulates MSCDEX functions.
    /// </summary>
    internal sealed class Mscdex : IMultiplexInterruptHandler
    {
        private VirtualMachine vm;
        private ReservedBlock deviceHeader;

        int IMultiplexInterruptHandler.Identifier => 0x15;

        void IMultiplexInterruptHandler.HandleInterrupt()
        {
            switch (vm.Processor.AL)
            {
                case Functions.GetCDRomDeviceList:
                    GetCDRomDeviceList();
                    break;

                case Functions.GetNumberOfCDRomDrives:
                    vm.Processor.BX = (short)GetCDRomDrives().Count();
                    if (vm.Processor.BX != 0)
                        vm.Processor.CX = (short)(GetCDRomDrives().First().Key - 'A');
                    break;

                case Functions.GetCDRomDriveLetters:
                    GetCDRomDriveLetters();
                    break;

                case Functions.CDRomDriveCheck:
                    vm.Processor.BX = unchecked((short)0xADAD);
                    vm.Processor.AX = vm.FileSystem.Drives[vm.Processor.CX].DriveType == DriveType.CDROM ? (short)1 : (short)0;
                    break;

                case Functions.MSCDEXVersion:
                    vm.Processor.BX = 0x020A; // Version 2.1
                    break;

                case Functions.GetDirectoryEntry:
                    GetDirectoryEntry();
                    SaveFlags(EFlags.Carry);
                    break;

                case Functions.AbsoluteDiskRead:
                    AbsoluteDiskRead();
                    SaveFlags(EFlags.Carry);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"MSCDEX function {vm.Processor.AL:X2}h not implemented.");
                    break;
            }
        }
        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
            this.vm = vm;
            this.deviceHeader = vm.PhysicalMemory.Reserve(0xF000, 22);
            vm.PhysicalMemory.SetUInt32(this.deviceHeader.Segment, 0, uint.MaxValue);
            vm.PhysicalMemory.SetUInt16(this.deviceHeader.Segment, 4, 0xC800);
            vm.PhysicalMemory.SetString(this.deviceHeader.Segment, 10, "AEONVMCD", false);
        }
        void IDisposable.Dispose()
        {
        }

        /// <summary>
        /// Returns a collection of all of the CD-ROM drives currently defined in the emulated system.
        /// </summary>
        /// <returns>Collection of all of the CD-ROM drives currently defined in the emulated system.</returns>
        private IEnumerable<KeyValuePair<char, VirtualDrive>> GetCDRomDrives()
        {
            for (char d = 'A'; d <= 'Z'; d++)
            {
                if (this.vm.FileSystem.Drives[new DriveLetter(d)].DriveType == DriveType.CDROM)
                    yield return new KeyValuePair<char, VirtualDrive>(d, this.vm.FileSystem.Drives[new DriveLetter(d)]);
            }
        }
        /// <summary>
        /// Writes all of the CD-ROM drive letters to ES:BX.
        /// </summary>
        private void GetCDRomDriveLetters()
        {
            uint offset = (ushort)vm.Processor.BX;
            foreach (var drive in GetCDRomDrives())
            {
                vm.PhysicalMemory.SetByte(vm.Processor.ES, offset, (byte)(drive.Key - 'A'));
                offset++;
            }
        }
        /// <summary>
        /// Writes the CD-ROM device list to ES:BX.
        /// </summary>
        private void GetCDRomDeviceList()
        {
            // First make sure the device header is correct.
            if (GetCDRomDrives().Count() > 0)
                vm.PhysicalMemory.SetByte(this.deviceHeader.Segment, 20, (byte)(GetCDRomDrives().First().Key - 'A' + 1));

            vm.PhysicalMemory.SetByte(this.deviceHeader.Segment, 21, (byte)GetCDRomDrives().Count());

            // Now write the actual device list.
            uint offset = (ushort)vm.Processor.BX;
            byte subunit = 0;

            foreach (var drive in GetCDRomDrives())
            {
                vm.PhysicalMemory.SetByte(vm.Processor.ES, offset++, subunit++);
                vm.PhysicalMemory.SetUInt32(vm.Processor.ES, offset, (uint)this.deviceHeader.Segment << 16);
                offset += 4;
            }
        }
        /// <summary>
        /// Writes the directory entry for the file with path ES:BX to SI:DI.
        /// </summary>
        private void GetDirectoryEntry()
        {
            var drive = vm.FileSystem.Drives[(int)vm.Processor.CX];
            if (drive.DriveType != DriveType.CDROM)
            {
                vm.Processor.AX = 15;
                vm.Processor.Flags.Carry = true;
                return;
            }

            var fileName = vm.PhysicalMemory.GetString(vm.Processor.ES, (ushort)vm.Processor.BX, 512, 0);
            var path = new VirtualPath(fileName);
            var fileInfo = drive.GetFileInfo(path);
            if (fileInfo.Result == null)
            {
                vm.Processor.AX = (short)fileInfo.ErrorCode;
                vm.Processor.Flags.Carry = true;
                return;
            }

            vm.Processor.AX = 1; // ISO-9660

            unsafe
            {
                string identifier;
                var entry = (DirectoryEntry*)vm.PhysicalMemory.GetPointer(vm.Processor.SI, vm.Processor.DI).ToPointer();
                if (fileInfo.Result is IIso9660DirectoryEntry isoEntry)
                {
                    identifier = isoEntry.Identifier;
                    entry->XAR_len = isoEntry.ExtendedAttributeRecordLength;
                    entry->loc_extentI = isoEntry.LBALocation;
                    entry->data_lenI = isoEntry.DataLength;
                    if (isoEntry.RecordingDate != null)
                    {
                        var recordingDate = (DateTimeOffset)isoEntry.RecordingDate;
                        entry->record_time[0] = (byte)(recordingDate.Year - 1900);
                        entry->record_time[1] = (byte)recordingDate.Month;
                        entry->record_time[2] = (byte)recordingDate.Day;
                        entry->record_time[3] = (byte)recordingDate.Hour;
                        entry->record_time[4] = (byte)recordingDate.Minute;
                        entry->record_time[5] = (byte)recordingDate.Second;
                        entry->record_time[6] = (byte)(sbyte)(recordingDate.Offset.TotalMinutes / 15);
                    }
                    else
                    {
                        for (int i = 0; i < 7; i++)
                            entry->record_time[i] = 0;
                    }

                    entry->file_flags_iso = isoEntry.FileFlags;
                    entry->il_size = isoEntry.InterleavedUnitSize;
                    entry->il_skip = isoEntry.InterleavedGapSize;
                    entry->VSSNI = isoEntry.VolumeSequenceNumber;
                }
                else
                {
                    identifier = fileInfo.Result.Name;
                    if ((fileInfo.Result.Attributes & VirtualFileAttributes.Directory) == 0)
                        identifier += ";1";
                    entry->XAR_len = 0;
                    entry->loc_extentI = 0;
                    entry->data_lenI = fileInfo.Result.DosLength;
                    entry->record_time[0] = (byte)(fileInfo.Result.ModifyDate.Year - 1900);
                    entry->record_time[1] = (byte)fileInfo.Result.ModifyDate.Month;
                    entry->record_time[2] = (byte)fileInfo.Result.ModifyDate.Day;
                    entry->record_time[3] = (byte)fileInfo.Result.ModifyDate.Hour;
                    entry->record_time[4] = (byte)fileInfo.Result.ModifyDate.Minute;
                    entry->record_time[5] = (byte)fileInfo.Result.ModifyDate.Second;
                    entry->record_time[6] = (byte)(sbyte)(TimeZoneInfo.Local.GetUtcOffset(fileInfo.Result.ModifyDate).TotalMinutes / 15);
                    entry->file_flags_iso = 0;
                    if ((fileInfo.Result.Attributes & VirtualFileAttributes.Hidden) != 0)
                        entry->file_flags_iso |= 1;
                    if ((fileInfo.Result.Attributes & VirtualFileAttributes.Directory) != 0)
                        entry->file_flags_iso |= 2;
                    entry->il_size = 0;
                    entry->il_skip = 0;
                    entry->VSSNI = 0;
                    entry->VSSNM = 0;
                }

                entry->loc_extentM = (uint)System.Net.IPAddress.HostToNetworkOrder((int)entry->loc_extentI);
                entry->data_lenM = (uint)System.Net.IPAddress.HostToNetworkOrder((int)entry->data_lenI);
                entry->VSSNM = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)entry->VSSNI);
                entry->len_fi = (byte)identifier.Length;
                vm.PhysicalMemory.SetString(vm.Processor.SI, (uint)(vm.Processor.DI + sizeof(DirectoryEntry)), identifier, (identifier.Length % 2) == 0);
            }

            vm.Processor.Flags.Carry = false;
        }
        /// <summary>
        /// Reads sectors from the disc to ES:BX.
        /// </summary>
        private void AbsoluteDiskRead()
        {
            int driveIndex = this.vm.Processor.CX;
            if (driveIndex < 0 || driveIndex >= 26 || this.vm.FileSystem.Drives[driveIndex].DriveType != DriveType.CDROM)
            {
                this.vm.Processor.AX = 15;
                this.vm.Processor.Flags.Carry = true;
                return;
            }

            var drive = this.vm.FileSystem.Drives[driveIndex].Mapping as IRawSectorReader;
            if (drive == null)
            {
                System.Diagnostics.Debug.WriteLine("Tried to read raw sectors from a device that does not implement IRawSectorReader");
                this.vm.Processor.AX = 21;
                this.vm.Processor.Flags.Carry = true;
                return;
            }

            int sectorsToRead = (ushort)this.vm.Processor.DX;
            int startingSector = (this.vm.Processor.SI << 16) | this.vm.Processor.DI;
            var buffer = new byte[sectorsToRead * drive.SectorSize];
            drive.ReadSectors(startingSector, sectorsToRead, buffer, 0);
            Thread.Sleep(5);

            var ptr = this.vm.PhysicalMemory.GetPointer(this.vm.Processor.ES, (ushort)this.vm.Processor.BX);
            Marshal.Copy(buffer, 0, ptr, buffer.Length);
        }
        private void SaveFlags(EFlags modified)
        {
            var oldFlags = (EFlags)vm.PhysicalMemory.GetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4));
            oldFlags &= ~modified;
            vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)(oldFlags | (vm.Processor.Flags.Value & modified)));
        }
    }
}
