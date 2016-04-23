using System.Collections.Generic;
using System.IO;
using CSharp2CIL.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace CSharp2CIL.Services
{
    public class PEService
    {
        public List<PEType> ParseTypes(Stream peStream, Stream pdbStream)
        {
            var readerParameters = new ReaderParameters { ReadSymbols = true, SymbolStream = pdbStream };
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);
            PdbReader rader;

            var types = new List<PEType>();
            foreach (var typeDefinition in assemblyDefinition.MainModule.Types)
            {
                if (typeDefinition.Name != "<Module>")
                {
                    var type = new PEType
                    {
                        Name = typeDefinition.Name,
                        Methods = new List<PEMethod>()

                    };
                    foreach (var methodDefinition in typeDefinition.Methods)
                    {
                        if (methodDefinition.Name != ".ctor")
                        {
                            type.Methods.Add(
                                new PEMethod
                                {
                                    Name = methodDefinition.Name,
                                    BodyLines = ParseInstructions(methodDefinition.Body.Instructions)
                                });
                        }
                    }
                    types.Add(type);
                }
            }

            return types;
        }

        private List<PEBodyLine> ParseInstructions(IEnumerable<Instruction> instructions)
        {
            var lines = new List<PEBodyLine>();
            PEBodyLine line = null;
            foreach (var instruction in instructions)
            {
                if (instruction.SequencePoint != null && instruction.SequencePoint.StartLine < 100) //new code line
                {
                    line = new PEBodyLine
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