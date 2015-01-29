using System.Collections.Generic;

namespace CSharp2CIL.Models
{
	public class CilType
	{
		public string Name { get; set; }
		public int StartLine { get; set; }
		public int EndLine { get; set; }
		public List<CilMethod> CilMethods { get; set; }
	}

	public class CilMethod
	{
		public string Name { get; set; }
		public int StartLine { get; set; }
		public int EndLine { get; set; }
		public List<CilLineInstructions> CilLineInsturctions { get; set; }
	}

	public class CilLineInstructions
	{
		public int Line { get; set; }
		public List<string> Instructions { get; set; }
	}
}