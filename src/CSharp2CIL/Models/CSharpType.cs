using System.Collections.Generic;

namespace CSharp2CIL.Models
{
    public class CSharpType
    {
        public string Name { get; set; }
        public int[] Lines { get; set; }
        public List<CSharpMethod> Methods { get; set; }
    }
}