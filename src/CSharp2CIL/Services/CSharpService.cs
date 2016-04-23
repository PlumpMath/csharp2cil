using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharp2CIL.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharp2CIL.Services
{
    public class CSharpService
    {
        public bool IsValid(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = tree.GetDiagnostics();
            return diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
        }

        public void Compile(string code, Stream peStream, Stream pdbStream)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("temp", new[] { tree },
                new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) }, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            compilation.Emit(peStream, pdbStream);
            peStream.Seek(0, SeekOrigin.Begin);
        }

        public List<CSharpType> ParseTypes(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);

            var csharpTypes = new List<CSharpType>();
            foreach (var type in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var csharpType = new CSharpType
                {
                    Name = type.Identifier.ToString(),
                    Lines = new[]
                    {
                        type.GetLocation().GetLineSpan().StartLinePosition.Line,
                        type.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line,
                        type.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line
                    },
                    Methods = new List<CSharpMethod>()
                };
                foreach (var method in type.Members.OfType<MethodDeclarationSyntax>())
                {
                    csharpType.Methods.Add(new CSharpMethod
                    {
                        Name = method.Identifier.ToString(),
                        Lines = new[]
                        {
                            method.GetLocation().GetLineSpan().StartLinePosition.Line,
                            method.Body.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line,
                            method.Body.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line
                        }
                    });
                }
                csharpTypes.Add(csharpType);
            }

            return csharpTypes;
        }
    }
}