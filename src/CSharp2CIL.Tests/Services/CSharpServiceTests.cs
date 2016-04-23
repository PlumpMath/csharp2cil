using System.IO;
using System.Reflection;
using CSharp2CIL.Services;
using NUnit.Framework;

namespace CSharp2CIL.Tests.Services
{
    [TestFixture]
    public class CSharpServiceTests
    {
        private CSharpService _cSharpService;

        [SetUp]
        public void Init()
        {
            _cSharpService = new CSharpService();
        }

        [Test]
        public void IsValid_Valid_Test()
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

            var isValid = _cSharpService.IsValid(code);

            Assert.AreEqual(true, isValid);
        }

        [Test]
        public void IsValid_InValid_Test()
        {
            var code = @"class A
             {
                void Add()
                {
                
            }";

            var isValid = _cSharpService.IsValid(code);

            Assert.AreEqual(false, isValid);
        }

        [Test]
        public void ParseTypes_Test()
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

            var types = _cSharpService.ParseTypes(code);

            Assert.AreEqual("A", types[0].Name);
            CollectionAssert.AreEqual(new[] { 0, 1, 8 }, types[0].Lines);
            Assert.AreEqual("Add", types[0].Methods[0].Name);
            CollectionAssert.AreEqual(new[] { 2, 3, 7 }, types[0].Methods[0].Lines);
        }

        [Test]
        public void Compile_Test()
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
            using (MemoryStream pe = new MemoryStream(), pdb = new MemoryStream())
            {
                _cSharpService.Compile(code, pe, pdb);

                var assembly = Assembly.Load(pe.ToArray());
                Assert.NotNull(assembly.GetType("A"));
            }

        }
    }
}
