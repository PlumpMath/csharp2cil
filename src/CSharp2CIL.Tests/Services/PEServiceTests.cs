using System.IO;
using CSharp2CIL.Services;
using NUnit.Framework;

namespace CSharp2CIL.Tests.Services
{
    [TestFixture]
    public class PEServiceTests
    {
        private PEService _peService;

        [SetUp]
        public void Init()
        {
            _peService = new PEService();
        }

        [Test]
        public void Parse_Test()
        {
            var code = @"class A
             {
                void Add()
                {
                    int a = 1;
                    int b = 2;
                    int c = a + b;
                }
            }";
            var pe = new MemoryStream();
            var pdb = new MemoryStream();
            new CSharpService().Compile(code, pe, pdb);

            var types = _peService.ParseTypes(pe, pdb);

            Assert.AreEqual("A", types[0].Name);
            Assert.AreEqual("Add", types[0].Methods[0].Name);
            Assert.AreEqual(4, types[0].Methods[0].BodyLines[0].Line);
            Assert.AreEqual("IL_0000: nop", types[0].Methods[0].BodyLines[0].Instructions[0]);
        }
    }
}
