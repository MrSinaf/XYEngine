using XYEngine.Inputs;
using XYEngine.UI;

namespace XYEngine.Debugs.Windows;

internal class DebugCanvas : IDebugWindow, IDebugMainBar
{
	private static UIElement cElement;
	private static int cElementIndex;
	private static int elementIndex;
	
	public int priority => 5;
	public bool visible { get; set; }
	
	public void OnDebugWindowRender(string name)
	{
		var isOpen = visible;
		if (ImGui.Begin(name, ref isOpen))
		{
			var root = SceneManager.current.canvas.root;
			
			ImGui.Columns(2, "CanvasColumns");
			elementIndex = 0;
			ImGui.BeginChild("Tree");
			if (ImGui.TreeNodeEx("Root", ImGuiTreeNodeFlags.None))
			{
				foreach (var children in root.childrenArray)
					ShowChildrenTree(children);
				
				ImGui.TreePop();
			}
			ImGui.EndChild();
			
			ImGui.NextColumn();
			
			ImGui.BeginChild("Inspector");
			ShowElementInspector();
			DrawSelectedElementArea();
			ImGui.EndChild();
			ImGui.End();
		}
		visible = isOpen;
	}
	
	public void OnDebugMainBarRender()
	{
		if (ImGui.BeginMenu("Windows"))
		{
			if (ImGui.MenuItem("Canvas"))
				visible = !visible;
			
			ImGui.EndMenu();
		}
	}
	
	private static void ShowElementInspector()
	{
		if (cElement == null)
			return;
		
		var type = cElement.GetType();
		var properties = type.GetProperties();
		
		ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Inspector");
		ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  {cElement.GetType().Name}");
		ImGui.Separator();
		ImGui.Spacing();
		
		XYDebug.ShowValue("parent", cElement.parent.GetType());
		XYDebug.ShowValue("mouseIsOver", cElement.ContainsPoint(Input.mousePosition));
		XYDebug.ShowValue("nChild", cElement.nChild);
		XYDebug.ShowValue("isObservable", cElement.isObservable);
		ImGui.Spacing();
		
		var currentType = type;
		while (currentType != null && currentType != typeof(object))
		{
			if (type == currentType)
				DrawProperty(type);
			else
			{
				ImGui.Spacing();
				ImGui.Columns(1);
				if (ImGui.CollapsingHeader($"{currentType.Name}##{cElement.GetHashCode()}"))
					DrawProperty(currentType);
			}
			
			currentType = currentType.BaseType;
		}
		
		void DrawProperty(Type type)
		{
			foreach (var p in properties)
			{
				if (!p.CanRead || p.DeclaringType != type)
					continue;
				
				if (p.CanWrite && XYDebug.IsCompatibleInput(p.PropertyType))
				{
					p.SetValue(cElement, XYDebug.GetInput(p.Name, p.GetValue(cElement)));
				}
			}
		}
	}
	
	private static void ShowChildrenTree(UIElement element)
	{
		if (element == cElement)
			ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.7F, 0, 1));
		ImGui.PushID(elementIndex++);
		var nodeOpen = ImGui.TreeNodeEx(
			string.IsNullOrEmpty(element.name) ? element.GetType().Name : $"{element.name} [{element.GetType().Name}]",
			element.childrenArray.Length == 0
				? ImGuiTreeNodeFlags.Leaf
				: ImGuiTreeNodeFlags.None | ImGuiTreeNodeFlags.OpenOnArrow);
		ImGui.PopID();
		
		if (element == cElement)
			ImGui.PopStyleColor();
		
		if (ImGui.IsItemClicked())
		{
			cElement = element;
			cElementIndex = elementIndex;
		}
		
		if (nodeOpen)
		{
			foreach (var child in element.childrenArray)
				ShowChildrenTree(child);
			
			ImGui.TreePop();
		}
	}
	
	private static void DrawSelectedElementArea()
	{
		if (cElement == null)
			return;
		
		var drawList = ImGui.GetForegroundDrawList();
		
		var clipMin = new Vector2(cElement.clipArea.position00.x, GameWindow.size.y - cElement.clipArea.position11.y);
		var clipMax = new Vector2(cElement.clipArea.position11.x, GameWindow.size.y - cElement.clipArea.position00.y);
		drawList.AddRect(clipMin, clipMax, ImGui.GetColorU32(new Vector4(0, 1, 1, 0.2F)), 0, ImDrawFlags.None, 1);
		
		var worldPivotPosition = cElement.GetWorldPivotPosition();
		var pivotPoint = new Vector2(
			worldPivotPosition.x,
			GameWindow.size.y - worldPivotPosition.y
		);
		
		cElement.GetBoundingCorners(out var topLeft, out var topRight, out var bottomLeft, out var bottomRight);
		var points = new[]
		{
			new Vector2(topLeft.x, GameWindow.size.y - topLeft.y),
			new Vector2(bottomLeft.x, GameWindow.size.y - bottomLeft.y),
			new Vector2(bottomRight.x, GameWindow.size.y - bottomRight.y),
			new Vector2(topRight.x, GameWindow.size.y - topRight.y)
		};
		drawList.AddPolyline(ref points[0], 4, ImGui.GetColorU32(new Vector4(1, 0, 0, 1)), ImDrawFlags.Closed, 1);
		drawList.AddCircleFilled(pivotPoint, 2.0f, ImGui.GetColorU32(new Vector4(0, 1, 0, 1)));
	}
}