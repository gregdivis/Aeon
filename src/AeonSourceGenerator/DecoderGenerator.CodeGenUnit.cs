using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Aeon.SourceGenerator.Emitters;
using Microsoft.CodeAnalysis;

namespace Aeon.SourceGenerator
{
    public sealed partial class DecoderGenerator
    {
        private sealed class CodeGenUnit : IEquatable<CodeGenUnit>
        {
            private static readonly SortedList<OperandType, Func<EmitStateInfo, Emitter>> emitterFactories = InitializeLoaders();

            private CodeGenUnit(InstructionInfo instruction, string[] methodNames, string generatedCode)
            {
                this.Instruction = instruction;
                this.MethodNames = ImmutableArray.Create(methodNames);
                this.GeneratedCode = generatedCode;
            }

            public InstructionInfo Instruction { get; }
            public ImmutableArray<string> MethodNames { get; }
            public string GeneratedCode { get; }

            public bool Equals(CodeGenUnit other)
            {
                if (ReferenceEquals(this, other))
                    return true;
                if (other is null)
                    return false;

                if (!this.Instruction.Equals(other.Instruction) || this.GeneratedCode != other.GeneratedCode || this.MethodNames.Length != other.MethodNames.Length)
                    return false;

                for (int i = 0; i < this.MethodNames.Length; i++)
                {
                    if (this.MethodNames[i] != other.MethodNames[i])
                        return false;
                }

                return true;
            }
            public override bool Equals(object obj) => this.Equals(obj as CodeGenUnit);
            public override int GetHashCode() => this.Instruction.GetHashCode();

            public static CodeGenUnit Build(InstructionInfo instruction, OpcodeMethodGroup methodGroup)
            {
                var methodNames = new string[4];
                string code = null;

                if (instruction.Operands.Count == 0 && methodGroup.OpcodeMethod.Method.Parameters[0].Type.Name == "VirtualMachine")
                {
                    for (int i = 0; i < methodNames.Length; i++)
                    {
                        var method = methodGroup.EmulateMethods[i]?.Method;
                        if (method != null)
                            methodNames[i] = $"{method.ContainingNamespace}.{method.ContainingType.Name}.{method.Name}";
                    }
                }
                else
                {
                    var methodO16A16 = methodGroup.EmulateMethods[0]?.Method;
                    var methodO32A16 = methodGroup.EmulateMethods[1]?.Method;
                    var methodO16A32 = methodGroup.EmulateMethods[2]?.Method;
                    var methodO32A32 = methodGroup.EmulateMethods[3]?.Method;

                    if ((methodO16A16 ?? methodO32A16 ?? methodO16A32 ?? methodO32A32) is not null)
                    {
                        using var sw = new StringWriter();
                        using var writer = new IndentedTextWriter(sw) { Indent = 1 };
                        writer.Write(IndentedTextWriter.DefaultTabString);

                        bool isPrefix = methodGroup.OpcodeMethod.IsPrefix;

                        if (methodO16A16 != null)
                        {
                            methodNames[0] = GetMethodName(instruction, false, false);
                            WriteMethod(writer, instruction, methodNames[0], methodO16A16, isPrefix, false, false);
                        }

                        if (methodO32A16 != null)
                        {
                            if (!instruction.IsAffectedByOperandSize && methodO32A16.Equals(methodO16A16, SymbolEqualityComparer.Default))
                            {
                                methodNames[1] = methodNames[0];
                            }
                            else
                            {
                                methodNames[1] = GetMethodName(instruction, true, false);
                                WriteMethod(writer, instruction, methodNames[1], methodO32A16, isPrefix, true, false);
                            }
                        }

                        if (methodO16A32 != null)
                        {
                            if (!instruction.IsAffectedByAddressSize && methodO16A32.Equals(methodO16A16, SymbolEqualityComparer.Default))
                            {
                                methodNames[2] = methodNames[0];
                            }
                            else if (!instruction.IsAffectedByOperandSize && !instruction.IsAffectedByAddressSize && methodO16A32.Equals(methodO32A16, SymbolEqualityComparer.Default))
                            {
                                methodNames[2] = methodNames[1];
                            }
                            else
                            {
                                methodNames[2] = GetMethodName(instruction, false, true);
                                WriteMethod(writer, instruction, methodNames[2], methodO16A32, isPrefix, false, true);
                            }
                        }

                        if (methodO32A32 != null)
                        {
                            if (!instruction.IsAffectedByAddressSize && !instruction.IsAffectedByOperandSize && methodO32A32.Equals(methodO16A16, SymbolEqualityComparer.Default))
                            {
                                methodNames[3] = methodNames[0];
                            }
                            else if (!instruction.IsAffectedByAddressSize && methodO32A32.Equals(methodO32A16, SymbolEqualityComparer.Default))
                            {
                                methodNames[3] = methodNames[1];
                            }
                            else if (!instruction.IsAffectedByOperandSize && methodO32A32.Equals(methodO16A32, SymbolEqualityComparer.Default))
                            {
                                methodNames[3] = methodNames[2];
                            }
                            else
                            {
                                methodNames[3] = GetMethodName(instruction, true, true);
                                WriteMethod(writer, instruction, methodNames[3], methodO32A32, isPrefix, true, true);
                            }
                        }

                        writer.Flush();
                        code = sw.ToString();
                    }
                }

                return new CodeGenUnit(instruction, methodNames, code);
            }

            private static string GetMethodName(InstructionInfo info, bool operand32, bool address32)
            {
                int operandSize = operand32 ? 32 : 16;
                int addressSize = address32 ? 32 : 16;
                if (info.ModRmByte == ModRmInfo.OnlyRm)
                    return $"Op_{info.Opcode:X4}_R{info.ExtendedRmOpcode}_O{operandSize}_A{addressSize}";
                else
                    return $"Op_{info.Opcode:X4}_O{operandSize}_A{addressSize}";
            }
            private static void WriteMethod(IndentedTextWriter writer, InstructionInfo instruction, string methodName, IMethodSymbol emulateMethod, bool isPrefix, bool operandSize32, bool addressSize32)
            {
                writer.Write("public static unsafe void ");
                writer.Write(methodName);
                writer.WriteLine("(VirtualMachine vm)");
                writer.WriteLine('{');
                writer.Indent++;

                if (instruction.Operands.Count == 0)
                {
                    writer.Write(emulateMethod.ContainingNamespace);
                    writer.Write('.');
                    writer.Write(emulateMethod.ContainingType.Name);
                    writer.Write('.');
                    writer.Write(emulateMethod.Name);
                    writer.Write('(');
                    writer.Write(
                        emulateMethod.Parameters[0].Type.Name switch
                        {
                            "VirtualMachine" => "vm",
                            "Processor" => "vm.Processor",
                            "PhysicalMemory" => "vm.PhysicalMemory",
                            "FPU" => "vm.Processor.FPU",
                            _ => throw new ArgumentOutOfRangeException()
                        }
                    );
                    writer.WriteLine(");");
                    if (!isPrefix)
                        writer.WriteLine("vm.Processor.InstructionEpilog();");
                }
                else
                {
                    var parameters = emulateMethod.Parameters;
                    bool isByRef = parameters[1].RefKind != RefKind.None;
                    bool isParam2ByRef = false;

                    if (parameters.Length >= 3 && parameters[2].RefKind != RefKind.None)
                        isParam2ByRef = true;

                    var firstArg = parameters[0].Type.Name switch
                    {
                        "VirtualMachine" => "vm",
                        "Processor" => "p",
                        "PhysicalMemory" => "vm.PhysicalMemory",
                        _ => throw new NotImplementedException($"arg1 {parameters[0].Type.Name}")
                    };

                    writer.WriteLine("var p = vm.Processor;");
                    var emitters = CreateOperandEmitters(instruction, isByRef, isParam2ByRef, emulateMethod, writer, addressSize32, operandSize32 ? 4 : 2);
                    if (!instruction.TryGetOperandSize(operandSize32, addressSize32, out int eipIncrement))
                    {
                        writer.WriteLine("p.EIP = (uint)(p.CachedIP - p.CachedInstruction + p.StartEIP);");
                    }
                    else if (eipIncrement == 1)
                    {
                        writer.WriteLine("p.EIP++;");
                    }
                    else if (eipIncrement > 0)
                    {
                        writer.Write("p.EIP += ");
                        writer.Write(eipIncrement);
                        writer.WriteLine(';');
                    }

                    var methodCallInfo = new EmulateMethodCall($"{emulateMethod.ContainingNamespace}.{emulateMethod.ContainingType.Name}.{emulateMethod.Name}", firstArg, emitters);
                    Emitter.WriteCall(writer, methodCallInfo);
                    foreach (var emitter in emitters)
                        emitter.Complete(writer);

                    if (!isPrefix)
                        writer.WriteLine("p.InstructionEpilog();");
                }

                writer.Indent--;
                writer.WriteLine('}');
            }
            private static List<Emitter> CreateOperandEmitters(InstructionInfo info, bool isParam1ByRef, bool isParam2ByRef, IMethodSymbol method, IndentedTextWriter writer, bool addressSize32, int wordSize)
            {
                var emitters = new List<Emitter>(3);

                for (int index = 0; index < info.Operands.Count; index++)
                {
                    bool returnValue = !(index == 0 && isParam1ByRef) && !(index == 1 && isParam2ByRef);
                    var (typeCode, writeOnly) = GetMethodArgType(method.Parameters[index + 1]);
                    var state = new EmitStateInfo(wordSize, returnValue ? EmitReturnType.Value : EmitReturnType.Address, addressSize32 ? 32 : 16, index, typeCode, writeOnly);

                    var operand = info.Operands[index];
                    if (!emitterFactories.TryGetValue(operand, out var newEmitter))
                    {
                        if (!LoadKnownRegister.IsKnownRegister(operand))
                            throw new InvalidOperationException();

                        newEmitter = s => new LoadKnownRegister(s, operand);
                    }

                    var emitter = newEmitter(state);
                    emitters.Add(emitter);
                }

                foreach (int i in info.Operands.SortedIndices)
                    emitters[i].Initialize(writer);

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
                    "Single" => EmitterTypeCode.Float,
                    "Double" => EmitterTypeCode.Double,
                    "Real10" => EmitterTypeCode.Real10,
                    "SegmentIndex" => EmitterTypeCode.SegmentIndex,
                    "IntPtr" => EmitterTypeCode.IntPtr,
                    _ => throw new ArgumentException("Unknown type: " + arg.Type.Name)
                };

                return (code, arg.RefKind == RefKind.Out);
            }
            private static SortedList<OperandType, Func<EmitStateInfo, Emitter>> InitializeLoaders()
            {
                return new SortedList<OperandType, Func<EmitStateInfo, Emitter>>
                {
                    { OperandType.ImmediateByte, s => new LoadImmediate(s, 1, ValueExtend.Zero) },
                    { OperandType.ImmediateByteExtend, s => new LoadImmediate(s, 1, ValueExtend.Sign) },
                    { OperandType.ImmediateRelativeByte, s => new LoadImmediate(s, 1, ValueExtend.Sign) },
                    { OperandType.ImmediateWord, s => new LoadImmediate(s, s.WordSize, ValueExtend.Zero) },
                    { OperandType.ImmediateRelativeWord, s => new LoadImmediate(s, s.WordSize, ValueExtend.Zero) },
                    { OperandType.ImmediateInt16, s => new LoadImmediate(s, 2, ValueExtend.Zero) },
                    { OperandType.RegisterByte, s => new LoadRegister(s, 1) },
                    { OperandType.RegisterWord, s => new LoadRegister(s, s.WordSize) },
                    { OperandType.SegmentRegister, s => new LoadSegmentRegister(s) },
                    { OperandType.MemoryOffsetByte, s => new LoadMoffs(s, 1) },
                    { OperandType.MemoryOffsetWord, s => new LoadMoffs(s, s.WordSize) },
                    { OperandType.RegisterOrMemoryByte, s => LoadRegRmw.Create(s, 1, false) },
                    { OperandType.RegisterOrMemoryWord, s => LoadRegRmw.Create(s, s.WordSize, false) },
                    { OperandType.RegisterOrMemoryWordNearPointer, s => LoadRegRmw.Create(s, s.WordSize, false) },
                    { OperandType.MemoryInt16, s => LoadRegRmw.Create(s, 2, true) },
                    { OperandType.RegisterOrMemory16, s => LoadRegRmw.Create(s, 2, false) },
                    { OperandType.MemoryInt32, s => LoadRegRmw.Create(s, 4, true) },
                    { OperandType.RegisterOrMemory32, s => LoadRegRmw.Create(s, 4, false) },
                    { OperandType.MemoryInt64, s => LoadRegRmw.Create(s, 8, true) },
                    { OperandType.ImmediateFarPointer, s => new LoadImmediate(s, s.WordSize + 2, ValueExtend.Zero) },
                    { OperandType.IndirectFarPointer, s => LoadRegRmw.Create(s, s.WordSize * 2, true) },
                    { OperandType.MemoryFloat32, s => LoadRegRmw.CreateFloat(s, 4) },
                    { OperandType.MemoryFloat64, s => LoadRegRmw.CreateFloat(s, 8) },
                    { OperandType.MemoryFloat80, s => LoadRegRmw.CreateFloat(s, 10) },
                    { OperandType.DebugRegister, s => new LoadDebugRegister(s) },
                    { OperandType.EffectiveAddress, LoadRegRmw.CreateEffectiveAddress },
                    { OperandType.FullLinearAddress, LoadRegRmw.CreateLinearAddress }
                };
            }
        }
    }
}
