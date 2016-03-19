using CSharp2CIL.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.CodeAnalysis;
using Mono.Cecil.Cil;

namespace CSharp2CIL.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Parse(string csCode)
        {
            var tree = CSharpSyntaxTree.ParseText(csCode);
            if (!tree.GetDiagnostics().Any(d => d.IsWarningAsError))
            {
                var compilation = CSharpCompilation.Create("temp", new[] { tree },
                    new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) }, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                var stream = new MemoryStream();
                var pdbStream = new MemoryStream();
                var result = compilation.Emit(stream, pdbStream);

                if (result.Success)
                {
                    var declarations = ParseDeclarations(tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>());
                    stream.Seek(0, SeekOrigin.Begin);

                    var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolStream = pdbStream };
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, readerParameters);

                    foreach (var cilType in assemblyDefinition.MainModule.Types)
                    {
                        var codeType = declarations.FirstOrDefault(t => t.Name == cilType.Name);
                        if (codeType != null)
                        {
                            foreach (var cilMethod in cilType.Methods)
                            {
                                var codeMethod = codeType.Methods.FirstOrDefault(m => m.Name == cilMethod.Name);
                                if (codeMethod != null)
                                {
                                    codeMethod.BodyLines = ParseInstructions(cilMethod.Body.Instructions);
                                }
                            }
                        }
                    }
                    return Json(declarations);
                }
            }

            return Json("error");
        }

        private IEnumerable<CSharp2CIL.Models.Type> ParseDeclarations(IEnumerable<TypeDeclarationSyntax> typeDeclarations)
        {
            var types = new List<CSharp2CIL.Models.Type>();
            foreach (var type in typeDeclarations)
            {
                var cilType = new CSharp2CIL.Models.Type
                {
                    Name = type.Identifier.ToString(),
                    Lines = new[]
                    {
                        type.GetLocation().GetLineSpan().StartLinePosition.Line,
                        type.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line,
                        type.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line
                    }
                };
                cilType.Methods = new List<Method>();
                foreach (var method in type.Members.OfType<MethodDeclarationSyntax>())
                {
                    cilType.Methods.Add(new Method
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
                types.Add(cilType);
            }

            return types;
        }

        private List<BodyLine> ParseInstructions(IEnumerable<Instruction> instructions)
        {
            var lines = new List<BodyLine>();
            BodyLine line = null;
            foreach (var instruction in instructions)
            {
                if (instruction.SequencePoint != null && instruction.SequencePoint.StartLine < 100) //new code line
                {
                    line = new BodyLine
                    {
                        Line = instruction.SequencePoint.StartLine,
                        Instructions = new List<string>()
                    };
                    lines.Add(line);
                }
                line.Instructions.Add(instruction.ToString());
            }

            return lines;
        }

    }
}