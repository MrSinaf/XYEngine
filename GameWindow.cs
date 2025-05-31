using System.Reflection;
using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;
using XYEngine.Debugs;
using XYEngine.Inputs;
using XYEngine.Rendering;
using XYEngine.Scenes;
using XYEngine.UI;
using XYEngine.Utils;

namespace XYEngine;

public enum DisplayMode { Windowed, NoBorder, FullScreen }

public class GameWindow
{
	private static GameWindow m;
	
	public static Vector2Int size { get; private set; }
	private readonly IWindow window;
	
	internal GameWindow(string name)
	{
		m = this;
		size = new Vector2Int(640, 360);
		window = Window.Create(WindowOptions.Default with
		{
			Title = name,
			VSync = false,
			
			// Initialisation des paramètres pour le SplashScreen :
			WindowBorder = WindowBorder.Hidden,
			TransparentFramebuffer = false,
			Size = new Vector2D<int>(size.x, size.y)
		});
		
		window.Load += Load;
		window.Update += Update;
		window.Render += Render;
		window.FramebufferResize += FramebufferResize;
		window.Closing += Closing;
		
		window.Run();
	}
	
	public static void Close() => m?.window.Close();
	
	public static void SetDisplayMode(DisplayMode mode)
	{
		var window = m.window;
		switch (mode)
		{
			case DisplayMode.Windowed:
				window.IsVisible = false;
				window.WindowBorder = WindowBorder.Resizable;
				window.WindowState = WindowState.Normal;
				window.Size = new Vector2D<int>(1200, 600);
				window.Center();
				window.IsVisible = true;
				window.Focus();
				break;
			case DisplayMode.NoBorder:
				window.IsVisible = false;
				window.WindowBorder = WindowBorder.Hidden;
				window.WindowState = WindowState.Maximized;
				window.Size = window.Monitor.VideoMode.Resolution!.Value;
				window.IsVisible = true;
				break;
			case DisplayMode.FullScreen:
				window.WindowState = WindowState.Fullscreen;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
		}
		
		size = new Vector2Int(window.Size.X, window.Size.Y);
	}
	
	public static void SetBackgroundColor(Color color) => Graphics.SetBackgroundColor(color);
	
	internal static IWindow GetWindow() => m.window;
	
	private void Load()
	{
		ImageResult result;
		if (File.Exists("icon.png"))
			result = ImageResult.FromStream(new FileStream("icon.png", FileMode.Open), ColorComponents.RedGreenBlueAlpha);
		else
		{
			var assembly = Assembly.GetExecutingAssembly();
			using var stream = assembly.GetManifestResourceStream("XYEngine.assets.textures.xy.png");
			result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
		}
		
		var icon = new RawImage(result.Width, result.Height, result.Data);
		var input = window.CreateInput();
		var gl = window.CreateOpenGL();
		
		window.SetWindowIcon(ref icon);
		window.Center();
		
		Graphics.Init(gl);
		Graphics.Viewport(size);
		
		Primitif.Init();
		Input.Initialize(input);
		Audio.Initialize();
		
		SceneManager.SetCurrentScene<SplashScreen>();
		XYDebug.Load();
	}
	
	private static void Update(double delta)
	{
		Time.Update(delta);
		try
		{
			SceneManager.Update();
		}
		catch (Exception e)
		{
			XY.InternalLog("Error", e, TypeLog.Error);
		}
		
		UIEvent.Update();
		Input.Update();
		XYDebug.Update();
	}
	
	private static void Render(double delta)
	{
		GCommandQueue.ExecuteAll();
		SceneManager.Render();
		XYDebug.Render();
	}
	
	private static void FramebufferResize(Vector2D<int> value)
	{
		size = new Vector2Int(value.X, value.Y);
		Graphics.Viewport(size);
	}
	
	private void Closing()
	{
		m = null;
		Audio.Dispose();
		SceneManager.Dispose();
	}
}