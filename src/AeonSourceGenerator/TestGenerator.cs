using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AeonSourceGenerator
{
    [Generator]
    public sealed class TestGenerator : ISourceGenerator
    {
        public void Execute(SourceGeneratorContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;

            var buffer = new MemoryStream();
            using (var writer = new StreamWriter(buffer, Encoding.UTF8, 16, true))
            {
                WriteHeader(writer);
                WriteFooter(writer);
            }

            Debugger.Break();

            buffer.Position = 0;

            context.AddSource("Instructions", SourceText.From(buffer));

                //var instructions = GetInstructions(context, receiver.CandidateMethods);
        }

        private static void WriteHeader(TextWriter writer)
        {
            writer.WriteLine("using System;");
            writer.WriteLine();

            writer.WriteLine("namespace Aeon.Emulator.Decoding");
            writer.WriteLine('{');
            writer.WriteLine("partial class InstructionDecoders");
            writer.WriteLine('{');
        }
        private static void WriteFooter(TextWriter writer)
        {
            writer.WriteLine('}');
            writer.WriteLine('}');
        }

        private static void WriteMethod(StringBuilder builder, InstructionInfo info)
        {
            builder.AppendLine($"public static unsafe void Op_{info.Opcode:X4}(VirtualMachine vm)");
            builder.AppendLine("{");

            builder.AppendLine("}");
        }

        private static List<InstructionInfo> GetInstructions(SourceGeneratorContext context, IEnumerable<MethodDeclarationSyntax> candidateMethods)
        {
            var attributeSymbol = context.Compilation.GetTypeByMetadataName("Aeon.Emulator.Instructions.OpcodeAttribute");
            var instructions = new List<InstructionInfo>();

            foreach (var method in candidateMethods)
            {
                var model = context.Compilation.GetSemanticModel(method.SyntaxTree);
                var methodSymbol = model.GetDeclaredSymbol(method);
                foreach (var attr in methodSymbol.GetAttributes())
                {
                    if (attr.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default))
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

                            foreach (var code in opcode.Split('|'))
                            {
                                var info = InstructionInfo.Parse(code);
                                info.Name = name;
                                info.IsPrefix = prefix;
                                info.SetEmulateMethods(operandSize, addressSize, methodSymbol);
                                instructions.Add(info);
                            }
                        }
                    }
                }
            }

            return instructions;
        }

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();

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
