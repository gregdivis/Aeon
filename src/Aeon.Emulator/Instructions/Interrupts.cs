using System;
using System.IO;
using Aeon.Emulator.CommandInterpreter;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator.Instructions
{
    internal static class Int
    {
        [Opcode("CC", Name = "int 3h")]
        public static void RaiseInterrupt3(VirtualMachine vm) => vm.RaiseInterrupt(3);
        [Opcode("CD ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void RaiseInterrupt(VirtualMachine vm, byte interrupt)
        {
#if DEBUG
            if (((vm.Processor.CR0 & CR0.ProtectedModeEnable) == 0 || vm.Processor.Flags.Virtual8086Mode) && interrupt != 0x1c && vm.PhysicalMemory.GetRealModeInterruptAddress(interrupt) == PhysicalMemory.NullInterruptHandler)
                System.Diagnostics.Debug.WriteLine($"Unhandled interrupt {interrupt:X2}h");
#endif

            vm.RaiseInterrupt(interrupt, true);
        }
        [Opcode("CE", Name = "into")]
        public static void RaiseInterrupt4(VirtualMachine vm)
        {
            if (vm.Processor.Flags.Overflow)
                vm.RaiseInterrupt(4);
        }

        [Opcode("CF", Name = "iret", AddressSize = 16 | 32)]
        public static void InterruptReturn(VirtualMachine vm)
        {
            ushort cs;
            uint eip;
            var flags = (EFlags)((uint)vm.Processor.Flags.Value & 0xFFFF0000u);

            if (vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable) & !vm.Processor.Flags.Virtual8086Mode)
            {
                if (vm.Processor.Flags.NestedTask)
                {
                    vm.TaskReturn();
                    vm.Processor.InstructionEpilog();
                    return;
                }

                eip = vm.PopFromStack();
                cs = vm.PopFromStack();
                flags |= (EFlags)vm.PopFromStack();

                uint cpl = vm.Processor.CPL;
                uint rpl = cs & 3u;

                if (cpl != rpl)
                {
                    if (cpl > rpl)
                        ThrowHelper.ThrowCplGreaterThanRplException();

                    uint esp = vm.PopFromStack();
                    ushort ss = vm.PopFromStack();
                    vm.WriteSegmentRegister(SegmentIndex.SS, ss);
                    vm.Processor.ESP = esp;
                }
            }
            else
            {
                eip = vm.PopFromStack();
                cs = vm.PopFromStack();
                flags |= (EFlags)vm.PopFromStack();
            }

            bool throwTrap = false;

            if (flags.HasFlag(EFlags.Trap) && !vm.Processor.Flags.Trap)
                throwTrap = true;

            vm.Processor.Flags.SetWithMask(flags, ~(EFlags.Trap | EFlags.Reserved1 | EFlags.Virtual8086Mode));
            
            vm.WriteSegmentRegister(SegmentIndex.CS, cs);
            vm.Processor.EIP = eip;

            vm.Processor.InstructionEpilog();

            if (throwTrap)
                ThrowHelper.ThrowEnableInstuctionTrapException();
        }
        [Alternate(nameof(InterruptReturn), AddressSize = 16 | 32)]
        public static void InterruptReturn32(VirtualMachine vm)
        {
            ushort cs;
            uint eip;
            EFlags flags;

            if (vm.Processor.CR0.HasFlag(CR0.ProtectedModeEnable) & !vm.Processor.Flags.Virtual8086Mode)
            {
                if (vm.Processor.Flags.NestedTask)
                {
                    vm.TaskReturn();
                    vm.Processor.InstructionEpilog();
                    return;
                }

                eip = vm.PopFromStack32();
                cs = (ushort)vm.PopFromStack32();
                flags = (EFlags)vm.PopFromStack32();

                if (flags.HasFlag(EFlags.Virtual8086Mode))
                {
                    vm.Processor.EIP = eip & 0xffff;
                    uint n_esp = vm.PopFromStack32();
                    ushort n_ss = (ushort)vm.PopFromStack32();
                    ushort n_es = (ushort)vm.PopFromStack32();
                    ushort n_ds = (ushort)vm.PopFromStack32();
                    ushort n_fs = (ushort)vm.PopFromStack32();
                    ushort n_gs = (ushort)vm.PopFromStack32();

                    vm.Processor.Flags.SetWithMask(flags, ~(EFlags.Trap | EFlags.Reserved1));
                    vm.Processor.Flags.Virtual8086Mode = true;
                    vm.Processor.CPL = 3;
                    vm.WriteSegmentRegister(SegmentIndex.SS, n_ss);
                    vm.WriteSegmentRegister(SegmentIndex.ES, n_es);
                    vm.WriteSegmentRegister(SegmentIndex.DS, n_ds);
                    vm.WriteSegmentRegister(SegmentIndex.FS, n_fs);
                    vm.WriteSegmentRegister(SegmentIndex.GS, n_gs);

                    vm.Processor.ESP = n_esp;

                    vm.WriteSegmentRegister(SegmentIndex.CS, cs);
                    vm.Processor.InstructionEpilog();
                    return;
                }

                uint cpl = vm.Processor.CPL;
                uint rpl = cs & 3u;

                if (cpl != rpl)
                {
                    if (cpl > rpl)
                        ThrowHelper.ThrowCplGreaterThanRplException();

                    uint esp = vm.PopFromStack32();
                    ushort ss = (ushort)vm.PopFromStack32();
                    vm.WriteSegmentRegister(SegmentIndex.SS, ss);
                    vm.Processor.ESP = esp;
                }
            }
            else
            {
                eip = vm.PopFromStack32();
                cs = (ushort)vm.PopFromStack32();
                flags = (EFlags)vm.PopFromStack32();
            }

            bool throwTrap = false;

            if (flags.HasFlag(EFlags.Trap) && !vm.Processor.Flags.Trap)
                throwTrap = true;

            vm.Processor.Flags.SetWithMask(flags, ~(EFlags.Trap | EFlags.Reserved1));
            vm.WriteSegmentRegister(SegmentIndex.CS, cs);
            vm.Processor.EIP = eip;

            vm.Processor.InstructionEpilog();

            if (throwTrap)
                ThrowHelper.ThrowEnableInstuctionTrapException();
        }
    }

    internal static class Host
    {
        [Opcode("0F55 ib", Name = "inth")]
        public static void RaiseInterrupt(VirtualMachine vm, byte interrupt) => vm.CallInterruptHandler(interrupt);

        [Opcode("0F56 ib", Name = "callh")]
        public static void InvokeCallback(VirtualMachine vm, byte id) => vm.CallCallback(id);

        [Opcode("0F57 ib", Name = "cmd")]
        public static void RunCommandInterpreter(VirtualMachine vm, byte mode)
        {
            var commandProcessor = vm.Dos.GetCommandProcessor();
            if (commandProcessor.HasBatch)
            {
                var result = commandProcessor.RunNextBatchStatement();
                if (result == CommandResult.Exit)
                    vm.Processor.Flags.Carry = true;
                else if(result == CommandResult.Continue && commandProcessor.HasBatch)
                    vm.Processor.EIP -= 3;

                return;
            }

            if (mode == 0)
            {
                // mode 0 = process args if any specified
                var args = vm.CurrentProcess.CommandLineArguments;
                if (args.Contains("/c", StringComparison.OrdinalIgnoreCase))
                {
                    var actualArgs = args.AsSpan()[2..].Trim();
                    if (StatementParser.IsBatchFile(actualArgs))
                    {
                        StatementParser.Split(actualArgs, out var batchFileName, out var batchArgs);
                        vm.Processor.Flags.Carry = !commandProcessor.BeginBatch(batchFileName, batchArgs.TrimStart());
                    }
                    else
                    {
                        ParseCommand(vm, actualArgs);
                        vm.Processor.Flags.Carry = true;
                    }
                }
                else
                {
                    args = args.Trim();
                    if (!string.IsNullOrEmpty(args))
                    {
                        ParseCommand(vm, args);
                        vm.Processor.Flags.Carry = true;
                    }
                    else
                    {
                        vm.Processor.Flags.Carry = false;
                    }
                }
            }
            else if (mode == 1)
            {
                // mode 1 = write command prompt
                vm.Console.Write(vm.FileSystem.WorkingDirectory.ToString() + ">");
            }
            else
            {
                // mode 2 = process input from mode 1
                int stringLength = vm.PhysicalMemory.GetByte(vm.Processor.DS, (ushort)vm.Processor.DX + 1u);
                var input = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX + 2u, stringLength);

                vm.Console.WriteLine();

                vm.Processor.Flags.Carry = ParseCommand(vm, input);
            }
        }

        private static bool ParseCommand(VirtualMachine vm, ReadOnlySpan<char> input)
        {
            if (!input.IsEmpty && !input.IsWhiteSpace())
            {
                var command = vm.Dos.GetCommandProcessor();
                var result = command.Run(input);
                if (result == CommandResult.Exit)
                    return true;
            }

            return false;
        }
    }
}
