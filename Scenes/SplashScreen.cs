using XYEngine.Resources;
using XYEngine.UI.Widgets;

namespace XYEngine.Scenes;

internal class SplashScreen : Scene
{
	internal static Type startScene { private get; set; }
	internal static Action action;
	
	protected override void Start()
	{
		Graphics.gl.ClearColor(0, 0, 0, 0);
		
		LoadDefaultAssets();
		
		var windowSize = GameWindow.size.ToVector2();
		var backShader = AssetManager.LoadEmbeddedAsset<Shader>("shaders.splash_background.shadxy");
		var backTexture = AssetManager.LoadEmbeddedAsset<Texture2D>("textures.spattern.png", Texture2D.internalConfig);
		var xyTexture = AssetManager.LoadEmbeddedAsset<Texture2D>("textures.xyengine.png", Texture2D.internalConfig);
		
		var backMaterial = new Material(backShader, ("repeat", windowSize / backTexture.size), ("isBackground", true), ("mainTex", backTexture));
		
		canvas.root.AddChild(new Image(backMaterial) { anchorMax = Vector2.one });
		canvas.root.AddChild(new Image(xyTexture) { pivotAndAnchors = new Vector2(0.5F), scale = new Vector2(15) });
		canvas.root.AddChild(new Label($"v{XY.version} [{XY.VERSION_STATE}]") { position = new Vector2Int(10), tint = Color.green });
		
		action?.Invoke();
	}
	
	protected override void Update()
	{
		if (Time.total > 5)
			SetStartScene();
	}
	
	private static void LoadDefaultAssets()
	{
		AssetManager.LoadEmbeddedAsset<Texture2D>("textures.xy.png", Texture2D.internalConfig);
		AssetManager.LoadEmbeddedAsset<Shader>("shaders.default.shadxy", Shader.internalConfig);
		AssetManager.LoadEmbeddedAsset<Texture2D>("textures.white_pixel.png", Texture2D.internalConfig);
		
		var font = AssetManager.LoadEmbeddedAsset<Font>("fonts.jetbrains.ttf");
		font.GenerateBitmap(16, 256);
		AssetManager.LoadEmbeddedAsset<Shader>("shaders.ui.shadxy", new ShaderConfig(material =>
		{
			material.SetProperty("uvRegion", new Region(Vector2.zero, Vector2.one));
			material.SetProperty("padding", new Region(Vector2.zero));
			material.SetProperty("paddingScale", 1F);
		}, (material, name, obj) =>
		{
			if (name == "mainTex" && obj is Texture2D texture)
				material.SetProperty("mainTexSize", texture.size.ToVector2());
		}));
	}
	
	private static void SetStartScene()
	{
		Graphics.SetBackgroundColor(Color.black);
		GameWindow.SetDisplayMode(DisplayMode.NoBorder);
		
		AssetManager.UnLoadEmbeddedAsset("textures.xyengine.png");
		AssetManager.UnLoadEmbeddedAsset("textures.spattern.png");
		AssetManager.UnLoadEmbeddedAsset("shaders.splash_background.shadxy");
		
		SceneManager.SetCurrentScene(startScene);
	}
	
	protected override void Destroy() => AssetManager.UnLoadEmbeddedAsset("shaders.splash_background.shadxy");
}