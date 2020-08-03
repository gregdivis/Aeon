using System;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator.Instructions
{
    internal static class Int
    {
        [Opcode("CC", Name = "int 3h")]
        public static void RaiseInterrupt3(VirtualMachine vm)
        {
            vm.RaiseInterrupt(3);
        }
        [Opcode("CD ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void RaiseInterrupt(VirtualMachine vm, byte interrupt)
        {
#if DEBUG
            if ((vm.Processor.CR0 & CR0.ProtectedModeEnable) == 0 && interrupt != 0x1c && vm.PhysicalMemory.GetRealModeInterruptAddress(interrupt) == PhysicalMemory.NullInterruptHandler)
                System.Diagnostics.Debug.WriteLine(string.Format("Unhandled interrupt {0:X2}h", interrupt));
#endif

            vm.RaiseInterrupt(interrupt);
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
            uint flags = (uint)vm.Processor.Flags.Value & 0xFFFF0000u;

            if ((vm.Processor.CR0 & CR0.ProtectedModeEnable) != 0)
            {
                if ((vm.Processor.Flags.Value & EFlags.NestedTask) != 0)
                {
                    vm.TaskReturn();
                    vm.Processor.InstructionEpilog();
                    return;
                }

                eip = vm.PopFromStack();
                cs = vm.PopFromStack();
                flags |= vm.PopFromStack();

                uint cpl = vm.Processor.CS & 3u;
                uint rpl = cs & 3u;

                if (cpl != rpl)
                {
                    if (cpl > rpl)
                        throw new InvalidOperationException();

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
                flags |= vm.PopFromStack();
            }

            bool throwTrap = false;

            if (((EFlags)flags & EFlags.Trap) != 0 && (vm.Processor.Flags.Value & EFlags.Trap) == 0)
                throwTrap = true;

            vm.Processor.Flags.Value = (EFlags)flags | EFlags.Reserved1;
            vm.WriteSegmentRegister(SegmentIndex.CS, cs);
            vm.Processor.EIP = eip;

            vm.Processor.InstructionEpilog();

            if (throwTrap)
                throw new EnableInstructionTrapException();
        }
        [Alternate("InterruptReturn", AddressSize = 16 | 32)]
        public static void InterruptReturn32(VirtualMachine vm)
        {
            ushort cs;
            uint eip;
            uint flags;

            if ((vm.Processor.CR0 & CR0.ProtectedModeEnable) != 0)
            {
                if ((vm.Processor.Flags.Value & EFlags.NestedTask) != 0)
                {
                    vm.TaskReturn();
                    vm.Processor.InstructionEpilog();
                    return;
                }

                eip = vm.PopFromStack32();
                cs = (ushort)vm.PopFromStack32();
                flags = vm.PopFromStack32();

                uint cpl = vm.Processor.CS & 3u;
                uint rpl = cs & 3u;

                if (cpl != rpl)
                {
                    if (cpl > rpl)
                        throw new InvalidOperationException();

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
                flags = vm.PopFromStack32();
            }

            bool throwTrap = false;

            if (((EFlags)flags & EFlags.Trap) != 0 && (vm.Processor.Flags.Value & EFlags.Trap) == 0)
                throwTrap = true;

            vm.Processor.Flags.Value = (EFlags)flags | EFlags.Reserved1;
            vm.WriteSegmentRegister(SegmentIndex.CS, cs);
            vm.Processor.EIP = eip;

            vm.Processor.InstructionEpilog();

            if (throwTrap)
                throw new EnableInstructionTrapException();
        }
    }

    internal static class Host
    {
        [Opcode("0F55 ib", Name = "inth")]
        public static void RaiseInterrupt(VirtualMachine vm, byte interrupt)
        {
            vm.CallInterruptHandler(interrupt);
        }

        [Opcode("0F56 ib", Name = "callh")]
        public static void InvokeCallback(VirtualMachine vm, byte id)
        {
            vm.CallCallback(id);
        }

        [Opcode("0F57 ib", Name = "cmd")]
        public static void RunCommandInterpreter(VirtualMachine vm, byte mode)
        {
            if (mode == 0)
            {
                var args = vm.CurrentProcess.CommandLineArguments;
                int index = args.IndexOf("/c", StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    args = args.Substring(index + 2).Trim();

                    ParseCommand(vm, args);
                    vm.Processor.Flags.Carry = true;
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
                vm.Console.Write(vm.FileSystem.WorkingDirectory.ToString() + ">");
            }
            else
            {
                int stringLength = vm.PhysicalMemory.GetByte(vm.Processor.DS, (ushort)vm.Processor.DX + 1u);
                var input = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX + 2u, stringLength);

                vm.Console.WriteLine();

                vm.Processor.Flags.Carry = ParseCommand(vm, input);
            }
        }

        private static bool ParseCommand(VirtualMachine vm, string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var command = CommandInterpreter.Parser.Parse(input);
                if (command == null || !command.IsParsed)
                {
                    vm.Console.WriteLine("Bad command or file name.");
                }
                else
                {
                    var result = command.Run(vm);
                    if (result == CommandInterpreter.CommandResult.Exit)
                        return true;
                }
            }

            return false;
        }
    }
}
