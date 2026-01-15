using System.Collections;
using XYEngine.Debugs.Windows;
using XYEngine.Inputs;
using Key = XYEngine.Inputs.Key;

namespace XYEngine.Debugs;

public static class XYDebug
{
	public static bool active;
	
	private static bool isLoaded;
	private static ImGuiController imGuiController;
	
	private static readonly Dictionary<string, IDebugMainBar> debugMainBars = [];
	private static readonly Dictionary<string, IDebugWindow> debugWindows = [];
	private static readonly Dictionary<string, IDebugRender> debugRenders = [];
	
	private static readonly List<string> debugToRemove = [];
	private static readonly Dictionary<string, IImGuiRenderable> debugToAdd = [];
	
	private static bool showMainMenuBar = true;
	
	internal static void Load()
	{
		if (!active) return;
		
		imGuiController = new ImGuiController();
		isLoaded = true;
		
		ApplyXYStyle();
		Add("Scene", new DebugScene());
		Add("Vault", new DebugVault());
		Add("Canvas", new DebugCanvas());
		Add("Performance", new DebugPerformance());
		Add("About", new DebugAbout());
		
		Input.keyDown += OnKeyDown;
	}
	
	internal static void Update()
	{
		if (!isLoaded) return;
		
		imGuiController.Update(Time.delta);
		DebugHotReload.Update();
	}
	
	internal static void Render()
	{
		if (!isLoaded) return;
		
		foreach (var (name, imGuiRenderable) in debugToAdd)
		{
			if (imGuiRenderable is IDebugWindow window)
				debugWindows[name] = window;
			if (imGuiRenderable is IDebugMainBar mainBar)
				debugMainBars[name] = mainBar;
			if (imGuiRenderable is IDebugRender render)
				debugRenders[name] = render;
			
			ImGui.SetWindowFocus(name);
		}
		debugToAdd.Clear();
		
		foreach (var name in debugToRemove)
		{
			debugWindows.Remove(name);
			debugMainBars.Remove(name);
			debugRenders.Remove(name);
		}
		debugToRemove.Clear();
		
		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
		if (showMainMenuBar && ImGui.BeginMainMenuBar())
		{
			if (ImGui.BeginMenu("XY"))
			{
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
			
			foreach (var (_, value) in debugMainBars)
				value.OnDebugMainBarRender();
			
			ImGui.EndMainMenuBar();
		}
		ImGui.PopStyleVar();
		
		foreach (var (name, value) in debugWindows)
		{
			if (value.visible)
				value.OnDebugWindowRender(name);
		}
		
		foreach (var (name, value) in debugRenders)
			value.OnDebugRender(name);
		
		imGuiController.Render();
	}
	
	public static void Add(string name, IImGuiRenderable imGuiRenderable) => debugToAdd.Add(name, imGuiRenderable);
	public static void Remove(string name) => debugToRemove.Add(name);
	
	private static void OnKeyDown(Key key)
	{
		if (key == Key.F1 && Input.IsKeyHeldDown(Key.ControlLeft))
			showMainMenuBar = !showMainMenuBar;
	}
	
	public static void ShowCollapsingProperties(string name, object obj)
	{
		if (ImGui.CollapsingHeader($"{name}##{obj.GetHashCode()}"))
		{
			ImGui.Indent(10);
			ShowObjectProperties(obj);
			ImGui.Unindent(10);
		}
	}
	
	public static void ShowObjectProperties(object obj, bool checkIsProperty = true)
	{
		if (checkIsProperty && obj is IDebugProperty debugProperty)
		{
			debugProperty.OnDebugPropertyRender();
		}
		else
		{
			var componentType = obj.GetType();
			var properties = componentType.GetProperties();
			var fields = componentType.GetFields();
			
			foreach (var p in properties)
			{
				var value = p.GetValue(obj);
				if (value is IAsset asset)
				{
					var name = p.PropertyType.Name;
					ImGui.InputText(p.Name, ref name, int.MaxValue, ImGuiInputTextFlags.ReadOnly);
					if (ImGui.IsItemClicked())
						Add($"{componentType.Name}.{p.Name}", new DebugObject(asset));
				}
				else if (value is IDebugProperty property)
				{
					var name = p.PropertyType.Name;
					ImGui.InputText(p.Name, ref name, int.MaxValue, ImGuiInputTextFlags.ReadOnly);
					if (ImGui.IsItemClicked())
						Add($"{componentType.Name}.{p.Name}", new DebugObject(property));
				}
				else if (p.CanRead && p.CanWrite && IsCompatibleInput(p.PropertyType))
					p.SetValue(obj, GetInput(p.Name, value));
			}
			
			foreach (var f in fields)
			{
				var value = f.GetValue(obj);
				if (f.IsSpecialName)
					return;
				
				if (value is IAsset asset)
				{
					var name = f.FieldType.Name;
					ImGui.InputText(f.Name, ref name, int.MaxValue, ImGuiInputTextFlags.ReadOnly);
					if (ImGui.IsItemClicked())
						Add($"{componentType.Name}.{f.Name}", new DebugObject(asset));
				}
				else if (value is IDebugProperty property)
				{
					var name = f.FieldType.Name;
					ImGui.InputText(f.Name, ref name, int.MaxValue, ImGuiInputTextFlags.ReadOnly);
					if (ImGui.IsItemClicked())
						Add($"{componentType.Name}.{f.Name}", new DebugObject(property));
				}
				else if (!f.IsLiteral && !f.IsInitOnly && IsCompatibleInput(f.FieldType))
					f.SetValue(obj, GetInput(f.Name, f.GetValue(obj)));
				else if (value is IEnumerable enumerable)
				{
					if (ImGui.CollapsingHeader(f.Name))
					{
						ImGui.Indent(10);
						foreach (var o in enumerable)
							ShowObjectProperties(o);
						ImGui.Unindent(10);
					}
				}
			}
		}
	}
	
	public static Vector2 WorldToImGuiPosition(Vector2 position, Camera camera)
	{
		var relativePos = position - camera.position;
		relativePos.y = -relativePos.y;
		
		return relativePos * camera.zoom + camera.halfResolution * camera.zoom;
	}
	
	public static void ShowProperties(string name, object obj)
	{
		var componentType = obj.GetType();
		var properties = componentType.GetProperties();
		var fields = componentType.GetFields();
		
		var propertyId = 0;
		ImGui.Columns(2, "Params", false);
		ImGui.SetColumnWidth(0, 150);
		foreach (var p in properties)
		{
			if (!p.CanRead)
				continue;
			
			if (p.CanWrite && IsCompatibleInput(p.PropertyType))
			{
				ImGui.AlignTextToFramePadding();
				ImGui.Text(p.Name);
				ImGui.NextColumn();
				ImGui.PushItemWidth(-1f);
				ImGui.PushID(name + propertyId++);
				p.SetValue(obj, GetInput(p.Name, p.GetValue(obj)));
				ImGui.PopID();
				ImGui.PopItemWidth();
				ImGui.NextColumn();
			}
		}
		
		foreach (var f in fields)
		{
			if (!f.IsLiteral && !f.IsInitOnly && IsCompatibleInput(f.FieldType))
			{
				ImGui.AlignTextToFramePadding();
				ImGui.Text(f.Name);
				ImGui.NextColumn();
				ImGui.PushItemWidth(-1f);
				ImGui.PushID(name + propertyId++);
				f.SetValue(obj, GetInput(f.Name, f.GetValue(obj)));
				ImGui.PopID();
				ImGui.PopItemWidth();
				ImGui.NextColumn();
			}
		}
		ImGui.Columns();
	}
	
	public static void ShowValue(string text, object property)
	{
		ImGui.Text($"{text} :");
		ImGui.SameLine();
		ImGui.TextColored(new Vector4(0.5f, 0.8f, 0.3f, 1.0f), property.ToString());
	}
	
	public static bool IsCompatibleInput(Type type) => type == typeof(string) ||
													   type == typeof(bool) ||
													   type == typeof(Vector2) ||
													   type == typeof(Vector2Int) ||
													   type == typeof(Vector3) ||
													   type == typeof(Vector3Int) ||
													   type == typeof(Vector4) ||
													   type == typeof(Vector4Int) ||
													   type == typeof(RegionInt) ||
													   type == typeof(Region) ||
													   type == typeof(RectInt) ||
													   type == typeof(Rect) ||
													   type == typeof(float) ||
													   type == typeof(int) ||
													   type == typeof(Color);
	
	public static object GetInput(string id, object property)
	{
		var type = property.GetType();
		if (type == typeof(string))
		{
			var value = ((string)property).Replace("\n", @"\n");
			if (ImGui.InputText($"{id}", ref value, 9999))
				property = value.Replace(@"\n", "\n");
		}
		else if (type == typeof(bool))
		{
			var value = (bool)property;
			if (ImGui.Checkbox($"{id}", ref value))
				property = value;
		}
		else if (type == typeof(Vector2))
		{
			var value = (Vector2)property;
			if (ImGui.DragFloat2($"{id}", ref value, 0.1F))
				property = value;
		}
		else if (type == typeof(Vector2Int))
		{
			var vector = (Vector2Int)property;
			int[] values = [vector.x, vector.y];
			if (ImGui.DragInt2($"{id}", ref values[0], 1))
				property = new Vector2Int(values[0], values[1]);
		}
		else if (type == typeof(Vector3))
		{
			var value = (Vector3)property;
			if (ImGui.DragFloat3($"{id}", ref value, 0.1F))
				property = value;
		}
		else if (type == typeof(Vector3Int))
		{
			var vector = (Vector3Int)property;
			int[] values = [vector.x, vector.y, vector.z];
			if (ImGui.DragInt3($"{id}", ref values[0], 1))
				property = new Vector3Int(values[0], values[1], values[2]);
		}
		else if (type == typeof(Region))
		{
			var region = (Region)property;
			var value = new Vector4(region.position00.x, region.position00.y, region.position11.x, region.position11.y);
			if (ImGui.DragFloat4($"{id}", ref value, 1))
				property = new Region(value.x, value.y, value.z, value.w);
		}
		else if (type == typeof(RegionInt))
		{
			var region = (RegionInt)property;
			int[] values = [region.position00.x, region.position00.y, region.position11.x, region.position11.y];
			if (ImGui.DragInt4($"{id}", ref values[0], 1))
				property = new RegionInt(values[0], values[1], values[2], values[3]);
		}
		else if (type == typeof(Rect))
		{
			var region = (Rect)property;
			var value = new Vector4(region.position.x, region.position.y, region.size.x, region.size.y);
			if (ImGui.DragFloat4($"{id}", ref value, 0.1F))
				property = new Rect(value.x, value.y, value.z, value.w);
		}
		else if (type == typeof(RectInt))
		{
			var region = (RectInt)property;
			int[] values = [region.position.x, region.position.y, region.size.x, region.size.y];
			if (ImGui.DragInt4($"{id}", ref values[0], 1))
				property = new RectInt(values[0], values[1], values[2], values[3]);
		}
		else if (type == typeof(float))
		{
			var value = (float)property;
			if (ImGui.DragFloat($"{id}", ref value, 0.05F))
				property = value;
		}
		else if (type == typeof(int))
		{
			var value = (int)property;
			if (ImGui.DragInt($"{id}", ref value))
				property = value;
		}
		else if (type == typeof(Color))
		{
			var color = (Color)property;
			var value = new Vector4(color.r, color.g, color.b, color.a) * Color.FACTOR;
			if (ImGui.ColorButton($"{id}", value))
				ImGui.OpenPopup($"colorPicker_{id}");
			ImGui.SameLine();
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 5);
			ImGui.Text($"{id}");
			
			if (ImGui.BeginPopup($"colorPicker_{id}"))
			{
				if (ImGui.ColorPicker4($"##picker_{id}", ref value))
					property = new Color(value.x, value.y, value.z, value.w);
				
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
		colors[(int)ImGuiCol.TitleBg] = new Vector4(0.10f, 0.60f, 0.30f, 1f);
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
		style.WindowBorderSize = 1;
		style.ChildBorderSize = 0;
		style.PopupBorderSize = 0;
		style.WindowTitleAlign = new Vector2(0.5F, 0.5F);
		style.WindowMenuButtonPosition = ImGuiDir.None;
	}
}