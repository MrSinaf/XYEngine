using XYEngine.Inputs;
using XYEngine.UI;

namespace XYEngine.Debugs.Windows;

internal class DebugCanvas : DebugWindow
{
	public override string name => "Canvas";
	public override Vector2 size => new (650, 300);
	public override ImGuiWindowFlags flags => ImGuiWindowFlags.None;
	
	private static UIElement cElement;
	private static int elementIndex;
	
	public void Create() { }
	
	public override void Render()
	{
		var root = SceneManager.current.canvas.root;
		
		elementIndex = 0;
		ImGui.Columns(2, "CanvasColumns");
		if (!notFirstDraw)
			ImGui.SetColumnWidth(0, 200);
		
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
		ImGui.EndChild();
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
		
		XYDebug.ShowProperty("parent", cElement.parent.GetType());
		XYDebug.ShowProperty("mouseIsOver", cElement.ContainsPoint(Input.mousePosition));
		XYDebug.ShowProperty("nChild", cElement.nChild);
		XYDebug.ShowProperty("isObservable", cElement.isObservable);
		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.Spacing();
		
		ImGui.Columns(2, "Params", false);
		ImGui.SetColumnWidth(0, 150);
		foreach (var p in properties)
		{
			if (!p.CanRead)
				continue;
			
			if (p.CanWrite && XYDebug.IsCompatibleInput(p.PropertyType))
			{
				ImGui.AlignTextToFramePadding();
				ImGui.Text(p.Name);
				ImGui.NextColumn();
				p.SetValue(cElement, XYDebug.GetInput(p.Name, p.GetValue(cElement)));
				ImGui.NextColumn();
			}
		}
		
		var drawList = ImGui.GetForegroundDrawList();
		
		var clipMin = new Vector2(cElement.clipArea.position00.x, GameWindow.size.y - cElement.clipArea.position11.y);
		var clipMax = new Vector2(cElement.clipArea.position11.x, GameWindow.size.y - cElement.clipArea.position00.y);
		drawList.AddRect(clipMin, clipMax, ImGui.GetColorU32(new Vector4(0, 1, 1, 0.2F)), 0, ImDrawFlags.None, 1);
		
		var itemPos = new Vector2(cElement.realPosition.x, GameWindow.size.y - cElement.realPosition.y - cElement.scaledSize.y);
		var itemSize = cElement.scaledSize;
		var pivotPoint = new Vector2(itemPos.x + cElement.pivot.x * itemSize.x, itemPos.y + -cElement.pivot.y * itemSize.y + itemSize.y);
		
		if (cElement.rotation == 0)
		{
			drawList.AddRect(itemPos, itemPos + itemSize, ImGui.GetColorU32(new Vector4(1, 0, 0, 1)), 0, ImDrawFlags.None, 1);
		}
		else
		{
			var radians = -cElement.rotation * (MathF.PI / 180f);
			var points = new Vector2[]
			{
				new (itemPos.x, itemPos.y),
				new (itemPos.x + itemSize.x, itemPos.y),
				new (itemPos.x + itemSize.x, itemPos.y + itemSize.y),
				new (itemPos.x, itemPos.y + itemSize.y)
			};
			
			for (var i = 0; i < 4; i++)
			{
				var dx = points[i].x - pivotPoint.x;
				var dy = points[i].y - pivotPoint.y;
				points[i] = new Vector2(pivotPoint.x + dx * MathF.Cos(radians) - dy * MathF.Sin(radians),
										pivotPoint.y + dx * MathF.Sin(radians) + dy * MathF.Cos(radians));
			}
			
			drawList.AddPolyline(ref points[0], 4, ImGui.GetColorU32(new Vector4(1, 0, 0, 1)), ImDrawFlags.Closed, 1);
		}
		drawList.AddCircleFilled(pivotPoint, 1.5f, ImGui.GetColorU32(new Vector4(0, 1, 0, 1)));
	}
	
	private static void ShowChildrenTree(UIElement element)
	{
		if (element == cElement)
			ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.7F, 0, 1));
		ImGui.PushID(elementIndex++);
		var nodeOpen = ImGui.TreeNodeEx(string.IsNullOrEmpty(element.name) ? element.GetType().Name : $"{element.name} [{element.GetType().Name}]",
										element.childrenArray.Length == 0 ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None | ImGuiTreeNodeFlags.OpenOnArrow);
		ImGui.PopID();
		
		if (element == cElement)
			ImGui.PopStyleColor();
		
		if (ImGui.IsItemClicked())
			cElement = element;
		
		if (nodeOpen)
		{
			foreach (var child in element.childrenArray)
				ShowChildrenTree(child);
			
			ImGui.TreePop();
		}
	}
}