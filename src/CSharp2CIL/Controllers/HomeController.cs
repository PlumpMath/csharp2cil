using CSharp2CIL.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CSharp2CIL.Services;
using CSharp2CIL.ViewModels;

namespace CSharp2CIL.Controllers
{
    public class HomeController : Controller
    {
        private readonly CSharpService _cSharpService = new CSharpService();
        private readonly PEService _peService = new PEService();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Parse(string csCode)
        {
            if (_cSharpService.IsValid(csCode))
            {
                List<PEType> peTypes;
                using (Stream peStream = new MemoryStream(), pdbStream = new MemoryStream())
                {
                    _cSharpService.Compile(csCode, peStream, pdbStream);
                    peTypes = _peService.ParseTypes(peStream, pdbStream);
                }

                var csharpTypes = _cSharpService.ParseTypes(csCode);
                var model = csharpTypes.Select(d => new TypeViewModel
                {
                    Name = d.Name,
                    Lines = d.Lines,
                    Methods = d.Methods.Select(m => new Method
                    {
                        Name = m.Name,
                        Lines = m.Lines,
                        BodyLines =
                            peTypes.First(i => i.Name == d.Name)
                                .Methods.First(pem => pem.Name == m.Name)
                                .BodyLines.Select(b => new BodyLine
                                {
                                    Line = b.Line,
                                    Instructions = b.Instructions
                                })
                    })
                });

                return Json(model);
            }

            return Json("error");
        }
    }
}