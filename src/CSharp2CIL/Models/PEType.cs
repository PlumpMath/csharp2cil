using System.Collections.Generic;

namespace CSharp2CIL.Models
{
    public class PEType
    {
        public string Name { get; set; }
        public List<PEMethod> Methods { get; set; }
    }
}