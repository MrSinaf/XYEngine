using System.Reflection;
using XYEngine.Debugs.Windows;
using XYEngine.Inputs;
using Key = XYEngine.Inputs.Key;

namespace XYEngine.Debugs;

public static class XYDebug
{
	public static bool active { get => field && isLoaded; set; }
	
	private static bool isLoaded;
	private static DebugWindow[] debugWindows;
	private static ImGuiController imGuiController;
	private static bool showMainMenuBar = true;
	
	public static event Action onImGuiRender = delegate { };
	
	internal static void Load()
	{
		isLoaded = true;
		if (!active) return;
		
		imGuiController = new ImGuiController();
		
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("XYEngine.assets.imgui_style.json");
		
		ApplyXYStyle();
		debugWindows = [new DebugAssets(), new DebugObjects(), new DebugCanvas(), new DebugPerformance()];
		ImGui.GetIO().ConfigErrorRecoveryEnableAssert = false;
	}
	
	internal static void Update()
	{
		if (!isLoaded) return;
		
		imGuiController.Update(Time.delta);
		
		if (Input.IsKeyPressed(Key.F1) && Input.IsKeyHeldDown(Key.ControlLeft))
		{
			showMainMenuBar = !showMainMenuBar;
			AdapteRootCanvasSize();
		}
	}
	
	internal static void Render()
	{
		if (!isLoaded) return;
		
		if (showMainMenuBar && ImGui.BeginMainMenuBar())
		{
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
			
			foreach (var debugWindow in debugWindows)
			{
				if (ImGui.BeginMenu(debugWindow.menuItem))
				{
					if (ImGui.MenuItem(debugWindow.name))
						debugWindow.visible = true;
					ImGui.EndMenu();
				}
			}
			
			ImGui.EndMainMenuBar();
		}
		
		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2);
		foreach (var window in debugWindows)
			if (window.visible)
			{
				var visible = window.visible;
				
				ImGui.SetNextWindowSize(window.size, ImGuiCond.Once);
				if (ImGui.Begin(window.name, ref visible, window.flags))
				{
					window.Render();
					ImGui.End();
				}
				
				window.visible = visible;
				window.notFirstDraw = true;
			}
		
		onImGuiRender();
		ImGui.PopStyleVar();
		
		imGuiController.Render();
	}
	
	public static void ShowProperty(string text, object property)
	{
		ImGui.Text($"{text} :");
		ImGui.SameLine();
		ImGui.TextColored(new Vector4(0.5f, 0.8f, 0.3f, 1.0f), property.ToString());
	}
	
	private static void AdapteRootCanvasSize()
		=> SceneManager.current.canvas.root.UpdateSize(Graphics.resolution - (showMainMenuBar ? new Vector2Int(0, 18) : Vector2Int.zero));
	
	public static bool IsCompatibleInput(Type type) => type == typeof(string) || type == typeof(bool) || type == typeof(Vector2) || type == typeof(Vector2Int)
													   ||
													   type == typeof(RegionInt) || type == typeof(Region) || type == typeof(RectInt) || type == typeof(Rect) ||
													   type == typeof(float) || type == typeof(int) || type == typeof(Color);
	
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
			var value = (Vector2)property;
			if (ImGui.DragFloat2($"##{id}", ref value, 0.1F))
				property = value;
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
				property = new Region(value.x, value.y, value.z, value.w);
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
				property = new Rect(value.x, value.y, value.z, value.w);
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
		else if (type == typeof(Color))
		{
			var color = (Color)property;
			var value = new Vector4(color.r, color.g, color.b, color.a) * Color.FACTOR;
			if (ImGui.ColorButton($"##{id}", value))
				ImGui.OpenPopup($"colorPicker_{id}");
			
			if (ImGui.BeginPopup($"colorPicker_{id}"))
			{
				if (ImGui.ColorPicker4($"##picker_{id}", ref value))
					property = new Color((byte)(value.x * 255), (byte)(value.y * 255), (byte)(value.z * 255), (byte)(value.w * 255));
				
				ImGui.EndPopup();
			}
		}
		
		return property;
	}
	
	private static void ApplyXYStyle()
	{
		var style = ImGui.GetStyle();
		var colors = style.Colors;
		colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
		colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
		colors[(int)ImGuiCol.WindowBg] = new Vector4(0.07f, 0.14f, 0.20f, 1.00f);
		colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
		colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
		colors[(int)ImGuiCol.Border] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f);
		colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
		colors[(int)ImGuiCol.FrameBg] = new Vector4(0.09f, 0.17f, 0.24f, 1.00f);
		colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.40f);
		colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
		colors[(int)ImGuiCol.TitleBg] = new Vector4(0.18f, 0.67f, 0.39f, 0.49f);
		colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.18f, 0.67f, 0.39f, 1.00f);
		colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
		colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.09f, 0.17f, 0.24f, 1.00f);
		colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
		colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
		colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
		colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
		colors[(int)ImGuiCol.CheckMark] = new Vector4(0.18f, 0.67f, 0.39f, 1.00f);
		colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.18f, 0.67f, 0.39f, 1.00f);
		colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.15f, 0.75f, 0.41f, 1.00f);
		colors[(int)ImGuiCol.Button] = new Vector4(0.18f, 0.67f, 0.39f, 1.00f);
		colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.13f, 0.56f, 0.31f, 1.00f);
		colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.06f, 0.53f, 0.98f, 1.00f);
		colors[(int)ImGuiCol.Header] = new Vector4(0.15f, 0.23f, 0.29f, 1.00f);
		colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.13f, 0.55f, 0.31f, 1.00f);
		colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.18f, 0.67f, 0.39f, 1.00f);
		colors[(int)ImGuiCol.Separator] = new Vector4(0.18f, 0.67f, 0.39f, 1.00f);
		colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.11f, 0.52f, 0.29f, 1.00f);
		colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.15f, 0.81f, 0.43f, 1.00f);
		colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.26f, 0.59f, 0.98f, 0.20f);
		colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
		colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
		colors[(int)ImGuiCol.TabHovered] = new Vector4(0.09f, 0.17f, 0.26f, 1.00f);
		colors[(int)ImGuiCol.Tab] = new Vector4(0.14f, 0.24f, 0.41f, 1.00f);
		colors[(int)ImGuiCol.TabSelected] = new Vector4(0.15f, 0.25f, 0.34f, 1.00f);
		colors[(int)ImGuiCol.TabSelectedOverline] = new Vector4(0.07f, 0.10f, 0.15f, 0.97f);
		colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.14f, 0.26f, 0.42f, 1.00f);
		colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.15f, 0.96f, 0.01f, 0.20f);
		colors[(int)ImGuiCol.TabDimmedSelectedOverline] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
		colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
		colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
		colors[(int)ImGuiCol.PlotLines] = new Vector4(0.28f, 0.90f, 0.00f, 1.00f);
		colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.00f, 1.00f, 0.22f, 1.00f);
		colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
		colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
		colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.13f, 0.29f, 0.22f, 1.00f);
		colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
		colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.00f, 0.00f, 0.00f, 0.06f);
		colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.35f);
		colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(0.13f, 0.13f, 0.13f, 0.90f);
		colors[(int)ImGuiCol.TextLink] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
		colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
		colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
		colors[(int)ImGuiCol.NavCursor] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
		colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
		colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
		colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
		
		style.WindowRounding = 8;
		style.FrameRounding = 3;
		style.WindowBorderSize = 0;
		style.ChildBorderSize = 0;
		style.PopupBorderSize = 0;
		style.WindowTitleAlign = new Vector2(0.5F, 0.5F);
		style.WindowMenuButtonPosition = ImGuiDir.None;
	}
}