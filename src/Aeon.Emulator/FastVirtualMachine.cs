using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Aeon.Emulator.Decoding;

[assembly: InternalsVisibleTo("Aeon.Emulator.Generated")]

namespace Aeon.Emulator
{
    /// <summary>
    /// <see cref="VirtualMachine"/> with additional performance enhancements.
    /// </summary>
    public abstract class FastVirtualMachine : VirtualMachine
    {
        private static Type fastVmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastVirtualMachine"/> class.
        /// </summary>
        protected FastVirtualMachine(int physicalMemorySize)
            : base(physicalMemorySize)
        {
        }

        public static VirtualMachine Create() => Create(16);
        public static VirtualMachine Create(int physicalMemorySize)
        {
            if (fastVmType == null)
                fastVmType = BuildType();

            return (FastVirtualMachine)Activator.CreateInstance(fastVmType, physicalMemorySize);
        }

        private static unsafe Type BuildType()
        {
            InstructionSet.InitializeNativeArrays();

            var methodImplAttribute = new CustomAttributeBuilder(typeof(MethodImplAttribute).GetConstructor(new[] { typeof(MethodImplOptions) }), new object[] { MethodImplOptions.AggressiveOptimization });

            var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Aeon.Emulator.Generated"), AssemblyBuilderAccess.Run);
            var module = asm.DefineDynamicModule("Aeon.Emulator.Generated.dll");
            var fastVm = module.DefineType("FastVirtualMachineImpl", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(FastVirtualMachine));

            var constructor = fastVm.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(int) });
            var il = constructor.GetILGenerator();
            il.LoadThis();
            il.LoadArgument(1);
            il.Emit(OpCodes.Call, typeof(FastVirtualMachine).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int) }, null));
            il.Emit(OpCodes.Ret);

            var emulateMethod = fastVm.DefineMethod("Emulate", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new[] { typeof(int) });
            emulateMethod.SetCustomAttribute(methodImplAttribute);
            il = emulateMethod.GetILGenerator();

            var processorLocal = il.DeclareLocal(typeof(Processor));
            var memoryLocal = il.DeclareLocal(typeof(PhysicalMemory));
            var eipLocal = il.DeclareLocal(typeof(void).MakePointerType());
            var startEipLocal = il.DeclareLocal(typeof(uint));
            var ipLocal = il.DeclareLocal(typeof(byte).MakePointerType());
            var byte1Local = il.DeclareLocal(typeof(byte));
            var byte2Local = il.DeclareLocal(typeof(byte));
            var byte3Local = il.DeclareLocal(typeof(byte));
            var instLocal = il.DeclareLocal(typeof(void).MakePointerType());
            var modeLocal = il.DeclareLocal(typeof(int));
            var countLocal = il.DeclareLocal(typeof(int));

            var signature = SignatureHelper.GetMethodSigHelper(CallingConventions.Standard, typeof(void));
            signature.AddArgument(typeof(VirtualMachine));
            signature.GetSignature();

            var doneLabel = il.DefineLabel();

            void emitIncrementEIP(int n)
            {
                il.LoadLocal(eipLocal);
                il.LoadLocal(startEipLocal);
                il.LoadConstant(n);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stind_I4);

                il.LoadLocal(processorLocal);
                il.LoadLocal(ipLocal);
                il.LoadConstant(n);
                il.Emit(OpCodes.Conv_I);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stfld, Infos.Processor.CachedIP);
            }

            void emitLoadPointer(IntPtr p)
            {
                il.LoadPointer(p);
                il.LoadLocal(modeLocal);
                il.LoadConstant(IntPtr.Size);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Conv_I);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Ldind_I);

                il.LoadLocal(byte1Local);
                il.LoadConstant(IntPtr.Size);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Conv_I);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Ldind_I);
            }

            #region count = arg1;
            il.LoadArgument(1);
            il.StoreLocal(countLocal);
            #endregion

            #region processor = vm.Processor;
            il.LoadThis();
            il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.Processor);
            il.StoreLocal(processorLocal);
            #endregion

            #region memory = vm.PhysicalMemory;
            il.LoadThis();
            il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.PhysicalMemory);
            il.StoreLocal(memoryLocal);
            #endregion

            #region eip = (uint*)processor.PIP;
            il.LoadLocal(processorLocal);
            il.Emit(OpCodes.Ldfld, Infos.Processor.PIP);
            il.StoreLocal(eipLocal);
            #endregion

            var startLabel = il.DefineLabel();
            il.MarkLabel(startLabel);

            #region startEIP = *eip;
            il.LoadLocal(eipLocal);
            il.Emit(OpCodes.Ldind_U4);
            il.StoreLocal(startEipLocal);
            #endregion
            #region processor.StartEIP = startEIP;
            il.LoadLocal(processorLocal);
            il.LoadLocal(startEipLocal);
            il.Emit(OpCodes.Stfld, Infos.Processor.StartEIP);
            #endregion
            #region ip = processor.CachedInstruction;
            il.LoadLocal(processorLocal);
            il.Emit(OpCodes.Ldfld, Infos.Processor.CachedInstruction);
            il.StoreLocal(ipLocal);
            #endregion
            #region memory.FetchInstruction(processor.segmentBases[(int)SegmentIndex.CS] + startEIP, ip);
            il.LoadLocal(memoryLocal);

            il.LoadLocal(processorLocal);
            il.Emit(OpCodes.Ldfld, Infos.Processor.SegmentBases);
            il.LoadConstant((int)SegmentIndex.CS * sizeof(uint));
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_U4);
            il.LoadLocal(startEipLocal);
            il.Emit(OpCodes.Add);

            il.LoadLocal(ipLocal);

            il.Emit(OpCodes.Call, Infos.PhysicalMemory.FetchInstruction);
            #endregion

            #region mode = processor.SizeModeIndex;
            il.LoadLocal(processorLocal);
            il.Emit(OpCodes.Call, Infos.Processor.SizeModeIndex.GetGetMethod(true));
            il.StoreLocal(modeLocal);
            #endregion

            #region byte1 = ip[0];
            il.LoadLocal(ipLocal);
            il.Emit(OpCodes.Ldind_U1);
            il.StoreLocal(byte1Local);
            #endregion

            #region inst = OneBytePtrs[mode][byte1];
            emitLoadPointer(new IntPtr(InstructionSet.OneBytePtrs));
            il.StoreLocal(instLocal);
            #endregion

            var twoByteLabel = il.DefineLabel();
            #region if(inst != null) inst(vm);
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse_S, twoByteLabel);
            emitIncrementEIP(1);
            il.LoadThis();
            il.LoadLocal(instLocal);
            il.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(void), new[] { typeof(VirtualMachine) }, null);
            il.Emit(OpCodes.Br, doneLabel);
            #endregion

            var rmLabel = il.DefineLabel();
            il.MarkLabel(twoByteLabel);
            #region byte2 = ip[1];
            il.LoadLocal(ipLocal);
            il.LoadConstant(1);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_U1);
            il.StoreLocal(byte2Local);
            #endregion
            #region inst = TwoBytePtrs[mode][byte1];
            emitLoadPointer(new IntPtr(InstructionSet.TwoBytePtrs));
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst == null) goto rmLabel;
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse_S, rmLabel);
            #endregion
            #region inst = inst[byte2];
            il.LoadLocal(instLocal);
            il.LoadLocal(byte2Local);
            il.LoadConstant(IntPtr.Size);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst != null) inst(vm);
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse_S, rmLabel);
            emitIncrementEIP(2);
            il.LoadThis();
            il.LoadLocal(instLocal);
            il.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(void), new[] { typeof(VirtualMachine) }, null);
            il.Emit(OpCodes.Br, doneLabel);
            #endregion

            var twoByteRmLabel = il.DefineLabel();
            var invalidLabel = il.DefineLabel();
            il.MarkLabel(rmLabel);
            #region inst = RmPtrs[mode][byte1];
            emitLoadPointer(new IntPtr(InstructionSet.RmPtrs));
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst == null) goto twoByteRmLabel;
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse_S, twoByteRmLabel);
            #endregion
            #region inst = inst[(byte2 & 0x38) >> 3];
            il.LoadLocal(instLocal);
            il.LoadLocal(byte2Local);
            if (Bmi1.IsSupported)
            {
                il.LoadConstant(0x0303);
                il.Emit(OpCodes.Call, Infos.Intrinsics.BitFieldExtract);
            }
            else
            {
                il.LoadConstant(0x38);
                il.Emit(OpCodes.And);
                il.LoadConstant(3);
                il.Emit(OpCodes.Shr_Un);
            }
            il.LoadConstant(IntPtr.Size);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst == null) goto invalidLabel;
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse, invalidLabel);
            #endregion
            #region inst(vm);
            emitIncrementEIP(1);
            il.LoadThis();
            il.LoadLocal(instLocal);
            il.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(void), new[] { typeof(VirtualMachine) }, null);
            il.Emit(OpCodes.Br, doneLabel);
            #endregion

            il.MarkLabel(twoByteRmLabel);
            #region byte3 = ip[2];
            il.LoadLocal(ipLocal);
            il.LoadConstant(2);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_U1);
            il.StoreLocal(byte3Local);
            #endregion
            #region inst = TwoByteRmPtrs[mode][byte1];
            emitLoadPointer(new IntPtr(InstructionSet.TwoByteRmPtrs));
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst == null) goto invalidLabel;
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse, invalidLabel);
            #endregion
            #region inst = inst[byte2];
            il.LoadLocal(instLocal);
            il.LoadLocal(byte2Local);
            il.LoadConstant(IntPtr.Size);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst == null) goto invalidLabel;
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse_S, invalidLabel);
            #endregion
            #region inst = inst[(byte3 & 0x38) >> 3];
            il.LoadLocal(instLocal);
            il.LoadLocal(byte3Local);
            if (Bmi1.IsSupported)
            {
                il.LoadConstant(0x0303);
                il.Emit(OpCodes.Call, Infos.Intrinsics.BitFieldExtract);
            }
            else
            {
                il.LoadConstant(0x38);
                il.Emit(OpCodes.And);
                il.LoadConstant(3);
                il.Emit(OpCodes.Shr_Un);
            }
            il.LoadConstant(IntPtr.Size);
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.StoreLocal(instLocal);
            #endregion
            #region if(inst != null) inst(vm);
            il.LoadLocal(instLocal);
            il.Emit(OpCodes.Brfalse_S, invalidLabel);
            emitIncrementEIP(2);
            il.LoadThis();
            il.LoadLocal(instLocal);
            il.EmitCalli(OpCodes.Calli, CallingConventions.Standard, typeof(void), new[] { typeof(VirtualMachine) }, null);
            il.Emit(OpCodes.Br_S, doneLabel);
            #endregion

            il.MarkLabel(invalidLabel);
            il.LoadLocal(ipLocal);
            il.Emit(OpCodes.Call, typeof(InstructionSet).GetMethod(nameof(InstructionSet.GetOpcodeException), BindingFlags.Static | BindingFlags.NonPublic));
            il.Emit(OpCodes.Throw);

            il.MarkLabel(doneLabel);
            il.LoadLocal(countLocal);
            il.LoadConstant(1);
            il.Emit(OpCodes.Sub);
            il.StoreLocal(countLocal);
            il.LoadLocal(countLocal);
            il.LoadConstant(0);
            il.Emit(OpCodes.Cgt);
            il.Emit(OpCodes.Brtrue, startLabel);

            il.Emit(OpCodes.Ret);

            return fastVm.CreateType();
        }
    }
}
