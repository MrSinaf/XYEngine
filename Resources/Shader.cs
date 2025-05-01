using System.Text;
using System.Text.RegularExpressions;
using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Shader : IAsset
{
	public static readonly ShaderConfig internalConfig = new (delegate { }, delegate { });
	public static ShaderConfig defaultConfig = internalConfig;
	
	internal static readonly List<Shader> shaders = [];
	
	private const string OPENGL_VERSION = "#version 330 core";
	
	public GProgram gProgram;
	public ShaderConfig config { get; private set; }
	
	void IAsset.Load(AssetProperty property)
	{
		if (property.extension != ".shadxy")
			throw new FormatException("Invalid file format. Expected '.shadxy'.");
		
		config = property.config as ShaderConfig ?? defaultConfig;
		
		using var reader = new StreamReader(property.stream);
		var shadxy = reader.ReadToEnd();
		
		var preamble = ExtractPreamble(shadxy);
		var functions = ExtractAllFunctions(shadxy);
		
		var vertexShader = $"\n{preamble}\n";
		var fragmentShader = $"\n{preamble}\n";
		
		foreach (var (name, (method, param)) in functions)
		{
			if (name == "mainVertex")
			{
				vertexShader = param + vertexShader;
				vertexShader += method.Replace(name, "main") + "\n";
			}
			else if (name == "mainFragment")
			{
				fragmentShader = param + fragmentShader;
				fragmentShader += method.Replace(name, "main") + "\n";
			}
			else
			{
				vertexShader += method + "\n";
				fragmentShader += method + "\n";
			}
		}
		
		gProgram?.Dispose();
		gProgram = new GProgram();
		gProgram.Compile(OPENGL_VERSION + "\n" + vertexShader, OPENGL_VERSION + "\n" + fragmentShader);
		
		if (!property.onHotReload)
			shaders.Add(this);
	}
	
	private static string ExtractPreamble(string shaderContent)
	{
		var sb = new StringBuilder();
		
		var versionMatch = Regex.Match(shaderContent, "#version.*");
		if (versionMatch.Success)
			sb.AppendLine(versionMatch.Value);
		
		var layoutMatches = Regex.Matches(shaderContent, @"layout\s*\(.*?\).*?;");
		foreach (Match match in layoutMatches)
			sb.AppendLine(match.Value);
		
		if (layoutMatches.Count > 0)
			sb.AppendLine();
		
		var uniformMatches = Regex.Matches(shaderContent, @"uniform\s+.*?;");
		foreach (Match match in uniformMatches)
			sb.AppendLine(match.Value);
		
		return sb.ToString();
	}
	
	private static Dictionary<string, (string, string)> ExtractAllFunctions(string shaderContent)
	{
		var functions = new Dictionary<string, (string, string)>();
		var matches = Regex.Matches(shaderContent, @"(?:void|vec2|vec3|vec4|float|int)\s+(\w+)\s*\(([^\)]*)\)\s*{");
		
		foreach (Match match in matches)
		{
			var startIndex = match.Index;
			var functionName = match.Groups[1].Value;
			var parametersText = match.Groups[2].Value.Trim();
			
			var count = 1;
			var endIndex = startIndex + match.Length;
			for (; endIndex < shaderContent.Length; endIndex++)
			{
				if (shaderContent[endIndex] == '{')
					count++;
				else if (shaderContent[endIndex] == '}')
				{
					count--;
					if (count == 0)
						break;
				}
			}
			
			if (endIndex > startIndex)
			{
				if (!string.IsNullOrWhiteSpace(parametersText))
				{
					var parameters = parametersText.Split(',');
					var formattedParamsList = new List<string>();
					var cleanParamsList = new List<string>();
					
					foreach (var param in parameters)
					{
						var trimmedParam = param.Trim();
						if (trimmedParam.StartsWith("in ") || trimmedParam.StartsWith("out "))
							formattedParamsList.Add(trimmedParam + ";");
						else
							cleanParamsList.Add(trimmedParam);
					}
					
					functions[functionName] = (shaderContent.Substring(startIndex, endIndex - startIndex + 1)
															.Replace($"{functionName}({parametersText})", $"{functionName}({string.Join(", ", cleanParamsList)})"),
											   string.Join("\n", formattedParamsList));
				}
				else
					functions[functionName] = (shaderContent.Substring(startIndex, endIndex - startIndex + 1), "");
			}
		}
		
		return functions;
	}
	
	void IAsset.UnLoad()
	{
		gProgram.Dispose();
		shaders.Remove(this);
	}
	
	public static Shader GetDefault() => AssetManager.GetEmbeddedAsset<Shader>("shaders.default.shadxy");
	public static Shader GetDefaultUI() => AssetManager.GetEmbeddedAsset<Shader>("shaders.ui.shadxy");
}

public record class ShaderConfig(Action<Material> onCreate = null, Action<Material, string, object> onPropertyAdd = null) : IAssetConfig;