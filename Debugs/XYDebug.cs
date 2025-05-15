using System.Numerics;
using System.Reflection;
using System.Text.Json;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using XYEngine.Inputs;
using Key = XYEngine.Inputs.Key;

namespace XYEngine.Debugs;

public enum DebugState { Full, Limited, None }

public static class XYDebug
{
	public static DebugState state { get; internal set; } = DebugState.Full;
	
	private static ImGuiController imGuiController;
	private static bool showMainMenuBar;
	
	internal static void Load(GL gl, IView view, IInputContext input)
	{
		if (state == DebugState.None)
			return;
		
		imGuiController = new ImGuiController(gl, view, input);
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("XYEngine.assets.imgui_style.json");
		
		var style = ImGui.GetStyle();
		if (stream != null)
		{
			using var reader = new StreamReader(stream);
			var json = reader.ReadToEnd();
			var colorArray = JsonSerializer.Deserialize<float[][]>(json);
			
			if (colorArray != null)
			{
				var colors = style.Colors;
				for (var i = 0; i < colorArray.Length && i < colors.Count; i++)
					colors[i] = new Vector4(colorArray[i][0], colorArray[i][1], colorArray[i][2], colorArray[i][3]);
			}
		}
		
		style.WindowRounding = 8;
		style.FrameRounding = 3;
		style.WindowTitleAlign = new System.Numerics.Vector2(0.5F, 0.5F);
		style.WindowMenuButtonPosition = ImGuiDir.None;
		
		
		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		DebugHotReload.Init();
	}
	
	internal static void Update()
	{
		if (state == DebugState.None)
			return;
		
		imGuiController.Update(Time.delta);
		DebugHotReload.Update();
		
		if (Input.IsKeyHeldDown(Key.ControlLeft) && Input.IsKeyPressed(Key.F1))
			showMainMenuBar = !showMainMenuBar;
	}
	
	internal static void Render()
	{
		if (state == DebugState.None)
			return;
		
		if (showMainMenuBar)
		{
			ImGui.BeginMainMenuBar();
			if (ImGui.BeginMenu("XY"))
			{
				if (ImGui.MenuItem("Reload Scene"))
				{
					SceneManager.current.InternalDestroy();
					SceneManager.current.InternalStart();
				}
				
				if (ImGui.MenuItem("Rebuild UI"))
				{
					SceneManager.current.canvas.Destroy();
					SceneManager.current.InternalBuildUI();
				}
				
				if (ImGui.BeginMenu("Display"))
				{
					if (ImGui.MenuItem("Windowed"))
						GameWindow.SetDisplayMode(DisplayMode.Windowed);
					if (ImGui.MenuItem("No Border"))
						GameWindow.SetDisplayMode(DisplayMode.NoBorder);
					if (ImGui.MenuItem("Fullscreen"))
						GameWindow.SetDisplayMode(DisplayMode.FullScreen);
					
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
				
				
				if (state == DebugState.Full)
				{
					var hotReloadActif = DebugHotReload.actif;
					if (ImGui.MenuItem("HotReload (TEST)", string.Empty, ref hotReloadActif))
						DebugHotReload.actif = !DebugHotReload.actif;
				}
				
				ImGui.EndMenu();
			}
			
			ImGui.EndMainMenuBar();
		}
		
		DebugCanvas.Render();
		DebugAssets.Render();
		imGuiController.Render();
	}
	
	public static System.Numerics.Vector2 ToSystem(Vector2 value) => new (value.x, value.y);
	
	public static Vector2 ToXY(System.Numerics.Vector2 value) => new (value.X, value.Y);
}