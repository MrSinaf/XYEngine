using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Shader : IAsset
{
	private const string BASE_SHADER =
		"""
		#version 330 core
		layout (location = 0) in vec2 position;
		layout (location = 1) in vec2 uv;
		layout (location = 2) in vec4 color;
		
		uniform mat3 projection;
		uniform mat3 view;
		uniform mat3 model;
		
		
		""";
	
	public static readonly ShaderConfig internalConfig = new (_ => { }, (_, _, _) => { });
	public static ShaderConfig defaultConfig = internalConfig;
	
	internal static readonly List<Shader> shaders = [];
	
	public GProgram gProgram { get; private set; }
	public ShaderConfig config { get; private set; }
	
	void IAsset.Load(AssetProperty property)
	{
		if (property.extension != ".shadxy")
			throw new FormatException("Invalid file format. Expected '.shadxy'.");
		
		config = property.config as ShaderConfig ?? defaultConfig;
		
		using var reader = new StreamReader(property.stream);
		var sections = reader.ReadToEnd().Split("#type ", StringSplitOptions.RemoveEmptyEntries);
		
		string vertexShader = null;
		string fragmentShader = null;
		
		foreach (var section in sections)
		{
			if (section.StartsWith("vertex", StringComparison.CurrentCultureIgnoreCase))
				vertexShader = BASE_SHADER + section[section.IndexOf('\n')..].Trim();
			
			if (section.StartsWith("fragment", StringComparison.CurrentCultureIgnoreCase))
				fragmentShader = "#version 330 core\n" + section[section.IndexOf('\n')..].Trim();
		}
		
		if (vertexShader == null || fragmentShader == null)
			throw new Exception("Impossible de continuer");
		
		gProgram = new GProgram(vertexShader, fragmentShader);
		gProgram.Use();
		shaders.Add(this);
	}
	
	void IAsset.UnLoad()
	{
		gProgram?.Dispose();
		gProgram = null;
		
		shaders.Remove(this);
	}
	
	public static Shader GetDefault() => AssetManager.GetEmbeddedAsset<Shader>("shaders.default.shadxy");
	public static Shader GetDefaultUI() => AssetManager.GetEmbeddedAsset<Shader>("shaders.ui.shadxy");
}

public record class ShaderConfig(Action<Material> onCreate = null, Action<Material, string, object> onPropertyAdd = null) : IAssetConfig;