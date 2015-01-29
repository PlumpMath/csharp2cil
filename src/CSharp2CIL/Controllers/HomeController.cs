using CSharp2CIL.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

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
			var types = tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>();

			var codeTypes = new List<CilType>();
			foreach (var type in types)
			{
				var type1 = new CilType
				{
					StartLine = type.GetLocation().GetLineSpan().StartLinePosition.Line,
					EndLine = type.GetLocation().GetLineSpan().EndLinePosition.Line,
					Name = type.Identifier.ToString()
				};
				codeTypes.Add(type1);
				var methods = new List<CilMethod>();
				foreach (var method in type.Members.OfType<MethodDeclarationSyntax>())
				{
					var method1 = new CilMethod
					{
						StartLine = method.GetLocation().GetLineSpan().StartLinePosition.Line,
						EndLine = method.GetLocation().GetLineSpan().EndLinePosition.Line,
						Name = method.Identifier.ToString()
					};
					methods.Add(method1);
				}

				type1.CilMethods = methods;
			}



			var compilation = CSharpCompilation.Create("asse", new[] { tree },
				new[] { Microsoft.CodeAnalysis.MetadataReference.CreateFromAssembly(typeof(object).Assembly) });
			var stream = new MemoryStream();
			var pdbStream = new MemoryStream();
			var result = compilation.Emit(stream, pdbStream);
			stream.Seek(0, SeekOrigin.Begin);

			var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolStream = pdbStream };
			var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, readerParameters);

			foreach (var cilType in assemblyDefinition.MainModule.Types)
			{
				var codeType = codeTypes.FirstOrDefault(t => t.Name == cilType.Name);
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

			return Json(codeTypes);
		}
	}
}