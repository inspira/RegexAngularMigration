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
			Regex rgx = new Regex(@"\$http[\.\[]['""]?\w+['""]?\]?\([^\)]+\)\s*\.\s*(success\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})(\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})\s*\)\s*\;", RegexOptions.Singleline);

			var matches = rgx.Matches(contents);

			foreach (Match m in matches) {
				string valorAntigo = m.Value;
				string grupoSuccess = string.Empty;
				string grupoConteudoSuccess = string.Empty;
				string[] variaveisSuccess = null;
				string grupoError = string.Empty;
				string grupoConteudoErro = string.Empty;
				string[] variaveisErro = null;

				for (int i =0; i < m.Groups.Count; i++)
				{
					if(i == 1)
					{
						grupoSuccess = m.Groups[i].ToString();
					}
					if(i == 2)
					{
						variaveisSuccess = m.Groups[i].ToString().Replace(" ", "").Split(',');
					}
					if(i == 3)
					{
						grupoConteudoSuccess = m.Groups[i].ToString();
					}
					if( i == 4)
					{
						grupoError = m.Groups[i].ToString();
					}

					if(i == 5)
					{
						variaveisErro = m.Groups[i].ToString().Replace(" ", "").Split(',');
					}

					if(i == 6)
					{
						grupoConteudoErro = m.Groups[i].ToString();
					}
					Console.WriteLine("{0}: {1}", i, m.Groups[i]);
				}

				string valorNovo = TratarMetodosCallback(valorAntigo, "then(function(responseNovo)", grupoSuccess, variaveisSuccess, grupoConteudoSuccess);
				valorNovo = TratarGrupoConteudo(valorNovo, grupoConteudoSuccess, variaveisSuccess);

				valorNovo = TratarMetodosCallback(valorNovo, ", function(responseNovo)", grupoError, variaveisErro, grupoConteudoErro);
				valorNovo = TratarGrupoConteudo(valorNovo, grupoConteudoErro, variaveisErro);

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

		public static string TratarMetodosCallback(string valorAntigo, string conteudoNovo, string grupoCallback, string[] parametrosFunc, string grupoConteudo)
		{
			if (!ValidaParametrosCallback(parametrosFunc, grupoConteudo)) {
				conteudoNovo = conteudoNovo.Replace("responseNovo", "");
			}
			return SubstituiBloco(valorAntigo, conteudoNovo, grupoCallback);
		}

		public static string TratarGrupoConteudo(string valorAntigo, string grupoConteudo, string[] parametrosFunc) {

			var funcParams = new string[] { "data", "status", "headers", "config" };
			string grupoConteudoNovo = grupoConteudo;
				for (int i = 0; i < parametrosFunc.Length; i++) {
				if(parametrosFunc[i].Length == 0) { break; }
				grupoConteudoNovo = grupoConteudoNovo.Replace(parametrosFunc[i], $"responseNovo.{funcParams[i]}");
			}
			return SubstituiBloco(valorAntigo, grupoConteudoNovo, grupoConteudo);
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

		public static bool ValidaParametrosCallback (string[] parametros, string grupoConteudo) {
			if (parametros == null) {
				return false;
			}
			var qtdOcorrenciasdeParamsNoConteudo = 0;
			foreach (var p in parametros) {
				if (p.Length != 0 && grupoConteudo.Contains(p)) {
					qtdOcorrenciasdeParamsNoConteudo++;
				}
			}
			return qtdOcorrenciasdeParamsNoConteudo > 0;
		}
	}
}
