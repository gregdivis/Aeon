using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Dos.CD;

/// <summary>
/// Emulates MSCDEX functions.
/// </summary>
internal sealed class Mscdex(VirtualMachine vm) : IMultiplexInterruptHandler
{
    private const ushort Status_Done = 1 << 8;
    private const int LeadInSectors = 200;

    private readonly VirtualMachine vm = vm;
    private ReservedBlock? deviceHeader;
    private readonly List<IAudioCD> paused = [];

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

            case Functions.SendDeviceRequest:
                this.SendDeviceRequest();
                break;

            default:
                System.Diagnostics.Debug.WriteLine($"MSCDEX function {vm.Processor.AL:X2}h not implemented.");
                break;
        }
    }
    void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
    {
        this.deviceHeader = vm.PhysicalMemory.Reserve(0xF000, 22);
        vm.PhysicalMemory.SetUInt32(this.deviceHeader.Segment, 0, uint.MaxValue);
        vm.PhysicalMemory.SetUInt16(this.deviceHeader.Segment, 4, 0xC800);
        vm.PhysicalMemory.SetString(this.deviceHeader.Segment, 10, "AEONVMCD", false);
    }
    Task IVirtualDevice.PauseAsync()
    {
        foreach (var drive in vm.FileSystem.Drives)
        {
            if (drive.Mapping is IAudioCD cd && cd.Playing)
            {
                cd.Stop();
                this.paused.Add(cd);
            }
        }

        return Task.CompletedTask;
    }
    Task IVirtualDevice.ResumeAsync()
    {
        foreach (var cd in this.paused)
            cd.Play();

        this.paused.Clear();
        return Task.CompletedTask;
    }

    private void SendDeviceRequest()
    {
        var drive = this.vm.FileSystem.Drives[vm.Processor.CX];
        if (drive.DriveType != DriveType.CDROM || drive.Mapping is not IAudioCD cd)
        {
            System.Diagnostics.Debug.WriteLine("Drive does not contain an audio CD.");
            return;
        }

        var reqHeaderAddress = new RealModeAddress(vm.Processor.ES, (ushort)vm.Processor.BX);
        int headerSize = this.vm.PhysicalMemory.GetByte(reqHeaderAddress.Segment, reqHeaderAddress.Offset);
        var fullHeader = this.vm.PhysicalMemory.GetSpan(reqHeaderAddress.Segment, reqHeaderAddress.Offset, headerSize + 18);
        int commandCode = fullHeader[2];
        ref ushort status = ref Unsafe.As<byte, ushort>(ref fullHeader[3]);
        ushort dataSegment = BinaryPrimitives.ReadUInt16LittleEndian(fullHeader.Slice(0x10, 2));
        ushort dataOffset = BinaryPrimitives.ReadUInt16LittleEndian(fullHeader.Slice(0x0E, 2));
        ushort dataLength = BinaryPrimitives.ReadUInt16LittleEndian(fullHeader.Slice(0x12, 2));
        var data = this.vm.PhysicalMemory.GetSpan(dataSegment, dataOffset, dataLength + 1);

        switch (commandCode)
        {
            case CommandCodes.Read:
                status = IoctlRead(cd, data);
                break;

            case CommandCodes.Write:
                status = IoctlWrite(cd, data);
                break;

            case CommandCodes.Seek:
                status = Status_Done;
                break;

            case CommandCodes.PlayAudio:
                {
                    int startFrame = BinaryPrimitives.ReadInt32LittleEndian(fullHeader[14..]);
                    int playLength = BinaryPrimitives.ReadInt32LittleEndian(fullHeader[18..]);
                    cd.PlaybackSector = startFrame;
                    cd.Play(playLength);
                }
                break;

            case CommandCodes.StopAudio:
                cd.Stop();
                status = Status_Done;
                break;

            case CommandCodes.ReadLongPrefetch:
                System.Diagnostics.Debug.WriteLine("MSCDEX: ReadLongPrefetch");
                break;

            default:
                throw new NotImplementedException();
        }

        //status = 1 << 8;
        //this.vm.PhysicalMemory.SetUInt16(reqHeaderAddress.Segment, (ushort)(reqHeaderAddress.Offset + 3u), 1 << 8);
    }
    private static ushort IoctlRead(IAudioCD cd, Span<byte> data)
    {
        switch (data[0])
        {
            case 1:
                return Status_Done;

            case 8:
                BinaryPrimitives.WriteInt32LittleEndian(data[1..], cd.TotalSectors);
                return Status_Done;

            case 10:
                {
                    var leadOut = new CDTimeSpan(cd.TotalSectors + LeadInSectors);
                    data[1] = 1;
                    data[2] = (byte)cd.Tracks.Count;
                    data[3] = (byte)leadOut.Frames;
                    data[4] = (byte)leadOut.Seconds;
                    data[5] = (byte)leadOut.Minutes;

                    //BinaryPrimitives.WriteInt32LittleEndian(data[3..], cd.TotalSectors);
                }
                return Status_Done;

            case 11:
                {
                    var offset = cd.Tracks[data[1] - 1].Offset + new CDTimeSpan(LeadInSectors);
                    data[2] = (byte)offset.Frames;
                    data[3] = (byte)offset.Seconds;
                    data[4] = (byte)offset.Minutes;
                    data[5] = 0;
                    data[6] = 0;
                }
                return Status_Done;

            case 12:
                {
                    var pos = new CDTimeSpan(cd.PlaybackSector);
                    int trackNumber = 0;
                    while (trackNumber < cd.Tracks.Count - 1)
                    {
                        if (cd.Tracks[trackNumber].Offset > pos)
                        {
                            trackNumber--;
                            break;
                        }

                        trackNumber++;
                    }

                    var track = cd.Tracks[trackNumber];

                    int indexIndex = 0;
                    while (indexIndex < track.Indexes.Count - 1)
                    {
                        if (track.Indexes[indexIndex].Position > pos)
                        {
                            indexIndex--;
                            break;
                        }

                        indexIndex++;
                    }

                    var leadIn = new CDTimeSpan(LeadInSectors);

                    var trackPos = pos - track.Offset; //+ 0//leadIn;
                    pos += leadIn;

                    data[1] = 0; // control/adr
                    data[2] = ToBCD(trackNumber + 1);
                    data[3] = (byte)track.Indexes[indexIndex].Number;
                    data[4] = (byte)trackPos.Minutes;
                    data[5] = (byte)trackPos.Seconds;
                    data[6] = (byte)trackPos.Frames;
                    data[7] = 0;
                    data[8] = (byte)pos.Minutes;
                    data[9] = (byte)pos.Seconds;
                    data[10] = (byte)pos.Frames;
                }
                return Status_Done;

            default:
                throw new NotImplementedException();
        }
    }
    private static ushort IoctlWrite(IAudioCD cd, Span<byte> data)
    {
        switch (data[0])
        {
            case 2:
                // reset drive
                return Status_Done;

            case 3:
                System.Diagnostics.Debug.WriteLine("CD volume control");
                return Status_Done;

            default:
                throw new NotImplementedException();
        }
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
        if (this.GetCDRomDrives().Any())
            vm.PhysicalMemory.SetByte(this.deviceHeader!.Segment, 20, (byte)(GetCDRomDrives().First().Key - 'A' + 1));

        vm.PhysicalMemory.SetByte(this.deviceHeader!.Segment, 21, (byte)GetCDRomDrives().Count());

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

        string identifier;
        ref var entry = ref vm.PhysicalMemory.GetRef<DirectoryEntry>(vm.Processor.SI, vm.Processor.DI);
        if (fileInfo.Result is IIso9660DirectoryEntry isoEntry)
        {
            identifier = isoEntry.Identifier;
            entry.XAR_len = isoEntry.ExtendedAttributeRecordLength;
            entry.loc_extentI = isoEntry.LBALocation;
            entry.data_lenI = isoEntry.DataLength;
            if (isoEntry.RecordingDate != null)
            {
                var recordingDate = (DateTimeOffset)isoEntry.RecordingDate;
                entry.record_time[0] = (byte)(recordingDate.Year - 1900);
                entry.record_time[1] = (byte)recordingDate.Month;
                entry.record_time[2] = (byte)recordingDate.Day;
                entry.record_time[3] = (byte)recordingDate.Hour;
                entry.record_time[4] = (byte)recordingDate.Minute;
                entry.record_time[5] = (byte)recordingDate.Second;
                entry.record_time[6] = (byte)(sbyte)(recordingDate.Offset.TotalMinutes / 15);
            }
            else
            {
                for (int i = 0; i < 7; i++)
                    entry.record_time[i] = 0;
            }

            entry.file_flags_iso = isoEntry.FileFlags;
            entry.il_size = isoEntry.InterleavedUnitSize;
            entry.il_skip = isoEntry.InterleavedGapSize;
            entry.VSSNI = isoEntry.VolumeSequenceNumber;
        }
        else
        {
            identifier = fileInfo.Result.Name;
            if ((fileInfo.Result.Attributes & VirtualFileAttributes.Directory) == 0)
                identifier += ";1";
            entry.XAR_len = 0;
            entry.loc_extentI = 0;
            entry.data_lenI = fileInfo.Result.DosLength;
            entry.record_time[0] = (byte)(fileInfo.Result.ModifyDate.Year - 1900);
            entry.record_time[1] = (byte)fileInfo.Result.ModifyDate.Month;
            entry.record_time[2] = (byte)fileInfo.Result.ModifyDate.Day;
            entry.record_time[3] = (byte)fileInfo.Result.ModifyDate.Hour;
            entry.record_time[4] = (byte)fileInfo.Result.ModifyDate.Minute;
            entry.record_time[5] = (byte)fileInfo.Result.ModifyDate.Second;
            entry.record_time[6] = (byte)(sbyte)(TimeZoneInfo.Local.GetUtcOffset(fileInfo.Result.ModifyDate).TotalMinutes / 15);
            entry.file_flags_iso = 0;
            if ((fileInfo.Result.Attributes & VirtualFileAttributes.Hidden) != 0)
                entry.file_flags_iso |= 1;
            if ((fileInfo.Result.Attributes & VirtualFileAttributes.Directory) != 0)
                entry.file_flags_iso |= 2;
            entry.il_size = 0;
            entry.il_skip = 0;
            entry.VSSNI = 0;
            entry.VSSNM = 0;
        }

        entry.loc_extentM = (uint)System.Net.IPAddress.HostToNetworkOrder((int)entry.loc_extentI);
        entry.data_lenM = (uint)System.Net.IPAddress.HostToNetworkOrder((int)entry.data_lenI);
        entry.VSSNM = (ushort)System.Net.IPAddress.HostToNetworkOrder((short)entry.VSSNI);
        entry.len_fi = (byte)identifier.Length;
        vm.PhysicalMemory.SetString(vm.Processor.SI, (uint)(vm.Processor.DI + Unsafe.SizeOf<DirectoryEntry>()), identifier, (identifier.Length % 2) == 0);

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

        if (this.vm.FileSystem.Drives[driveIndex].Mapping is not IRawSectorReader drive)
        {
            System.Diagnostics.Debug.WriteLine("Tried to read raw sectors from a device that does not implement IRawSectorReader");
            this.vm.Processor.AX = 21;
            this.vm.Processor.Flags.Carry = true;
            return;
        }

        int sectorsToRead = (ushort)this.vm.Processor.DX;
        int startingSector = (this.vm.Processor.SI << 16) | this.vm.Processor.DI;
        var buffer = new byte[sectorsToRead * drive.SectorSize];
        drive.ReadSectors(startingSector, sectorsToRead, buffer);
        Thread.Sleep(5);

        var target = this.vm.PhysicalMemory.GetSpan(this.vm.Processor.ES, (ushort)this.vm.Processor.BX, buffer.Length);
        buffer.AsSpan().CopyTo(target);
    }
    private void SaveFlags(EFlags modified)
    {
        var oldFlags = (EFlags)vm.PhysicalMemory.GetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4));
        oldFlags &= ~modified;
        vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)(oldFlags | (vm.Processor.Flags.Value & modified)));
    }

    private static byte ToBCD(int n) => (byte)(((n / 10) << 4) | (n % 10));
}
