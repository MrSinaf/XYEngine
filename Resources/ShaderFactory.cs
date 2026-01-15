using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XYEngine.Resources;

public static partial class ShaderFactory
{
	public static Result Build(string shadxySource)
	{
		var expanded = PreprocessIncludes(shadxySource);
		var preamble = ExtractPreambleDedup(expanded);
		var functions = ExtractFunctions(expanded);
		
		var vertexSb = new StringBuilder();
		var fragmentSb = new StringBuilder();
		
		vertexSb.Append('\n').Append(preamble).Append('\n');
		fragmentSb.Append('\n').Append(preamble).Append('\n');
		
		var vertexParams = new StringBuilder();
		var fragmentParams = new StringBuilder();
		
		foreach (var f in functions)
		{
			switch (f.Name)
			{
				case "mainVertex":
				{
					vertexParams.Insert(0, f.GlobalParams);
					var method = f.Body.Replace(f.Name, "main");
					vertexSb.Append(method).Append('\n');
					break;
				}
				case "mainFragment":
				{
					fragmentParams.Insert(0, f.GlobalParams);
					var method = f.Body.Replace(f.Name, "main");
					fragmentSb.Append(method).Append('\n');
					break;
				}
				default:
					switch (f.Stage)
					{
						case Stage.VertexOnly:
							vertexSb.Append(f.Body).Append('\n');
							break;
						case Stage.FragmentOnly:
							fragmentSb.Append(f.Body).Append('\n');
							break;
						case Stage.Both:
							vertexSb.Append(f.Body).Append('\n');
							fragmentSb.Append(f.Body).Append('\n');
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					break;
			}
		}
		
		vertexSb.Insert(0, vertexParams.ToString());
		fragmentSb.Insert(0, fragmentParams.ToString());
		
		return new Result(vertexSb.ToString(), fragmentSb.ToString());
	}
	
	private enum Stage { Both, VertexOnly, FragmentOnly }
	
	private sealed record FunctionInfo(string Name, string Body, string GlobalParams, Stage Stage);
	
	private static List<FunctionInfo> ExtractFunctions(string src)
	{
		var list = new List<FunctionInfo>();
		var matches = FunctionHeaderRegex().Matches(src);
		foreach (Match m in matches)
		{
			var startIndex = m.Index;
			var functionName = m.Groups[1].Value;
			var parametersText = m.Groups[2].Value.Trim();
			
			var endIndex = startIndex + m.Length;
			for (var count = 1; endIndex < src.Length; endIndex++)
			{
				var c = src[endIndex];
				if (c == '{') count++;
				else if (c == '}' && --count == 0) break;
			}
			if (endIndex <= startIndex) continue;
			
			var stage = Stage.Both;
			{
				var prevLineStart = src.LastIndexOf('\n', Math.Max(0, startIndex - 1));
				if (prevLineStart >= 0)
				{
					var prevPrevLineStart = src.LastIndexOf('\n', Math.Max(0, prevLineStart - 1));
					var prevLine = src.Substring(prevLineStart + 1, startIndex - (prevLineStart + 1));
					var prevPrevLine = prevPrevLineStart >= 0
						? src.Substring(prevPrevLineStart + 1, prevLineStart - (prevPrevLineStart + 1))
						: string.Empty;
					if (prevLine.Contains("@fragment-only") || prevPrevLine.Contains("@fragment-only"))
						stage = Stage.FragmentOnly;
					else if (prevLine.Contains("@vertex-only") || prevPrevLine.Contains("@vertex-only"))
						stage = Stage.VertexOnly;
				}
			}
			
			var globalParams = string.Empty;
			string body;
			if (!string.IsNullOrWhiteSpace(parametersText))
			{
				var parameters = parametersText.Split(',');
				var formattedParamsList = new List<string>();
				var cleanParamsList = new List<string>();
				foreach (var p in parameters)
				{
					var trimmed = p.Trim();
					if (trimmed.StartsWith("in ") || trimmed.StartsWith("out "))
						formattedParamsList.Add(trimmed + ";\n");
					else
						cleanParamsList.Add(trimmed);
				}
				globalParams = string.Join(string.Empty, formattedParamsList);
				var originalSig = $"{functionName}({parametersText})";
				var cleanSig = $"{functionName}({string.Join(", ", cleanParamsList)})";
				body = src.Substring(startIndex, endIndex - startIndex + 1).Replace(originalSig, cleanSig);
			}
			else
				body = src.Substring(startIndex, endIndex - startIndex + 1);
			
			list.Add(new FunctionInfo(functionName, body, globalParams, stage));
		}
		return list;
	}
	
	private static string ExtractPreambleDedup(string src)
	{
		var layouts = new HashSet<string>();
		var uniforms = new HashSet<string>();
		
		foreach (Match m in LayoutRegex().Matches(src))
			layouts.Add(m.Value.Trim());
		
		foreach (Match m in UniformRegex().Matches(src))
			uniforms.Add(m.Value.Trim());
		
		var sb = new StringBuilder();
		foreach (var l in layouts) sb.AppendLine(l);
		if (layouts.Count > 0) sb.AppendLine();
		foreach (var u in uniforms) sb.AppendLine(u);
		return sb.ToString();
	}
	
	private static string PreprocessIncludes(string src)
	{
		var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		return IncludeRegex().Replace(src, ReplaceInclude);
		
		string Resolver(string inc)
		{
			var key = inc.Replace('/', '\\');
			if (!visited.Add(key)) return string.Empty;
			
			var tryPaths = new List<string>();
			var appBase = AppContext.BaseDirectory;
			tryPaths.Add(Path.Combine(appBase, "assets", inc));
			tryPaths.Add(inc);
			
			foreach (var p in tryPaths)
			{
				try
				{
					if (File.Exists(p))
						return File.ReadAllText(p);
				}
				catch
				{
					// ignore
				}
			}
			
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resName = "XYEngine.assets." + inc.Replace('/', '.').Replace('\\', '.');
				using var stream = assembly.GetManifestResourceStream(resName);
				if (stream != null)
					using (var reader = new StreamReader(stream))
					{
						return reader.ReadToEnd();
					}
			}
			catch
			{
				// ignore
			}
			
			// Introuvable -> laisser la directive en commentaire pour debug
			return $"\n/* Failed to include '{inc}' */\n";
		}
		
		string ReplaceInclude(Match m)
		{
			var content = Resolver(m.Groups[1].Value.Trim());
			return string.IsNullOrEmpty(content) ? string.Empty : PreprocessIncludes(content);
		}
	}
	
	public sealed class Result(string v, string f)
	{
		public readonly string vertexGLSL = v;
		public readonly string fragmentGLSL = f;
	}
	
	public sealed class ShaderBuildOptions
	{
		public PipelineHints pipeline;
	}
	
	public sealed class PipelineHints
	{
		public bool supportsShadowMap { get; init; }
		public bool supportsPostProcess { get; init; }
	}
	
	[GeneratedRegex("(?:void|bool|int|uint|float|double|vec[234]|ivec[234]|uvec[234]|dvec[234]|mat[234](?:x[234])" +
					@"?)\s+(\w+)\s*\(([^\)]*)\)\s*{", RegexOptions.CultureInvariant)]
	private static partial Regex FunctionHeaderRegex();
	
	[GeneratedRegex(@"^\s*#include\s+""([^""]+)""\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
	private static partial Regex IncludeRegex();
	
	[GeneratedRegex(@"layout\s*\(.*?\).*?;")]
	private static partial Regex LayoutRegex();
	
	[GeneratedRegex(@"uniform\s+.*?;")]
	private static partial Regex UniformRegex();
}