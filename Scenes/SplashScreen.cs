using XYEngine.Animations;
using XYEngine.Debugs;
using XYEngine.Resources;
using XYEngine.UI;
using XYEngine.UI.Widgets;

namespace XYEngine.Scenes;

internal class SplashScreen : Scene
{
	internal static Type startScene { private get; set; }
	internal static Func<IProgress<float>, Task> loadingTask;
	
	private Animator<Image> logoAnimator;
	private MaterialUI backMaterial;
	private Texture2D xyTexture;
	private Image logo;
	
	private bool loaded;
	private ProgressBar progressBar;
	private Label errorLabel;
	
	private float targetValue;
	
	protected override void Start()
	{
		Graphics.gl.ClearColor(0, 0, 0, 0);
		
		LoadDefaultAssets();
		
		var windowSize = GameWindow.size.ToVector2();
		var backShader = Vault.LoadEmbeddedResource<Shader>("splash.shader", "shaders.splash_background.shadxy");
		var backTexture = Vault.LoadEmbeddedResource<Texture2D>("splash.texture", "textures.spattern.png", 
																Texture2D.internalConfig);
		
		xyTexture = Vault.LoadEmbeddedResource<Texture2D>("xyengine.gif", "textures.xyengine_animate.png",
														  Texture2D.internalConfig);
		logo = new Image(xyTexture)
		{
			pivotAndAnchors = new Vector2(0.5F, 0), size = new Vector2Int(22, 21), scale = new Vector2(15),
			position = new Vector2Int(0, 13)
		};
		logo.SetUV(new RectInt(0, 0, 22, 21));
		
		var logoAnimController = new AnimationController<Image>();
		var startAnim = new Animation<Image>();
		startAnim.AddTrack(new AnimationTrack<Image, Rect>(new KeyFrame<Rect>[]
		{
			new (xyTexture.GetUVRect(new RectInt(0, 0, 22, 21)), 0),
			new (xyTexture.GetUVRect(new RectInt(22, 0, 22, 21)), 1.5F),
			new (xyTexture.GetUVRect(new RectInt(44, 0, 22, 21)), 1.6F),
			new (xyTexture.GetUVRect(new RectInt(66, 0, 22, 21)), 1.7F),
			new (xyTexture.GetUVRect(new RectInt(88, 0, 22, 21)), 1.8F),
			new (xyTexture.GetUVRect(new RectInt(110, 0, 22, 21)), 1.9F),
			new (xyTexture.GetUVRect(new RectInt(132, 0, 22, 21)), 2.5F),
			new (xyTexture.GetUVRect(new RectInt(110, 0, 22, 21)), 2.6F)
		}, (t, rect) => t.SetUV(rect)));
		logoAnimController.AddBlock("start", startAnim, (animator, _) =>
		{
			if (!animator.isRunning)
				animator.SetAnimation("loop");
		});
		
		var loopAnim = new Animation<Image> { loop = true };
		loopAnim.AddTrack(new AnimationTrack<Image, Rect>(new KeyFrame<Rect>[]
		{
			new (xyTexture.GetUVRect(new RectInt(110, 0, 22, 21)), 0),
			new (xyTexture.GetUVRect(new RectInt(132, 0, 22, 21)), 2.5F),
			new (xyTexture.GetUVRect(new RectInt(110, 0, 22, 21)), 2.6F),
			new (xyTexture.GetUVRect(new RectInt(132, 0, 22, 21)), 4F),
			new (xyTexture.GetUVRect(new RectInt(110, 0, 22, 21)), 4.1F)
		}, (t, rect) => t.SetUV(rect)));
		logoAnimController.AddBlock("loop", loopAnim);
		logoAnimator = new Animator<Image>();
		logoAnimator.SetController(logo, logoAnimController);
		logoAnimator.SetAnimation("start");
		logoAnimator.Play();
		
		backMaterial = new MaterialUI(backShader, ("repeat", windowSize / backTexture.size), ("isBackground", true))
			.SetTexture(backTexture);
		
		progressBar = new ProgressBar(0, maxValue: 1)
		{
			position = new Vector2Int(0, 10), anchorMin = Vector2.zero, anchorMax = Vector2.right,
			size = new Vector2Int(0, 5), margin = new RegionInt(10),
			tint = Color.black
		};
		errorLabel = new Label
		{
			position = new Vector2Int(5, -5), pivotAndAnchors = new Vector2(0, 1), tint = Color.red
		};
		
		Task.Run(async () =>
		{
			XY.InternalLog("SPLASHSCREEN", "Starting loading...");
			
			var isFaulted = false;
			await loadingTask.Invoke(new Progress<float>(value => targetValue = value)).ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					isFaulted = true;
					XY.InternalLog("SPLASHSCREEN",
								   $"Unable to continue, an error occurred : {task.Exception?.InnerException?.Message ?? task.Exception?.Message}",
								   TypeLog.Error);
					errorLabel.text = "A problem has occurred, check logs !";
				}
			});
			
			if (isFaulted)
				throw new Exception("Loading task failed !");
			
			await Task.Delay(3000);
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
		root.AddChild(logo);
		root.AddChild(new Label($"v{XY.version}") { position = new Vector2Int(20, 15), tint = Color.green });
		root.AddChild(progressBar);
		root.AddChild(errorLabel);
	}
	
	protected override void Update()
	{
		logoAnimator.Update();
		progressBar.value = float.Lerp(progressBar.value, targetValue, Time.delta * 10);
		
		if (loaded)
		{
			XYDebug.Load();
			SetStartScene();
		}
	}
	
	private static void LoadDefaultAssets()
	{
		var font = Vault.LoadEmbeddedResource<Font>("jetbrains.ttf", "fonts.jetbrains.ttf");
		font.GenerateBitmap(16, 256);
		
		Vault.LoadEmbeddedResource<Texture2D>("xyengine.png", "textures.xyengine.png", Texture2D.internalConfig);
		Vault.LoadEmbeddedResource<Shader>("default.shader", "shaders.default.shadxy", new ShaderConfig(material =>
		{
			material.SetProperty(MaterialObject.TINT, Color.white);
			material.SetProperty(MaterialObject.UV_RECT, new Rect(0, 0, 1, 1));
			material.SetProperty(MaterialObject.ALPHA, 1F);
		}, delegate { }));
		Vault.LoadEmbeddedResource<Shader>("camera.shader", "shaders.camera.shadxy");
		Vault.LoadEmbeddedResource<Shader>("canvas.shader", "shaders.canvas.shadxy");
		Vault.LoadEmbeddedResource<Shader>("ui.shader", "shaders.ui.shadxy", new ShaderConfig(material =>
		{
			material.SetProperty(MaterialUI.UV_RECT, new Rect(Vector2.zero, Vector2.one));
			material.SetProperty(MaterialUI.NINEPATCH, new Region(Vector2.zero));
			material.SetProperty(MaterialUI.NINEPATCH_SCALE, 1F);
			material.SetProperty(MaterialUI.CORNER_RADIUS, new Region(0));
		}, (material, name, obj) =>
		{
			if (name == "mainTex" && obj is Texture2D texture)
				material.SetProperty("mainTexSize", texture.size.ToVector2());
		}));
	}
	
	private static void SetStartScene()
	{
		Graphics.SetBackgroundColor(GameWindow.startConfig.backgroundColor);
		GameWindow.SetDisplayMode(GameWindow.startConfig.mode);
		
		Vault.RemoveEmbeddedAsset("splash.shader");
		Vault.RemoveEmbeddedAsset("splash.texture");
		
		SceneManager.SetCurrentScene(startScene);
	}
	
	protected override void Destroy()
	{
		loadingTask = null;
		startScene = null;
	}
}