using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegexAngularMigration
{
	class Program
	{
		static void Main(string[] args)
		{
			GetFile();
		}


		public static  void GetFile()
		{
			string contents = File.ReadAllText(@"C:\temp\AdministracaoController.js");
			Regex rgx = new Regex(@"(\$http[\.\[]['""]?\w+['""]?\]?\([^\)]+\)\s*\.\s*)success\s*\(\s*function\s*\(([^\)]*?)\)\s*(\{.+?\})\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\)\s*(\{.+?\})\s*\)\s*\;"
, RegexOptions.Singleline);

			var matches = rgx.Matches(contents);
			foreach (Match m in matches) {
//				Console.WriteLine(m.Value);
				for (int i =0; i < m.Groups.Count; i++)
				{
					Console.WriteLine("{0}: {1}", i, m.Groups[i]);
				}
				Console.ReadKey();
				Console.WriteLine("\n=====================================================\n");
			}
		}
	}
}
