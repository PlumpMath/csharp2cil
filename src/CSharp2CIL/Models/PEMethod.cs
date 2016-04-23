using System.Collections.Generic;

namespace CSharp2CIL.Models
{
    public class PEMethod
    {
        public string Name { get; set; }
        public List<PEBodyLine> BodyLines { get; set; }
    }
}