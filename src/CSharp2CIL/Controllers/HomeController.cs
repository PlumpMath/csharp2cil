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
            var csTypes = tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>();

            var cilTypes = new List<CilType>();
            foreach (var type in csTypes)
            {
                var cilType = new CilType
                {
                    StartLine = type.GetLocation().GetLineSpan().StartLinePosition.Line,
                    EndLine = type.GetLocation().GetLineSpan().EndLinePosition.Line,
                    Name = type.Identifier.ToString()
                };
                cilTypes.Add(cilType);
                var methods = type.Members.OfType<MethodDeclarationSyntax>().Select(method => new CilMethod
                {
                    Name = method.Identifier.ToString(),
                    StartLine = method.GetLocation().GetLineSpan().StartLinePosition.Line,
                    EndLine = method.GetLocation().GetLineSpan().EndLinePosition.Line,
                });

                cilType.CilMethods = methods.ToList();
            }

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
                            var codeMethod = codeType.CilMethods.FirstOrDefault(m => m.Name == cilMethod.Name);
                            if (codeMethod != null)
                            {
                                codeMethod.CilLineInsturctions = new List<CilLineInstructions>();
                                CilLineInstructions bodyBlock = null;
                                foreach (var instruction in cilMethod.Body.Instructions)
                                {
                                    if (instruction.ToString().Substring(instruction.ToString().Length - 3) == "nop") continue;
                                    if (instruction.SequencePoint != null && instruction.SequencePoint.StartLine < 100)
                                    {
                                        bodyBlock = new CilLineInstructions
                                        {
                                            Line = instruction.SequencePoint.StartLine,
                                            Instructions = new List<string>()
                                        };
                                        codeMethod.CilLineInsturctions.Add(bodyBlock);
                                    }
                                    bodyBlock.Instructions.Add(instruction.ToString());
                                }
                            }
                        }
                    }
                }
                return Json(cilTypes);
            }

            return Json("error");
        }
    }
}