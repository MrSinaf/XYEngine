using System.Numerics;
using System.Reflection;
using System.Text.Json;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using XYEngine.Scenes;

namespace XYEngine.Debugs;

public static class XYDebug
{
	private static ImGuiController imGuiController;
	
	internal static void Load(GL gl, IView view, IInputContext input)
	{
		imGuiController = new ImGuiController(gl, view, input);
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("XYEngine.assets.imgui_style.json");
		
		if (stream != null)
		{
			using var reader = new StreamReader(stream);
			var json = reader.ReadToEnd();
			var colorArray = JsonSerializer.Deserialize<float[][]>(json);
			
			if (colorArray != null)
			{
				var colors = ImGui.GetStyle().Colors;
				for (var i = 0; i < colorArray.Length && i < colors.Count; i++)
					colors[i] = new Vector4(colorArray[i][0], colorArray[i][1], colorArray[i][2], colorArray[i][3]);
			}
		}
		
		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		DebugHotReload.Init();
	}
	
	internal static void Update()
	{
		imGuiController.Update(Time.delta);
		DebugHotReload.Update();
	}
	
	internal static void Render()
	{
		if (SceneManager.current.GetType() == typeof(SplashScreen))
			return;
		
		ImGui.BeginMainMenuBar();
		if (ImGui.BeginMenu("XY"))
		{
			if (ImGui.MenuItem("Reload Scene"))
			{
				SceneManager.current.InternalDestroy();
				SceneManager.current.InternalStart();
			}
			
			if (ImGui.BeginMenu("Display"))
			{
				if (ImGui.MenuItem("Windowed"))
					GameWindow.SetDisplayMode(DisplayMode.Windowed);
				if (ImGui.MenuItem("No Border"))
					GameWindow.SetDisplayMode(DisplayMode.NoBorder);
				if (ImGui.MenuItem("Fullscreen"))
					GameWindow.SetDisplayMode(DisplayMode.FullScren);
				
				ImGui.EndMenu();
			}
			
			if (ImGui.MenuItem("Exit"))
				GameWindow.Close();
			
			ImGui.EndMenu();
		}
		
		if (ImGui.BeginMenu("Tools"))
		{
			if (ImGui.MenuItem("Assets"))
				DebugAssets.showAssets = true;
			
			if (ImGui.MenuItem("Canvas"))
				DebugCanvas.showCanvas = true;
			
			ImGui.Separator();
			
			var hotReloadActif = DebugHotReload.actif;
			if (ImGui.MenuItem("HotReload", string.Empty, ref hotReloadActif))
				DebugHotReload.actif = !DebugHotReload.actif;
			
			ImGui.EndMenu();
		}
		
		ImGui.EndMainMenuBar();
		
		DebugCanvas.Render();
		DebugAssets.Render();
		imGuiController.Render();
	}
	
	public static System.Numerics.Vector2 ToSystem(Vector2 value) => new (value.x, value.y);
	
	public static Vector2 ToXY(System.Numerics.Vector2 value) => new (value.X, value.Y);
}