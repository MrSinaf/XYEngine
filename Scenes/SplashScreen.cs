using XYEngine.Resources;
using XYEngine.UI;
using XYEngine.UI.Widgets;

namespace XYEngine.Scenes;

internal class SplashScreen : Scene
{
	internal static Type startScene { private get; set; }
	internal static Func<Task>[] loadingTasks;
	
	private MaterialUI backMaterial;
	private Texture2D xyTexture;
	
	private bool loaded;
	private ProgressBar progressBar;
	private Label errorLabel;
	
	private float targetValue;
	
	protected override void Start()
	{
		Graphics.gl.ClearColor(0, 0, 0, 0);
		
		LoadDefaultAssets();
		
		var windowSize = GameWindow.size.ToVector2();
		var backShader = AssetManager.LoadEmbeddedAsset<Shader>("shaders.splash_background.shadxy");
		var backTexture = AssetManager.LoadEmbeddedAsset<Texture2D>("textures.spattern.png", Texture2D.internalConfig);
		
		xyTexture = AssetManager.LoadEmbeddedAsset<Texture2D>("textures.xyengine.png", Texture2D.internalConfig);
		backMaterial = new MaterialUI(backShader, ("repeat", windowSize / backTexture.size), ("isBackground", true)).SetTexture(backTexture);
		
		progressBar = new ProgressBar(0, maxValue: loadingTasks.Length)
		{
			position = new Vector2Int(0, 10), anchorMin = Vector2.zero, anchorMax = Vector2.right, size = new Vector2Int(0, 5), margin = new RegionInt(10), tint = Color.black,
			visible = loadingTasks.Length > 0
		};
		errorLabel = new Label
		{
			position = new Vector2Int(5, -5), pivotAndAnchors = new Vector2(0, 1), tint = Color.red
		};
		
		Task.Run(async () =>
		{
			XY.InternalLog("SPLASHSCREEN", $"Starting loading with {progressBar.maxValue} step(s).");
			
			var isFaulted = false;
			foreach (var loadingTask in loadingTasks)
			{
				await loadingTask.Invoke().ContinueWith(task =>
				{
					targetValue++;
					if (task.IsFaulted)
					{
						isFaulted = true;
						XY.InternalLog("SPLASHSCREEN",
									   $"Unable to continue, an error occurred at step {targetValue}: {task.Exception?.InnerException?.Message ?? task.Exception?.Message}",
									   TypeLog.Error);
						errorLabel.text = $"A problem has occurred. [{targetValue}/{progressBar.maxValue} step]";
					}
				});
				
				if (isFaulted)
					throw new Exception("Loading task failed");
			}
			
			await Task.Delay(1000);
		}).ContinueWith(task =>
		{
			if (task.IsCompletedSuccessfully)
			{
				loaded = true;
				XY.InternalLog("SPLASHSCREEN", "Loading completed successfully!", TypeLog.Info);
			}
			
			return loaded = task.IsCompletedSuccessfully;
		});
	}
	
	protected override void BuildUI(RootElement root)
	{
		root.AddChild(new Image(backMaterial) { anchorMax = Vector2.one });
		root.AddChild(new Image(xyTexture) { pivotAndAnchors = new Vector2(0.5F), scale = new Vector2(15) });
		root.AddChild(new Label($"v{XY.version} [{XY.VERSION_STATE}]") { position = new Vector2Int(20, 15), tint = Color.green });
		root.AddChild(progressBar);
		root.AddChild(errorLabel);
	}
	
	protected override void Update()
	{
		progressBar.value = float.Lerp(progressBar.value, targetValue, Time.delta * 10);
		
		if (loaded)
			SetStartScene();
	}
	
	private static void LoadDefaultAssets()
	{
		AssetManager.LoadEmbeddedAsset<Shader>("shaders.default.shadxy", Shader.internalConfig);
		
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
	
	protected override void Destroy()
	{
		AssetManager.UnLoadEmbeddedAsset("shaders.splash_background.shadxy");
		loadingTasks = null;
	}
}