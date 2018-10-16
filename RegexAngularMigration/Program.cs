using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;

namespace RegexAngularMigration {
	class Program
	{
		static void Main(string[] args)
		{
			GetFile();
		}


		public static  void GetFile()
		{
			string scriptsFolder = ConfigurationManager.AppSettings["ScriptsFolder"];
			string fileNameToRefactor = ConfigurationManager.AppSettings["FileNameToRefactor"];
			string filePathToRefactor = scriptsFolder + fileNameToRefactor + ".js";
			string contents = File.ReadAllText(filePathToRefactor);

			// com grupo 
			//(\$http[\.\[]['""]?\w+['""]?\]?\([^\)]+\)\s*\.\s*)success\s*\(\s*function\s*\(([^\)]*?)\)\s*(\{.+?\})\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\)\s*(\{.+?\})\s*\)\s*\;
			//Regex rgx = new Regex(@"\$http[\.\[]['""]?\w+['""]?\]?\([^\)]+\)\s*\.\s*(success\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})(\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})\s*\)\s*\;", RegexOptions.Singleline);
			Regex rgx = new Regex(@"\$http[\.\[]['""]?\w+['""]?\]?\([^\n]+\)\s*\.\s*(success\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})(\s*\)\s*\.\s*error\s*\(\s*function\s*\(([^\)]*?)\))\s*(\{.+?\})\s*\)\s*\;", RegexOptions.Singleline);

			var matches = rgx.Matches(contents);

			foreach (Match match in matches) {
				string valorAntigo = match.Value;
				string grupoSuccess = string.Empty;
				string grupoConteudoSuccess = string.Empty;
				string[] variaveisSuccess = null;
				string grupoError = string.Empty;
				string grupoConteudoErro = string.Empty;
				string[] variaveisErro = null;

				for (int i =0; i < match.Groups.Count; i++)
				{
					if(i == 1)
					{
						grupoSuccess = match.Groups[i].ToString();
					}
					else if(i == 2)
					{
						variaveisSuccess = match.Groups[i].ToString().Replace(" ", "").Split(',');
					}
					else if(i == 3)
					{
						grupoConteudoSuccess = match.Groups[i].ToString();
					}
					else if( i == 4)
					{
						grupoError = match.Groups[i].ToString();
					}
					else if(i == 5)
					{
						variaveisErro = match.Groups[i].ToString().Replace(" ", "").Split(',');
					}
					else if(i == 6)
					{
						grupoConteudoErro = match.Groups[i].ToString();
					}
					Console.WriteLine("{0}: {1}", i, match.Groups[i]);
				}

				string valorNovo = TratarMetodosCallback(valorAntigo, "then(function onSuccess(responseNovo)", grupoSuccess, variaveisSuccess, grupoConteudoSuccess);
				valorNovo = TratarGrupoConteudo(valorNovo, grupoConteudoSuccess, variaveisSuccess);

				valorNovo = TratarMetodosCallback(valorNovo, ", function onError(responseNovo)", grupoError, variaveisErro, grupoConteudoErro);
				valorNovo = TratarGrupoConteudo(valorNovo, grupoConteudoErro, variaveisErro);

				contents = contents.Replace(valorAntigo, valorNovo);

				//Console.ReadKey();
				Console.WriteLine("\n=====================================================\n");
			}
			string refactorFolder = ConfigurationManager.AppSettings["RefactorFolder"];
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

		public static string TratarGrupoConteudo(string valorAntigo, string grupoConteudo, string[] parametrosExistentes) {

			var grupoConteudoNovo = SubstituiParametrosAntigosPorNovos(parametrosExistentes, grupoConteudo);

			return SubstituiBloco(valorAntigo, grupoConteudoNovo, grupoConteudo);
		}

		private static string SubstituiParametrosAntigosPorNovos(string[] parametrosExistentes, string grupoConteudoNovo) {
			var defaultParams = new string[] { "data", "status", "headers", "config", "statusText", "xhrStatus" };

			for (int i = 0; i < parametrosExistentes.Length; i++) {
				if (parametrosExistentes[i].Length == 0) { break; }
				grupoConteudoNovo = SubstituiParametroAntigoPorNovo(parametrosExistentes[i], defaultParams[i], grupoConteudoNovo);
			}

			return grupoConteudoNovo;
		}

		private static string SubstituiParametroAntigoPorNovo(string parametroFunc, string defaultParam, string grupoConteudoNovo) {
			return Regex.Replace(grupoConteudoNovo, $@"\b{parametroFunc}\b", $"responseNovo.{defaultParam}");
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
			string beyondCompareExePath = ConfigurationManager.AppSettings["BeyondCompareExe"];
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
