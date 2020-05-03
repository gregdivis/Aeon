using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Aeon.Emulator.Decoding.Emitters;
using Aeon.Emulator.Instructions;

namespace Aeon.Emulator.Decoding
{
    internal sealed class EmulatorBuilder
    {
        private const int VmArgument = 0;

        private ILGenerator il;
        private LocalBuilder paramIsAddress;
        private bool? requiresTemp;
        private Type tempType;
        private int wordSize = 2;
        private Type wordType = typeof(ushort);
        private bool addressSize32;
        private LocalBuilder processorLocal;
        private readonly SortedList<OperandType, Func<EmitStateInfo, Emitter>> newEmitters;

        public EmulatorBuilder()
        {
            this.newEmitters = InitializeLoaders();
        }

        public DecodeAndEmulate[] GetDelegates(InstructionInfo info)
        {
            var delegates = new DecodeAndEmulate[4];

            if (info.Operands.Count == 0)
            {
                // If there are no operands, just create delegates for the emulate methods directly.
                for (int i = 0; i < 4; i++)
                {
                    // These could be null if they aren't implemented yet.
                    if (info.EmulateMethods[i] != null)
                    {
                        var args = info.EmulateMethods[i].GetParameters();
                        if (args[0].ParameterType == typeof(VirtualMachine))
                            delegates[i] = (DecodeAndEmulate)Delegate.CreateDelegate(typeof(DecodeAndEmulate), info.EmulateMethods[i]);
                        else
                            delegates[i] = BuildWrapperMethod(info.EmulateMethods[i], args[0].ParameterType);
                    }
                }
            }
            else
            {
                // The instruction has operands, so we need to generate some IL.
                delegates[0] = BuildMethod(info, false, false);

                if (info.EmulateMethods[1] != null)
                {
                    // Prevent generating the same method multiple times.
                    if (info.EmulateMethods[0] == info.EmulateMethods[1] && !info.IsAffectedByOperandSize)
                        delegates[1] = delegates[0];
                    else
                        delegates[1] = BuildMethod(info, true, false);
                }

                if (info.EmulateMethods[2] != null)
                {
                    // Prevent generating the same method multiple times.
                    if (info.EmulateMethods[0] == info.EmulateMethods[2] && !info.IsAffectedByAddressSize)
                        delegates[2] = delegates[0];
                    else
                        delegates[2] = BuildMethod(info, false, true);
                }

                if (info.EmulateMethods[3] != null)
                {
                    // Prevent generating the same method multiple times.
                    if (info.EmulateMethods[0] == info.EmulateMethods[3] && !info.IsAffectedByOperandSize && !info.IsAffectedByAddressSize)
                        delegates[3] = delegates[0];
                    else if (info.EmulateMethods[1] == info.EmulateMethods[3] && !info.IsAffectedByAddressSize)
                        delegates[3] = delegates[1];
                    else if (info.EmulateMethods[2] == info.EmulateMethods[3] && !info.IsAffectedByOperandSize)
                        delegates[3] = delegates[2];
                    else
                        delegates[3] = BuildMethod(info, true, true);
                }
            }

            return delegates;
        }

        private static SortedList<OperandType, Func<EmitStateInfo, Emitter>> InitializeLoaders()
        {
            var newEmitters = new SortedList<OperandType, Func<EmitStateInfo, Emitter>>();

            newEmitters.Add(OperandType.ImmediateByte, s => new LoadImmediate(s, 1, ValueExtend.Zero));
            newEmitters.Add(OperandType.ImmediateByteExtend, s => new LoadImmediate(s, 1, ValueExtend.Sign));
            newEmitters.Add(OperandType.ImmediateRelativeByte, s => new LoadImmediate(s, 1, ValueExtend.Sign));
            newEmitters.Add(OperandType.ImmediateWord, s => new LoadImmediate(s, s.WordSize, ValueExtend.Zero));
            newEmitters.Add(OperandType.ImmediateRelativeWord, s => new LoadImmediate(s, s.WordSize, ValueExtend.Zero));
            newEmitters.Add(OperandType.ImmediateInt16, s => new LoadImmediate(s, 2, ValueExtend.Zero));
            newEmitters.Add(OperandType.RegisterByte, s => new LoadRegister(s, 1));
            newEmitters.Add(OperandType.RegisterWord, s => new LoadRegister(s, s.WordSize));
            newEmitters.Add(OperandType.SegmentRegister, s => new LoadSegmentRegister(s));
            newEmitters.Add(OperandType.MemoryOffsetByte, s => new LoadMoffs(s, 1));
            newEmitters.Add(OperandType.MemoryOffsetWord, s => new LoadMoffs(s, s.WordSize));
            newEmitters.Add(OperandType.RegisterOrMemoryByte, s => LoadRegRmw.Create(s, 1, false));
            newEmitters.Add(OperandType.RegisterOrMemoryWord, s => LoadRegRmw.Create(s, s.WordSize, false));
            newEmitters.Add(OperandType.RegisterOrMemoryWordNearPointer, s => LoadRegRmw.Create(s, s.WordSize, false));
            newEmitters.Add(OperandType.MemoryInt16, s => LoadRegRmw.Create(s, 2, true));
            newEmitters.Add(OperandType.RegisterOrMemory16, s => LoadRegRmw.Create(s, 2, false));
            newEmitters.Add(OperandType.MemoryInt32, s => LoadRegRmw.Create(s, 4, true));
            newEmitters.Add(OperandType.RegisterOrMemory32, s => LoadRegRmw.Create(s, 4, false));
            newEmitters.Add(OperandType.MemoryInt64, s => LoadRegRmw.Create(s, 8, true));
            newEmitters.Add(OperandType.ImmediateFarPointer, s => new LoadImmediate(s, s.WordSize + 2, ValueExtend.Zero));
            newEmitters.Add(OperandType.IndirectFarPointer, s => LoadRegRmw.Create(s, s.WordSize * 2, true));
            newEmitters.Add(OperandType.MemoryFloat32, s => LoadRegRmw.CreateFloat(s, 4));
            newEmitters.Add(OperandType.MemoryFloat64, s => LoadRegRmw.CreateFloat(s, 8));
            newEmitters.Add(OperandType.MemoryFloat80, s => LoadRegRmw.CreateFloat(s, 10));
            newEmitters.Add(OperandType.DebugRegister, s => new LoadDebugRegister(s));
            newEmitters.Add(OperandType.EffectiveAddress, s => LoadRegRmw.CreateEffectiveAddress(s));
            newEmitters.Add(OperandType.FullLinearAddress, s => LoadRegRmw.CreateLinearAddress(s));

            return newEmitters;
        }

        private DecodeAndEmulate BuildWrapperMethod(MethodInfo method, Type firstArgType)
        {
            var dynamicMethod = new DynamicMethod(string.Empty, typeof(void), new Type[] { typeof(VirtualMachine) }, typeof(EmulatorBuilder));
            var il = dynamicMethod.GetILGenerator();
            il.LoadArgument(VmArgument);
            if (firstArgType == typeof(Processor))
                il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.Processor);
            else if (firstArgType == typeof(PhysicalMemory))
                il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.PhysicalMemory);
            else
                throw new InvalidOperationException();

            il.Emit(OpCodes.Tailcall);
            il.Emit(OpCodes.Call, method);
            il.Emit(OpCodes.Ret);

            return (DecodeAndEmulate)dynamicMethod.CreateDelegate(typeof(DecodeAndEmulate));
        }

        private DecodeAndEmulate BuildMethod(InstructionInfo info, bool operandSize32, bool addressSize32)
        {
            MethodInfo emulateMethod;
            if (operandSize32)
            {
                this.wordSize = 4;
                this.wordType = typeof(uint);
                emulateMethod = info.EmulateMethods[1 | (addressSize32 ? 2 : 0)];
            }
            else
            {
                this.wordSize = 2;
                this.wordType = typeof(ushort);
                emulateMethod = info.EmulateMethods[0 | (addressSize32 ? 2 : 0)];
            }

            this.addressSize32 = addressSize32;

            var dynamicMethod = new DynamicMethod(string.Empty, typeof(void), new Type[] { typeof(VirtualMachine) }, typeof(EmulatorBuilder));
            this.il = dynamicMethod.GetILGenerator();

            var parameters = info.EmulateMethods[0].GetParameters();
            bool isByRef = parameters[1].ParameterType.IsByRef;
            bool isOut = parameters[1].IsOut;
            bool isParam2ByRef = false;

            if (parameters.Length >= 3 && parameters[2].ParameterType.IsByRef)
                isParam2ByRef = true;

            requiresTemp = false;
            tempType = null;

            var methodArgs = new LocalBuilder[3];

            this.processorLocal = il.DeclareLocal(typeof(Processor));
            InitializeProcessor();

            var emitters = CreateOperandEmitters(info, isByRef, isParam2ByRef, methodArgs);

            AdvanceEIP();

            InvokeEmulatorMethod(info, isOut, methodArgs, emulateMethod);

            foreach (var emitter in emitters)
                emitter.EmitEpilog();

            RestoreState(info);

            il.Emit(OpCodes.Ret);

            return (DecodeAndEmulate)dynamicMethod.CreateDelegate(typeof(DecodeAndEmulate));
        }
        private void InitializeProcessor()
        {
            il.LoadArgument(VmArgument);
            il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.Processor);
            il.StoreLocal(this.processorLocal);
        }
        private List<Emitter> CreateOperandEmitters(InstructionInfo info, bool isParam1ByRef, bool isParam2ByRef, LocalBuilder[] methodArgs)
        {
            var emitters = new List<Emitter>(3);

            foreach (int index in info.Operands.SortedIndices)
            {
                bool returnValue = !(index == 0 && isParam1ByRef) && !(index == 1 && isParam2ByRef);
                var state = new EmitStateInfo(il, this.processorLocal, wordSize, returnValue ? EmitReturnType.Value : EmitReturnType.Address, addressSize32 ? 32 : 16);

                Func<EmitStateInfo, Emitter> newEmitter;
                var operand = info.Operands[index];
                if (!this.newEmitters.TryGetValue(operand, out newEmitter))
                {
                    if (!LoadKnownRegister.IsKnownRegister(operand))
                        throw new InvalidOperationException();

                    newEmitter = s => new LoadKnownRegister(s, operand);
                }

                var emitter = newEmitter(state);
                emitter.EmitLoad();

                if (emitter.RequiresTemp != false)
                {
                    this.requiresTemp = emitter.RequiresTemp;
                    this.tempType = emitter.TempType;
                    this.paramIsAddress = emitter.ParamIsAddressLocal;
                }

                methodArgs[index] = il.DeclareLocal(emitter.MethodArgType);
                il.StoreLocal(methodArgs[index]);

                emitters.Add(emitter);
            }

            return emitters;
        }
        private void AdvanceEIP()
        {
            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.PIP);

            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.CachedInstruction);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Conv_U4);
            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.StartEIP);
            il.Emit(OpCodes.Add);

            il.Emit(OpCodes.Stind_I4);
        }
        private void InvokeEmulatorMethod(InstructionInfo info, bool isOut, LocalBuilder[] methodArgs, MethodInfo method)
        {
            var doneLabel = il.DefineLabel();

            var firstArgType = method.GetParameters()[0].ParameterType;
            if (firstArgType == typeof(VirtualMachine))
                il.LoadArgument(VmArgument);
            else if (firstArgType == typeof(Processor))
                LoadProcessor();
            else if (firstArgType == typeof(PhysicalMemory))
                LoadPhysicalMemory();

            LocalBuilder tempValue;

            // Code is different if temp variables are required.
            if (requiresTemp != false)
            {
                tempValue = il.DeclareLocal(tempType);
                Label skipTempLoadLabel = new Label();
                if (requiresTemp == null)
                {
                    skipTempLoadLabel = il.DefineLabel();

                    il.LoadLocal(paramIsAddress);
                    il.Emit(OpCodes.Brfalse_S, skipTempLoadLabel);
                }

                // Don't need to get initial value if this is only an out parameter.
                if (!isOut)
                {
                    LoadPhysicalMemory();
                    il.LoadLocal(methodArgs[0]);
                    il.Emit(OpCodes.Conv_U4);
                    CallGetMemoryValue(tempType);
                    il.StoreLocal(tempValue);
                }

                // Load the address of the temp variable onto the stack.
                il.Emit(OpCodes.Ldloca_S, (byte)tempValue.LocalIndex);

                // Load the rest of the parameters onto the stack.
                for (int i = 1; i < info.Operands.Count; i++)
                    il.LoadLocal(methodArgs[i]);

                // Call the emulate method.
                il.Emit(OpCodes.Call, method);

                // Write the new value to memory.
                if (methodArgs[0].LocalType != typeof(SegmentIndex))
                {
                    LoadPhysicalMemory();
                    il.LoadLocal(methodArgs[0]);
                    il.Emit(OpCodes.Conv_U4);
                    il.LoadLocal(tempValue);
                    CallSetMemoryValue(tempType);
                }
                else
                {
                    il.LoadArgument(VmArgument);
                    il.LoadLocal(methodArgs[0]);
                    il.Emit(OpCodes.Conv_U4);
                    il.LoadLocal(tempValue);
                    il.Emit(OpCodes.Call, Infos.VirtualMachine.WriteSegmentRegister);
                }

                il.Emit(OpCodes.Br_S, doneLabel);

                // If requiresTemp is unknown, add the code for the register case.
                if (requiresTemp == null)
                {
                    il.MarkLabel(skipTempLoadLabel);
                    for (int index = 0; index < info.Operands.Count; index++)
                        il.LoadLocal(methodArgs[index]);

                    il.Emit(OpCodes.Call, method);
                }
            }
            else
            {
                // All of the variables are either in or direct pointers, so
                // no temp variables are needed.
                for (int index = 0; index < info.Operands.Count; index++)
                    il.LoadLocal(methodArgs[index]);

                il.Emit(OpCodes.Call, method);
            }

            il.MarkLabel(doneLabel);
        }
        private void RestoreState(InstructionInfo info)
        {
            if (!info.IsPrefix)
            {
                LoadProcessor();
                il.Emit(OpCodes.Call, Infos.Processor.InstructionEpilog);
            }
        }

        private void LoadProcessor()
        {
            il.LoadLocal(this.processorLocal);
        }
        private void LoadPhysicalMemory()
        {
            il.LoadArgument(VmArgument);
            il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.PhysicalMemory);
        }
        private void CallGetMemoryValue(Type type)
        {
            if (type == typeof(byte) || type == typeof(sbyte))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetByte);
            else if (type == typeof(short) || type == typeof(ushort))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetUInt16);
            else if (type == typeof(int) || type == typeof(uint))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetUInt32);
            else if (type == typeof(long) || type == typeof(ulong))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetUInt64);
            else if (type == typeof(float))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetReal32);
            else if (type == typeof(double))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetReal64);
            else if (type == typeof(Real10))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetReal80);
            else
                throw new ArgumentException();
        }
        private void CallSetMemoryValue(Type type)
        {
            if (type == typeof(byte) || type == typeof(sbyte))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetByte);
            else if (type == typeof(short) || type == typeof(ushort))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetUInt16);
            else if (type == typeof(int) || type == typeof(uint))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetUInt32);
            else if (type == typeof(long) || type == typeof(ulong))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetUInt64);
            else if (type == typeof(float))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetReal32);
            else if (type == typeof(double))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetReal64);
            else if (type == typeof(Real10))
                il.Emit(OpCodes.Call, Infos.PhysicalMemory.SetReal80);
            else
                throw new ArgumentException();
        }
    }

    internal delegate void DecodeAndEmulate(VirtualMachine vm);
}
