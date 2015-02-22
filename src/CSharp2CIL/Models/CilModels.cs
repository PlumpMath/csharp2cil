using System.Collections.Generic;

namespace CSharp2CIL.Models
{
	public class Type
	{
		public string Name { get; set; }
        public int[] Lines { get; set; }
		public List<Method> Methods { get; set; }
	}

	public class Method
	{
		public string Name { get; set; }
        public int[] Lines { get; set; }
        public List<BodyLine> BodyLines { get; set; }
	}

	public class BodyLine
	{
		public int Line { get; set; }
		public List<string> Instructions { get; set; }
	}
}