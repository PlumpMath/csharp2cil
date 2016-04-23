using System.Collections.Generic;

namespace CSharp2CIL.ViewModels
{
    public class TypeViewModel
    {
        public string Name { get; set; }
        public int[] Lines { get; set; }
        public IEnumerable<Method> Methods { get; set; }
    }

    public class Method
    {
        public string Name { get; set; }
        public int[] Lines { get; set; }
        public IEnumerable<BodyLine> BodyLines { get; set; }
    }

    public class BodyLine
    {
        public int Line { get; set; }
        public IEnumerable<string> Instructions { get; set; }
    }
}