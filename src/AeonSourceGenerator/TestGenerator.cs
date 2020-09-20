using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void Execute(SourceGeneratorContext context)
        {
            using var writer = new StringWriter();
            try
            {
                if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                    return;

                var instructions = GetInstructions(context, receiver.CandidateMethods);

                var builder = new EmulatorBuilder();

                WriteHeader(writer);
                foreach (var inst in instructions.OrderBy(i => i.Opcode).Take(100))
                    builder.GetDelegates(inst, writer);

                WriteFooter(writer);

                var text = writer.ToString();
                File.WriteAllText(@"C:\Code\Instructions.cs", text, Encoding.UTF8);

                context.AddSource("Instructions", SourceText.From(text, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                File.WriteAllText(@"C:\Code\GeneratorError.txt", ex.ToString(), Encoding.UTF8);
                File.WriteAllText(@"C:\Code\Instructions.cs", writer.ToString(), Encoding.UTF8);
                throw;
            }
        }

        private static void WriteHeader(TextWriter writer)
        {
            writer.WriteLine("using System;");
            writer.WriteLine();

            writer.WriteLine("namespace Aeon.Emulator.Decoding");
            writer.WriteLine('{');
            writer.WriteLine("\tpartial class InstructionDecoders");
            writer.WriteLine("\t{");
        }
        private static void WriteFooter(TextWriter writer)
        {
            writer.WriteLine("\t}");
            writer.WriteLine('}');
        }

        private static IEnumerable<InstructionInfo> GetInstructions(SourceGeneratorContext context, IEnumerable<MethodDeclarationSyntax> candidateMethods)
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
