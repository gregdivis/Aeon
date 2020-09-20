using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class EmulatorBuilder
    {
        private const int VmArgument = 0;

        private bool? requiresTemp;
        private Type tempType;
        private int wordSize = 2;
        private Type wordType = typeof(ushort);
        private bool addressSize32;
        private readonly SortedList<OperandType, Func<EmitStateInfo, Emitter>> newEmitters;

        public EmulatorBuilder()
        {
            this.newEmitters = InitializeLoaders();
        }

        public void GetDelegates(InstructionInfo info, TextWriter writer)
        {
            var methodNames = new string[4];
            info.ActualMethodNames = methodNames;

            if (info.Operands.Count == 0)
            {
                // If there are no operands, just create delegates for the emulate methods directly.
                for (int i = 0; i < 4; i++)
                {
                    // These could be null if they aren't implemented yet.
                    if (info.EmulateMethods[i] != null)
                    {
                        var method = info.EmulateMethods[i];
                        var args = method.Parameters;
                        if (args[0].Type.Name == "VirtualMachine")
                        {
                            methodNames[i] = $"{method.ContainingNamespace}.{method.ContainingType.Name}.{method.Name}";
                        }
                        else
                        {
                            methodNames[i] = info.GetDecodeAndEmulateMethodName((i & 1) == 1, (i & 2) == 2);
                            BuildWrapperMethod(info, i, args[0].Type, writer);
                        }
                    }
                }
            }
            else
            {
                // The instruction has operands, so we need to generate some IL.
                methodNames[0] = info.GetDecodeAndEmulateMethodName(false, false);
                BuildMethod(writer, info, false, false);

                if (info.EmulateMethods[1] != null)
                {
                    // Prevent generating the same method multiple times.
                    if (info.EmulateMethods[0].Equals(info.EmulateMethods[1], SymbolEqualityComparer.Default) && !info.IsAffectedByOperandSize)
                    {
                        methodNames[1] = methodNames[0];
                    }
                    else
                    {
                        methodNames[1] = info.GetDecodeAndEmulateMethodName(true, false);
                        BuildMethod(writer, info, true, false);
                    }
                }

                if (info.EmulateMethods[2] != null)
                {
                    // Prevent generating the same method multiple times.
                    if (info.EmulateMethods[0].Equals(info.EmulateMethods[2], SymbolEqualityComparer.Default) && !info.IsAffectedByAddressSize)
                    {
                        methodNames[2] = methodNames[0];
                    }
                    else
                    {
                        methodNames[2] = info.GetDecodeAndEmulateMethodName(false, true);
                        BuildMethod(writer, info, false, true);
                    }
                }

                if (info.EmulateMethods[3] != null)
                {
                    // Prevent generating the same method multiple times.
                    if (info.EmulateMethods[0].Equals(info.EmulateMethods[3], SymbolEqualityComparer.Default) && !info.IsAffectedByOperandSize && !info.IsAffectedByAddressSize)
                    {
                        methodNames[3] = methodNames[0];
                    }
                    else if (info.EmulateMethods[1].Equals(info.EmulateMethods[3], SymbolEqualityComparer.Default) && !info.IsAffectedByAddressSize)
                    {
                        methodNames[3] = methodNames[1];
                    }
                    else if (info.EmulateMethods[2].Equals(info.EmulateMethods[3], SymbolEqualityComparer.Default) && !info.IsAffectedByOperandSize)
                    {
                        methodNames[3] = methodNames[2];
                    }
                    else
                    {
                        methodNames[3] = info.GetDecodeAndEmulateMethodName(true, true);
                        BuildMethod(writer, info, true, true);
                    }
                }
            }
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

        private void BuildWrapperMethod(InstructionInfo info, int methodIndex, ITypeSymbol firstArgType, TextWriter writer)
        {
            var method = info.EmulateMethods[methodIndex];
            writer.WriteLine($"\t\tpublic static void {info.ActualMethodNames[methodIndex]}(VirtualMachine vm)");
            writer.WriteLine("\t\t{");
            writer.Write($"\t\t\t{method.ContainingNamespace}.{method.ContainingType.Name}.{method.Name}(");
            writer.Write(
                firstArgType.Name switch
                {
                    "Processor" => "vm.Processor",
                    "PhysicalMemory" => "vm.PhysicalMemory",
                    _ => throw new InvalidOperationException()
                }
            );
            writer.WriteLine(");");
            writer.WriteLine("\t\t\tvm.Processor.InstructionEpilog();");
            writer.WriteLine("\t\t}");
        }

        private void BuildMethod(TextWriter writer, InstructionInfo info, bool operandSize32, bool addressSize32)
        {
            int methodIndex = (operandSize32 ? 1 : 0) | (addressSize32 ? 2 : 0);
            var emulateMethod = info.EmulateMethods[methodIndex];

            if (operandSize32)
            {
                this.wordSize = 4;
                this.wordType = typeof(uint);
            }
            else
            {
                this.wordSize = 2;
                this.wordType = typeof(ushort);
            }

            this.addressSize32 = addressSize32;

            var parameters = emulateMethod.Parameters;
            bool isByRef = parameters[1].RefKind != RefKind.None;
            bool isOut = parameters[1].RefKind == RefKind.Out;
            bool isParam2ByRef = false;

            if (parameters.Length >= 3 && parameters[2].RefKind != RefKind.None)
                isParam2ByRef = true;

            requiresTemp = false;
            tempType = null;

            writer.WriteLine($"\t\tpublic static unsafe void {info.ActualMethodNames[methodIndex]}(VirtualMachine vm)");
            writer.WriteLine("\t\t{");
            writer.WriteLine("\t\t\tvar p = vm.Processor;");
            writer.WriteLine("\t\t\tvar ip = p.CachedInstruction;");
            var emitters = CreateOperandEmitters(info, isByRef, isParam2ByRef, emulateMethod, writer);

            writer.WriteLine("\t\t\tp.EIP += (uint)(ip - p.CachedInstruction);");

            var firstArg = parameters[0].Type.Name switch
            {
                "VirtualMachine" => "vm",
                "Processor" => "p",
                "PhysicalMemory" => "vm.PhysicalMemory",
                _ => throw new NotImplementedException("arg1 " + parameters[0].Type.Name)
            };

            var methodCallInfo = new EmulateMethodCall($"{emulateMethod.ContainingNamespace}.{emulateMethod.ContainingType.Name}.{emulateMethod.Name}", firstArg, emitters);

            Emitter.WriteCall(writer, methodCallInfo);

            foreach (var emitter in emitters)
                emitter.Complete(writer);

            if (!info.IsPrefix)
                writer.WriteLine("\t\t\tp.InstructionEpilog();");

            writer.WriteLine("\t\t}");
        }
        private List<Emitter> CreateOperandEmitters(InstructionInfo info, bool isParam1ByRef, bool isParam2ByRef, IMethodSymbol method, TextWriter sb)
        {
            var emitters = new List<Emitter>(3);

            for (int index = 0; index < info.Operands.Count; index++)
            {
                bool returnValue = !(index == 0 && isParam1ByRef) && !(index == 1 && isParam2ByRef);
                var (typeCode, writeOnly) = GetMethodArgType(method.Parameters[index + 1]);
                var state = new EmitStateInfo(wordSize, returnValue ? EmitReturnType.Value : EmitReturnType.Address, addressSize32 ? 32 : 16, index, typeCode, writeOnly);

                var operand = info.Operands[index];
                if (!this.newEmitters.TryGetValue(operand, out var newEmitter))
                {
                    if (!LoadKnownRegister.IsKnownRegister(operand))
                        throw new InvalidOperationException();

                    newEmitter = s => new LoadKnownRegister(s, operand);
                }

                var emitter = newEmitter(state);
                emitters.Add(emitter);
            }

            foreach (int i in info.Operands.SortedIndices)
                emitters[i].Initialize(sb);

            return emitters;
        }

        private static (EmitterTypeCode typeCode, bool writeOnly) GetMethodArgType(IParameterSymbol arg)
        {
            var code = arg.Type.Name switch
            {
                "Byte" => EmitterTypeCode.Byte,
                "SByte" => EmitterTypeCode.SByte,
                "Int16" => EmitterTypeCode.Short,
                "UInt16" => EmitterTypeCode.UShort,
                "Int32" => EmitterTypeCode.Int,
                "UInt32" => EmitterTypeCode.UInt,
                "Int64" => EmitterTypeCode.Long,
                "UInt64" => EmitterTypeCode.ULong,
                "Float" => EmitterTypeCode.Float,
                "Double" => EmitterTypeCode.Double,
                "Real10" => EmitterTypeCode.Real10,
                "SegmentIndex" => EmitterTypeCode.SegmentIndex,
                "IntPtr" => EmitterTypeCode.IntPtr,
                _ => throw new ArgumentException("Unknown type: " + arg.Type.Name)
            };

            return (code, arg.RefKind == RefKind.Out);
        }
    }
}
