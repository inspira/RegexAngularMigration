using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;

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
			string scriptsFolder = ConfigurationSettings.AppSettings.Get("ScriptsFolder");
			string fileNameToRefactor = ConfigurationSettings.AppSettings.Get("FileNameToRefactor");
			string filePathToRefactor = scriptsFolder + fileNameToRefactor + ".js";
			string contents = File.ReadAllText(filePathToRefactor);

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
				valorNovo = SubstituiBloco(valorNovo, ",function(responseNovo)", grupoError);

				valorNovo = SubstituiBloco(valorNovo, TratarCorpoErro(grupoConteudoErro, variaveisErro), grupoConteudoErro);

				contents = contents.Replace(valorAntigo, valorNovo);

				//Console.ReadKey();
				Console.WriteLine("\n=====================================================\n");
			}
			string refactorFolder = ConfigurationSettings.AppSettings.Get("RefactorFolder");
			string refactoredFile = fileNameToRefactor + "Refatorado.js";

			CriarNovoArquivo(refactorFolder, refactoredFile, contents);
			AbrirBeyondCompare(filePathToRefactor, refactorFolder + refactoredFile);
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
				var index = Array.IndexOf(variaveisErro, item);
				if(index == 0)
				{
					declaracao += $"var {item} = responseNovo.data; \n";
				}
				else
				{
					declaracao += $"var {item} = responseNovo.{item.TrimStart(' ')}; \n";
				}
				
			}

			return $"{{ \n{declaracao} \n {grupoConteudoErro}";
		}

		public static void CriarNovoArquivo(string caminho, string nomeArquivo, string conteudo)
		{
			if (!Directory.Exists(caminho)) {
				Directory.CreateDirectory(caminho);
			}
			string path = Path.Combine(caminho, nomeArquivo);
			File.WriteAllText(path, conteudo);
		}

		public static void AbrirBeyondCompare(string arquivoOriginal, string arquivoRefatorado) {
			string beyondCompareExePath = @"C:\Program Files\Beyond Compare 4\BCompare.exe";
			var commandArgument = arquivoOriginal + " " + arquivoRefatorado;
			Process.Start(beyondCompareExePath, commandArgument);
		}
	}
}
