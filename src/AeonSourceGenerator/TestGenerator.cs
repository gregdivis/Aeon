using System.Text;
using AeonSourceGenerator.Emitters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AeonSourceGenerator
{
    [Generator]
    public sealed class TestGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var writer = new StringBuilder();
            try
            {
                if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                    return;

                var instructions = GetInstructions(context, receiver.CandidateMethods);

                var builder = new EmulatorBuilder();

                WriteHeader(writer);
                foreach (var inst in instructions.OrderBy(i => i.Opcode))
                    builder.GetDelegates(inst, writer);

                WriteOneByteInit(writer, instructions.Where(i => !i.IsMultiByte && i.ModRmByte != ModRmInfo.OnlyRm).OrderBy(i => i.Opcode));
                WriteOneByteRmInit(writer, instructions.Where(i => !i.IsMultiByte && i.ModRmByte == ModRmInfo.OnlyRm).OrderBy(i => i.Opcode).ThenBy(i => i.ExtendedRmOpcode));
                WriteTwoByteInit(writer, instructions.OrderBy(i => i.Opcode).Where(i => i.IsMultiByte && i.ModRmByte != ModRmInfo.OnlyRm));
                WriteTwoByteRmInit(writer, instructions.Where(i => i.IsMultiByte && i.ModRmByte == ModRmInfo.OnlyRm).OrderBy(i => i.Opcode).ThenBy(i => i.ExtendedRmOpcode));

                WriteFooter(writer);

                var text = writer.ToString();
                //File.WriteAllText(@"C:\Projects\Instructions.cs", text, Encoding.UTF8);

                context.AddSource("Instructions", SourceText.From(text, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                File.WriteAllText(@"C:\Projects\GeneratorError.txt", ex.ToString(), Encoding.UTF8);
                File.WriteAllText(@"C:\Projects\Instructions.cs", writer.ToString(), Encoding.UTF8);
                throw;
            }
        }

        private static void WriteOneByteInit(StringBuilder writer, IEnumerable<InstructionInfo> infos)
        {
            writer.AppendLine();
            writer.AppendLine("\tpublic static unsafe partial void GetOneBytePointers(delegate*<VirtualMachine, void>** ptrs)");
            writer.AppendLine("\t{");
            foreach (var inst in infos)
            {
                for (int m = 0; m < 4; m++)
                {
                    if (!string.IsNullOrEmpty(inst.ActualMethodNames[m]))
                        writer.AppendLine($"\t\tptrs[{m}][{inst.Opcode & 0xFF}] = &{inst.ActualMethodNames[m]};");
                }
            }

            writer.AppendLine("\t}");
        }
        private static void WriteTwoByteInit(StringBuilder writer, IEnumerable<InstructionInfo> infos)
        {
            writer.AppendLine();
            writer.AppendLine("\tpublic static unsafe partial void GetTwoBytePointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc)");
            writer.AppendLine("\t{");
            foreach (var g in infos.GroupBy(i => i.Opcode & 0xFF))
            {
                for (int m = 0; m < 4; m++)
                    writer.AppendLine($"\t\tptrs[{m}][{g.Key}] = (delegate*<VirtualMachine, void>*)alloc(256);");

                foreach (var inst in g)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        if (!string.IsNullOrEmpty(inst.ActualMethodNames[m]))
                            writer.AppendLine($"\t\tptrs[{m}][{inst.Opcode & 0xFF}][{inst.Opcode >>> 8}] = &{inst.ActualMethodNames[m]};");
                    }
                }
            }

            writer.AppendLine("\t}");
        }
        private static void WriteOneByteRmInit(StringBuilder writer, IEnumerable<InstructionInfo> infos)
        {
            writer.AppendLine();
            writer.AppendLine("\tpublic static unsafe partial void GetOneByteRmPointers(delegate*<VirtualMachine, void>*** ptrs, Func<int, nint> alloc)");
            writer.AppendLine("\t{");
            foreach (var g in infos.GroupBy(i => i.Opcode & 0xFF))
            {
                for (int m = 0; m < 4; m++)
                    writer.AppendLine($"\t\tptrs[{m}][{g.Key}] = (delegate*<VirtualMachine, void>*)alloc(8);");

                foreach (var inst in g)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        if (!string.IsNullOrEmpty(inst.ActualMethodNames[m]))
                            writer.AppendLine($"\t\tptrs[{m}][{inst.Opcode & 0xFF}][{inst.ExtendedRmOpcode}] = &{inst.ActualMethodNames[m]};");
                    }
                }
            }

            writer.AppendLine("\t}");
        }
        private static void WriteTwoByteRmInit(StringBuilder writer, IEnumerable<InstructionInfo> infos)
        {
            writer.AppendLine();
            writer.AppendLine("\tpublic static unsafe partial void GetTwoByteRmPointers(delegate*<VirtualMachine, void>**** ptrs, Func<int, nint> alloc)");
            writer.AppendLine("\t{");
            foreach (var g in infos.GroupBy(i => i.Opcode & 0xFF))
            {
                for (int m = 0; m < 4; m++)
                    writer.AppendLine($"\t\tptrs[{m}][{g.Key}] = (delegate*<VirtualMachine, void>**)alloc(256);");

                foreach (var g2 in g.GroupBy(i => i.Opcode >>> 8))
                {
                    for (int m = 0; m < 4; m++)
                        writer.AppendLine($"\t\tptrs[{m}][{g.Key}][{g2.Key}] = (delegate*<VirtualMachine, void>*)alloc(8);");

                    foreach (var inst in g2)
                    {
                        for (int m = 0; m < 4; m++)
                        {
                            if (!string.IsNullOrEmpty(inst.ActualMethodNames[m]))
                                writer.AppendLine($"\t\tptrs[{m}][{g.Key}][{g2.Key}][{inst.ExtendedRmOpcode}] = &{inst.ActualMethodNames[m]};");
                        }
                    }
                }
            }

            writer.AppendLine("\t}");
        }

        private static void WriteHeader(StringBuilder writer)
        {
            writer.AppendLine("using System;");
            writer.AppendLine("using System.Runtime.CompilerServices;");
            writer.AppendLine();

            writer.AppendLine("namespace Aeon.Emulator.Decoding;");
            writer.AppendLine();
            writer.AppendLine("partial class InstructionDecoders");
            writer.AppendLine("{");
        }
        private static void WriteFooter(StringBuilder writer)
        {
            writer.AppendLine("}");
        }

        private static IEnumerable<InstructionInfo> GetInstructions(GeneratorExecutionContext context, IEnumerable<MethodDeclarationSyntax> candidateMethods)
        {
            var opcodeAttributeSymbol = context.Compilation.GetTypeByMetadataName("Aeon.Emulator.Instructions.OpcodeAttribute");
            var alternateAttributeSymbol = context.Compilation.GetTypeByMetadataName("Aeon.Emulator.Instructions.AlternateAttribute");
            var instructions = new Dictionary<IMethodSymbol, List<InstructionInfo>>(SymbolEqualityComparer.Default);

            var alternates = new List<(IMethodSymbol method, AttributeData attr)>();

            foreach (var method in candidateMethods)
            {
                var model = context.Compilation.GetSemanticModel(method.SyntaxTree);
                var methodSymbol = model.GetDeclaredSymbol(method);

                foreach (var attr in methodSymbol.GetAttributes())
                {
                    if (attr.AttributeClass.Equals(opcodeAttributeSymbol, SymbolEqualityComparer.Default))
                    {
                        if (attr.ConstructorArguments.Length == 1 && attr.ConstructorArguments[0].Value is string opcode)
                        {
                            string name = null;
                            bool prefix = false;
                            int operandSize = 16;
                            int addressSize = 16;

                            foreach (var namedArg in attr.NamedArguments)
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

                            name ??= methodSymbol.ContainingType.Name.ToLowerInvariant();

                            var infosToAdd = new List<InstructionInfo>();

                            foreach (var code in opcode.Split('|'))
                            {
                                var info = InstructionInfo.Parse(code);
                                info.Name = name;
                                info.IsPrefix = prefix;
                                info.SetEmulateMethods(operandSize, addressSize, methodSymbol);

                                foreach (var subInst in info.Expand())
                                    infosToAdd.Add(subInst);
                            }

                            instructions.Add(methodSymbol, infosToAdd);
                        }
                    }
                    else if (attr.AttributeClass.Equals(alternateAttributeSymbol, SymbolEqualityComparer.Default))
                    {
                        alternates.Add((methodSymbol, attr));
                    }
                }
            }

            foreach (var (method, attr) in alternates)
            {
                if (attr.ConstructorArguments.Length == 1 && attr.ConstructorArguments[0].Value is string primaryMethodName)
                {
                    var primaryMethod = method.ContainingType.GetMembers(primaryMethodName).FirstOrDefault();
                    if (primaryMethod is IMethodSymbol primaryMethodSymbol && instructions.TryGetValue(primaryMethodSymbol, out var infos))
                    {
                        int operandSize = 32;
                        int addressSize = 16;

                        foreach (var namedArg in attr.NamedArguments)
                        {
                            if (namedArg.Key == "OperandSize" && namedArg.Value.Value is int opSize)
                                operandSize = opSize;
                            else if (namedArg.Key == "AddressSize" && namedArg.Value.Value is int adrSize)
                                addressSize = adrSize;
                        }

                        foreach (var info in infos)
                            info.SetEmulateMethods(operandSize, addressSize, method);
                    }
                }
            }

            return instructions.Values.SelectMany(i => i);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>(1000);

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
                {
                    if (methodDeclarationSyntax.AttributeLists.Count > 0)
                        this.CandidateMethods.Add(methodDeclarationSyntax);
                }
            }
        }
    }
}
