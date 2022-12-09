using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Aeon.SourceGenerator
{
    [Generator(LanguageNames.CSharp)]
    public sealed partial class DecoderGenerator : IIncrementalGenerator
    {
        private const string OpcodeAttributeName = "Aeon.Emulator.Instructions.OpcodeAttribute";
        private const string AlternateAttributeName = "Aeon.Emulator.Instructions.AlternateAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var opcodes = context.SyntaxProvider.ForAttributeWithMetadataName(
                OpcodeAttributeName,
                static (n, _) => n is MethodDeclarationSyntax,
                GetOpcodeMethod
            ).Where(static o => o is not null);

            var alternates = context.SyntaxProvider.ForAttributeWithMetadataName(
                AlternateAttributeName,
                static (n, _) => n is MethodDeclarationSyntax,
                GetAlternateMethod
            ).Where(static m => m is not null).Collect().Select(static (m, _) => m.ToLookup(a => a!.OpcodeMethod, SymbolEqualityComparer.Default));

            var methodGroups = opcodes.Combine(alternates).Select(GetMethodGroup).Where(static g => g is not null);

            var units = methodGroups.SelectMany(GetCodeGenUnits).Collect();

            context.RegisterImplementationSourceOutput(units, GenerateSource);
        }

        private static OpcodeMethod GetOpcodeMethod(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
            var sm = context.SemanticModel;

            var compilation = sm.Compilation;
            var opcodeAttributeSymbol = compilation.GetTypeByMetadataName(OpcodeAttributeName);

            if (opcodeAttributeSymbol is null)
                return null;

            if (context.TargetSymbol is not IMethodSymbol opcodeMethodSymbol)
                return null;

            var boundAttributes = opcodeMethodSymbol.GetAttributes();
            if (boundAttributes.Length == 0)
                return null;

            string opcode = null;
            string name = null;
            bool prefix = false;
            int operandSize = 16;
            int addressSize = 16;

            foreach (var attributeData in boundAttributes)
            {
                if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, opcodeAttributeSymbol))
                    continue;

                if (attributeData.ConstructorArguments.Any(ca => ca.Kind == TypedConstantKind.Error))
                    return null;

                var items = attributeData.ConstructorArguments;
                if (items.Length != 1)
                    return null;

                if (opcode != null)
                    return null;

                opcode = items[0].Value as string;

                foreach (var namedArg in attributeData.NamedArguments)
                {
                    if (namedArg.Key == "Name" && namedArg.Value.Value is string name2)
                        name = name2;
                    else if (namedArg.Key == "IsPrefix" && namedArg.Value.Value is bool prefix2)
                        prefix = prefix2;
                    else if (namedArg.Key == "OperandSize" && namedArg.Value.Value is int opSize)
                        operandSize = opSize;
                    else if (namedArg.Key == "AddressSize" && namedArg.Value.Value is int adrSize)
                        addressSize = adrSize;
                }

                name ??= opcodeMethodSymbol.ContainingType.Name.ToLowerInvariant();
            }

            if (opcode == null)
                return null;

            return new OpcodeMethod(opcode, operandSize, addressSize, opcodeMethodSymbol, name, prefix);
        }
        private static AlternateMethod GetAlternateMethod(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
            var sm = context.SemanticModel;

            var compilation = sm.Compilation;
            var alternateAttributeSymbol = compilation.GetTypeByMetadataName(AlternateAttributeName);

            if (alternateAttributeSymbol is null)
                return null;

            if (context.TargetSymbol is not IMethodSymbol alternateMethodSymbol)
                return null;

            var boundAttributes = alternateMethodSymbol.GetAttributes();
            if (boundAttributes.Length == 0)
                return null;

            string methodName = null;
            int operandSize = 32;
            int addressSize = 16;

            foreach (var attributeData in boundAttributes)
            {
                if (!SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, alternateAttributeSymbol))
                    continue;

                if (attributeData.ConstructorArguments.Any(ca => ca.Kind == TypedConstantKind.Error))
                    return null;

                var items = attributeData.ConstructorArguments;
                if (items.Length != 1)
                    return null;

                if (methodName != null)
                    return null;

                methodName = items[0].Value as string;

                foreach (var namedArg in attributeData.NamedArguments)
                {
                    if (namedArg.Key == "OperandSize" && namedArg.Value.Value is int opSize)
                        operandSize = opSize;
                    else if (namedArg.Key == "AddressSize" && namedArg.Value.Value is int adrSize)
                        addressSize = adrSize;
                }
            }

            if (methodName == null)
                return null;

            var matches = alternateMethodSymbol.ContainingType.GetMembers(methodName);
            if (matches.Length != 1 || matches[0] is not IMethodSymbol targetMethod)
                return null;

            return new AlternateMethod(targetMethod, operandSize, addressSize, alternateMethodSymbol);
        }
        private static OpcodeMethodGroup GetMethodGroup((OpcodeMethod opcode, ILookup<ISymbol, AlternateMethod> alternates) input, CancellationToken cancellationToken)
        {
            var (opcode, alternates) = input;

            if (opcode is null || alternates is null)
                return null;

            var emulateMethods = new IOpcodeMethod[4];

            addMethod(opcode, emulateMethods);

            foreach (var alt in alternates[opcode.Method])
            {
                if (alt is null)
                    return null;

                addMethod(alt, emulateMethods);
            }

            return new OpcodeMethodGroup(opcode, emulateMethods.ToImmutableArray());

            static void addMethod(IOpcodeMethod method, IOpcodeMethod[] emulateMethods)
            {
                if ((method.OperandSize & 16) != 0 && (method.AddressSize & 16) != 0)
                    emulateMethods[0] = method;
                if ((method.OperandSize & 32) != 0 && (method.AddressSize & 16) != 0)
                    emulateMethods[1] = method;
                if ((method.OperandSize & 16) != 0 && (method.AddressSize & 32) != 0)
                    emulateMethods[2] = method;
                if ((method.OperandSize & 32) != 0 && (method.AddressSize & 32) != 0)
                    emulateMethods[3] = method;
            }
        }
        private static IEnumerable<CodeGenUnit> GetCodeGenUnits(OpcodeMethodGroup methodGroup, CancellationToken cancellationToken)
        {
            if (InstructionInfo.TryParse(methodGroup.OpcodeMethod.OpcodeText, out var values))
            {
                foreach (var v in values)
                    yield return CodeGenUnit.Build(v, methodGroup);
            }
        }

        private static void GenerateSource(SourceProductionContext context, ImmutableArray<CodeGenUnit> units)
        {
            var chunks = new Queue<string>(units.Length + 10);

            chunks.Enqueue("""
                using System;
                using System.Runtime.CompilerServices;
                using static Aeon.Emulator.Decoding.InstructionDecoders;
                using static Aeon.Emulator.Decoding.OpDecoders;

                namespace Aeon.Emulator.Decoding;

                static file class OpDecoders
                {

                """
            );

            foreach (var unit in units.OrderBy(u => u.Instruction.Opcode))
            {
                if (!string.IsNullOrEmpty(unit.GeneratedCode))
                    chunks.Enqueue(unit.GeneratedCode);
            }

            chunks.Enqueue("""
                }

                partial class InstructionSet
                {
                """
            );

            chunks.Enqueue(GetOneByteInit(units.Where(i => !i.Instruction.IsMultiByte && i.Instruction.ModRmByte != ModRmInfo.OnlyRm).OrderBy(i => i.Instruction.Opcode)));
            chunks.Enqueue(GetOneByteRmInit(units.Where(i => !i.Instruction.IsMultiByte && i.Instruction.ModRmByte == ModRmInfo.OnlyRm).OrderBy(i => i.Instruction.Opcode).ThenBy(i => i.Instruction.ExtendedRmOpcode)));
            chunks.Enqueue(GetTwoByteInit(units.OrderBy(i => i.Instruction.Opcode).Where(i => i.Instruction.IsMultiByte && i.Instruction.ModRmByte != ModRmInfo.OnlyRm)));
            chunks.Enqueue(GetTwoByteRmInit(units.Where(i => i.Instruction.IsMultiByte && i.Instruction.ModRmByte == ModRmInfo.OnlyRm).OrderBy(i => i.Instruction.Opcode).ThenBy(i => i.Instruction.ExtendedRmOpcode)));

            chunks.Enqueue("}");

            int length = chunks.Sum(c => c.Length);
            context.AddSource("Instructions.g.cs", SourceText.From(new ChunkTextReader(chunks), length, Encoding.UTF8));
        }
        private static string GetOneByteInit(IEnumerable<CodeGenUnit> units)
        {
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw) { Indent = 1 };

            writer.WriteLine();
            writer.WriteLine("private static unsafe partial void GetOneBytePointers(delegate*<VirtualMachine, void>** ptrs)");
            writer.WriteLine('{');
            writer.Indent++;
            foreach (var unit in units)
            {
                for (int m = 0; m < 4; m++)
                {
                    if (!string.IsNullOrEmpty(unit.MethodNames[m]))
                        writer.WriteLine($"ptrs[{m}][{unit.Instruction.Opcode & 0xFF}] = &{unit.MethodNames[m]};");
                }
            }

            writer.Indent--;
            writer.WriteLine('}');

            writer.Flush();
            return sw.ToString();
        }
        private static string GetTwoByteInit(IEnumerable<CodeGenUnit> units)
        {
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw) { Indent = 1 };

            writer.WriteLine();
            writer.WriteLine("private static unsafe partial void GetTwoBytePointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc)");
            writer.WriteLine('{');
            writer.Indent++;
            foreach (var g in units.GroupBy(i => i.Instruction.Opcode & 0xFF))
            {
                for (int m = 0; m < 4; m++)
                    writer.WriteLine($"ptrs[{m}][{g.Key}] = (delegate*<VirtualMachine, void>*)alloc(256);");

                foreach (var unit in g)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        if (!string.IsNullOrEmpty(unit.MethodNames[m]))
                            writer.WriteLine($"ptrs[{m}][{unit.Instruction.Opcode & 0xFF}][{unit.Instruction.Opcode >>> 8}] = &{unit.MethodNames[m]};");
                    }
                }
            }

            writer.Indent--;
            writer.WriteLine('}');

            writer.Flush();
            return sw.ToString();
        }
        private static string GetOneByteRmInit(IEnumerable<CodeGenUnit> units)
        {
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw) { Indent = 1 };

            writer.WriteLine();
            writer.WriteLine("private static unsafe partial void GetOneByteRmPointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc)");
            writer.WriteLine('{');
            writer.Indent++;
            foreach (var g in units.GroupBy(i => i.Instruction.Opcode & 0xFF))
            {
                for (int m = 0; m < 4; m++)
                    writer.WriteLine($"ptrs[{m}][{g.Key}] = (delegate*<VirtualMachine, void>*)alloc(8);");

                foreach (var unit in g)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        if (!string.IsNullOrEmpty(unit.MethodNames[m]))
                            writer.WriteLine($"ptrs[{m}][{unit.Instruction.Opcode & 0xFF}][{unit.Instruction.ExtendedRmOpcode}] = &{unit.MethodNames[m]};");
                    }
                }
            }

            writer.Indent--;
            writer.WriteLine('}');

            writer.Flush();
            return sw.ToString();
        }
        private static string GetTwoByteRmInit(IEnumerable<CodeGenUnit> units)
        {
            using var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw) { Indent = 1 };

            writer.WriteLine();
            writer.WriteLine("private static unsafe partial void GetTwoByteRmPointers(delegate*<VirtualMachine, void>**** ptrs, Func<int, nint> alloc)");
            writer.WriteLine('{');
            writer.Indent++;
            foreach (var g in units.GroupBy(i => i.Instruction.Opcode & 0xFF))
            {
                for (int m = 0; m < 4; m++)
                    writer.WriteLine($"ptrs[{m}][{g.Key}] = (delegate*<VirtualMachine, void>**)alloc(256);");

                foreach (var g2 in g.GroupBy(i => i.Instruction.Opcode >>> 8))
                {
                    for (int m = 0; m < 4; m++)
                        writer.WriteLine($"ptrs[{m}][{g.Key}][{g2.Key}] = (delegate*<VirtualMachine, void>*)alloc(8);");

                    foreach (var unit in g2)
                    {
                        for (int m = 0; m < 4; m++)
                        {
                            if (!string.IsNullOrEmpty(unit.MethodNames[m]))
                                writer.WriteLine($"ptrs[{m}][{g.Key}][{g2.Key}][{unit.Instruction.ExtendedRmOpcode}] = &{unit.MethodNames[m]};");
                        }
                    }
                }
            }

            writer.Indent--;
            writer.WriteLine('}');

            writer.Flush();
            return sw.ToString();
        }
    }
}
