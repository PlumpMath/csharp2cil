using CSharp2CIL.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Microsoft.CodeAnalysis;

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
                var csTypes = tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>();

                var cilTypes = csTypes.Select(type => new CSharp2CIL.Models.Type
                {
                    Name = type.Identifier.ToString(),
                    Lines = new[]{
                        type.GetLocation().GetLineSpan().StartLinePosition.Line,
                        type.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line,
                        type.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line
                    },
                    Methods = type.Members.OfType<MethodDeclarationSyntax>().Select(method => new Method
                    {
                        Name = method.Identifier.ToString(),
                        Lines = new[]{
                            method.GetLocation().GetLineSpan().StartLinePosition.Line,
                            method.Body.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line,
                            method.Body.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line
                        }
                    }).ToList()
                }).ToList();


                var compilation = CSharpCompilation.Create("temp", new[] { tree },
                    new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) }, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                var stream = new MemoryStream();
                var pdbStream = new MemoryStream();
                var result = compilation.Emit(stream, pdbStream);

                if (result.Success)
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolStream = pdbStream };
                    var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, readerParameters);

                    foreach (var cilType in assemblyDefinition.MainModule.Types)
                    {
                        var codeType = cilTypes.FirstOrDefault(t => t.Name == cilType.Name);
                        if (codeType != null)
                        {
                            foreach (var cilMethod in cilType.Methods)
                            {
                                var codeMethod = codeType.Methods.FirstOrDefault(m => m.Name == cilMethod.Name);
                                if (codeMethod != null)
                                {
                                    codeMethod.BodyLines = new List<BodyLine>();
                                    BodyLine bodyLine = null;
                                    foreach (var instruction in cilMethod.Body.Instructions)
                                    {
                                        if (instruction.SequencePoint != null && instruction.SequencePoint.StartLine < 100) //new code line
                                        {
                                            bodyLine = new BodyLine
                                            {
                                                Line = instruction.SequencePoint.StartLine,
                                                Instructions = new List<string>()
                                            };
                                            codeMethod.BodyLines.Add(bodyLine);
                                        }
                                        bodyLine.Instructions.Add(instruction.ToString());
                                    }
                                }
                            }
                        }
                    }
                    return Json(cilTypes);
                }
            }

            return Json("error");
        }
    }
}