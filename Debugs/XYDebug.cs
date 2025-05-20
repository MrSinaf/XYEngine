using System.Numerics;
using System.Reflection;
using System.Text.Json;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using XYEngine.Debugs.Windows;
using XYEngine.Inputs;
using Key = XYEngine.Inputs.Key;

namespace XYEngine.Debugs;

public enum DebugState { Full, Limited, None }

public static class XYDebug
{
	public static DebugState state { get; internal set; } = DebugState.Full;
	
	private static IDebugWindow[] debugWindows;
	private static ImGuiController imGuiController;
	private static bool showMainMenuBar;
	
	internal static void Load(GL gl, IView view, IInputContext input)
	{
		if (state == DebugState.None)
			return;
		
		debugWindows = [new DebugAssets(), new DebugCanvas()];
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
		style.WindowBorderSize = 0;
		style.ChildBorderSize = 0;
		style.PopupBorderSize = 0;
		style.WindowTitleAlign = new System.Numerics.Vector2(0.5F, 0.5F);
		style.WindowMenuButtonPosition = ImGuiDir.None;
		
		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		
		foreach (var debugWindow in debugWindows)
			debugWindow.Create();
	}
	
	internal static void Update()
	{
		if (state == DebugState.None)
			return;
		
		imGuiController.Update(Time.delta);
		
		if (Input.IsKeyHeldDown(Key.ControlLeft) && Input.IsKeyPressed(Key.F1))
		{
			showMainMenuBar = !showMainMenuBar;
			AdapteRootCanvasSize();
		}
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
					SceneManager.SetCurrentScene(SceneManager.current.GetType());
					AdapteRootCanvasSize();
				}
				
				if (ImGui.MenuItem("Rebuild UI"))
				{
					SceneManager.current.canvas.Destroy();
					SceneManager.current.InternalBuildUI();
					AdapteRootCanvasSize();
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
			
			if (ImGui.BeginMenu("Windows"))
			{
				foreach (var debugWindow in debugWindows)
					if (ImGui.MenuItem(debugWindow.name))
						debugWindow.visible = true;
				ImGui.EndMenu();
			}
			
			ImGui.EndMainMenuBar();
		}
		
		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2);
		foreach (var window in debugWindows)
			if (window.visible)
			{
				var visible = window.visible;
				
				ImGui.Begin(window.name, ref visible);
				window.Render();
				ImGui.End();
				
				window.visible = visible;
				window.notFirstDraw = true;
			}
		ImGui.PopStyleVar();
		
		imGuiController.Render();
	}
	
	public static void ShowProperty(string text, object property)
	{
		ImGui.Text($"{text} :");
		ImGui.SameLine();
		ImGui.TextColored(new Vector4(0.5f, 0.8f, 0.3f, 1.0f), property.ToString());
	}
	
	public static bool IsCompatibleInput(Type type) => type == typeof(string) || type == typeof(bool) || type == typeof(Vector2) || type == typeof(Vector2Int) ||
													   type == typeof(RegionInt) || type == typeof(Region) || type == typeof(RectInt) || type == typeof(Rect) ||
													   type == typeof(float) || type == typeof(int);
	
	public static object GetInput(string id, object property)
	{
		var type = property.GetType();
		if (type == typeof(string))
		{
			var value = ((string)property).Replace("\n", @"\n");
			if (ImGui.InputText($"##{id}", ref value, 9999))
				property = value.Replace(@"\n", "\n");
		}
		else if (type == typeof(bool))
		{
			var value = (bool)property;
			if (ImGui.Checkbox($"##{id}", ref value))
				property = value;
		}
		else if (type == typeof(Vector2))
		{
			var value = ToSystem((Vector2)property);
			if (ImGui.DragFloat2($"##{id}", ref value, 0.1F))
				property = ToXY(value);
		}
		else if (type == typeof(Vector2Int))
		{
			var vector = (Vector2Int)property;
			int[] values = [vector.x, vector.y];
			if (ImGui.DragInt2($"##{id}", ref values[0], 1))
				property = new Vector2Int(values[0], values[1]);
		}
		else if (type == typeof(Region))
		{
			var region = (Region)property;
			var value = new Vector4(region.position00.x, region.position00.y, region.position11.x, region.position11.y);
			if (ImGui.DragFloat4($"##{id}", ref value, 1))
				property = new Region(value.X, value.Y, value.Z, value.W);
		}
		else if (type == typeof(RegionInt))
		{
			var region = (RegionInt)property;
			int[] values = [region.position00.x, region.position00.y, region.position11.x, region.position11.y];
			if (ImGui.DragInt4($"##{id}", ref values[0], 1))
				property = new RegionInt(values[0], values[1], values[2], values[3]);
		}
		else if (type == typeof(Rect))
		{
			var region = (Rect)property;
			var value = new Vector4(region.position.x, region.position.y, region.size.x, region.size.y);
			if (ImGui.DragFloat4($"##{id}", ref value, 0.1F))
				property = new Rect(value.X, value.Y, value.Z, value.W);
		}
		else if (type == typeof(RectInt))
		{
			var region = (RectInt)property;
			int[] values = [region.position.x, region.position.y, region.size.x, region.size.y];
			if (ImGui.DragInt4($"##{id}", ref values[0], 1))
				property = new RectInt(values[0], values[1], values[2], values[3]);
		}
		else if (type == typeof(float))
		{
			var value = (float)property;
			if (ImGui.DragFloat($"##{id}", ref value, 0.05F))
				property = value;
		}
		else if (type == typeof(int))
		{
			var value = (int)property;
			if (ImGui.DragInt($"##{id}", ref value))
				property = value;
		}
		
		return property;
	}
	
	public static System.Numerics.Vector2 ToSystem(Vector2 value) => new (value.x, value.y);
	
	public static Vector2 ToXY(System.Numerics.Vector2 value) => new (value.X, value.Y);
	
	private static void AdapteRootCanvasSize()
	{
		SceneManager.current.canvas.root.UpdateSize(Graphics.resolution - (showMainMenuBar ? new Vector2Int(0, 18) : Vector2Int.zero));
	}
}