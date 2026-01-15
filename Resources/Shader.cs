using XYEngine.Debugs;
using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Shader : IResource, IDebugProperty
{
	public static readonly ShaderConfig internalConfig = new (delegate { }, delegate { });
	public static ShaderConfig defaultConfig = internalConfig;
	
	internal static readonly List<Shader> shaders = [];
	
	private const string OPENGL_VERSION = "#version 330 core";
	
	public GProgram gProgram;
	public string assetPath { get; set; }
	public ShaderConfig config { get; private set; }
	
	public Shader() { }
	
	public Shader(string vertexShader, string fragmentShader)
	{
		MainThreadQueue.EnqueueRenderer(() =>
		{
			gProgram = new GProgram();
			gProgram.Compile(OPENGL_VERSION + "\n" + vertexShader, OPENGL_VERSION + "\n" + fragmentShader);
		});
		shaders.Add(this);
	}
	
	void IResource.Load(Resource ressource)
	{
		if (ressource.extension != ".shadxy")
			throw new FormatException("Invalid file format. Expected '.shadxy'.");
		
		config = ressource.config as ShaderConfig ?? defaultConfig;
		
		using var reader = new StreamReader(ressource.stream);
		var shadxy = reader.ReadToEnd();
		
		var built = ShaderFactory.Build(shadxy);
		var vertexShader = built.vertexGLSL;
		var fragmentShader = built.fragmentGLSL;
		
		MainThreadQueue.EnqueueRenderer(() =>
		{
			gProgram?.Dispose();
			gProgram = new GProgram();
			gProgram.Compile(OPENGL_VERSION + "\n" + vertexShader, OPENGL_VERSION + "\n" + fragmentShader);
		});
		
		if (!ressource.onHotReload)
			shaders.Add(this);
	}
	
	void IAsset.Destroy()
	{
		gProgram.Dispose();
		shaders.Remove(this);
	}
	
	void IDebugProperty.OnDebugPropertyRender()
	{
		XYDebug.ShowValue("onCreateAction", config.onCreate != null);
		XYDebug.ShowValue("onPropertyAddAction", config.onPropertyAdd != null);
		ImGui.Dummy(new Vector2(0, 10));
		
		var uniforms = gProgram.GetUniforms();
		foreach (var uniform in uniforms)
		{
			// TODO > Maybe ajouter la possibilité de mettre à jour les données ?!
			if (!XYDebug.IsCompatibleInput(uniform.type) || uniform.arraySize == 0)
				ImGui.TextColored(new Vector4(0.1F, 0.5F, 0.1F, 1), uniform.name);
		}
	}
	
	public static Shader GetDefault() => Vault.GetEmbeddedAsset<Shader>("default.shader");
	public static Shader GetDefaultUI() => Vault.GetEmbeddedAsset<Shader>("ui.shader");
}

public record class ShaderConfig(Action<Material> onCreate, Action<Material, string, object> onPropertyAdd)
	: IResourceConfig;