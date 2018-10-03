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

			// com grupo 
			//(\$http[\.\[]['""]?\w+['""]?\]?\([^\)]+\)\s*\.\s*)success\s*\(\s*function\s*\(([^\)]*?)\)\s*(\{.+?\})\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\)\s*(\{.+?\})\s*\)\s*\;
			Regex rgx = new Regex(@"\$http[\.\[]['""]?\w+['""]?\]?\([^\)]+\)\s*\.\s*(success\s*\(\s*function\s*\([^\)]*?)\)\s*(\{.+?\})(\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})\s*\)\s*\;", RegexOptions.Singleline);

			var matches = rgx.Matches(contents);

			foreach (Match m in matches) {
				string valorAntigo = m.Value;
				string grupoSuccess = string.Empty;
				string grupoError = string.Empty;
				string grupoConteudoErro = string.Empty;
				string[] variaveisErro = null;

				for (int i =0; i < m.Groups.Count; i++)
				{
					if(i == 1)
					{
						grupoSuccess = m.Groups[i].ToString();
					}
					if( i == 3)
					{
						grupoError = m.Groups[i].ToString();
					}

					if(i == 4)
					{
						variaveisErro = m.Groups[i].ToString().Split(',');
					}

					if(i == 5)
					{
						grupoConteudoErro = m.Groups[i].ToString();
					}
					Console.WriteLine("{0}: {1}", i, m.Groups[i]);
				}

				string valorNovo = SubstituiBloco(valorAntigo, "then(function(responseNovo", grupoSuccess);
				valorNovo = SubstituiBloco(valorNovo, ",function(responseNovo", grupoError);

				valorNovo = SubstituiBloco(valorNovo, TratarCorpoErro(grupoConteudoErro, variaveisErro), grupoConteudoErro);

				contents = contents.Replace(valorAntigo, valorNovo);

				//Console.ReadKey();
				Console.WriteLine("\n=====================================================\n");
			}

			CriarNovoArquivo(@"C:\temp\", "AdministracaoController2.js", contents);
		}

		public static string SubstituiBloco(string valorAntigo, string valorNovo, string grupo)
		{
			return valorAntigo.Replace(grupo, valorNovo);
		}

		public static string TratarCorpoErro(string grupoConteudoErro, string[] variaveisErro)
		{
			grupoConteudoErro = grupoConteudoErro.Replace("{", "");
			string declaracao = null;
			foreach (var item in variaveisErro)
			{
				declaracao += $"var {item} = responseNovo.{item}; \n";
			}

			return $"{{ \n{declaracao} \n {grupoConteudoErro}";
		}

		public static void CriarNovoArquivo(string caminho, string nomeArquivo, string conteudo)
		{
			string path = Path.Combine(caminho, nomeArquivo);
			File.WriteAllText(path, conteudo);
		}
	}
}
