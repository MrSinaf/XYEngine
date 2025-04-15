using System.Numerics;
using ImGuiNET;
using XYEngine.Inputs;
using XYEngine.UI;

namespace XYEngine.Debugs;

internal static class DebugCanvas
{
	internal static bool showCanvas;
	private static UIElement cElement;
	
	internal static void Render()
	{
		if (showCanvas)
			DrawUITree();
	}
	
	private static void DrawUITree()
	{
		var canvas = SceneManager.current.canvas;
		var root = canvas.root;
		ImGui.Begin("Canvas", ref showCanvas);
		ImGui.Columns(2, "CanvasColumns");
		ImGui.BeginChild("Tree");
		if (ImGui.TreeNodeEx("Root", ImGuiTreeNodeFlags.None))
		{
			foreach (var children in root.childrenArray)
				ShowChildrenTree(children);
			
			ImGui.TreePop();
		}
		ImGui.EndChild();
		ImGui.NextColumn();
		ImGui.BeginChild("Properties");
		ImGui.TextColored(new Vector4(0, 1, 0, 0.8F), "Inspector");
		
		if (cElement is { isDestroyed: false })
		{
			var nChild = cElement.nChild;
			ImGui.TextColored(new Vector4(1, 1, 1, 0.3F),
							  $"{cElement.GetType().Name} | {(nChild > 1 ? $"{nChild} children" : nChild > 0 ? $"{nChild} child" : "no child")}" +
							  $"{(cElement.ContainsPoint(Input.mousePosition) ? " | mouse is hover" : "")}");
			ImGui.Separator();
			
			ImGui.Columns(2, "Params");
			var type = cElement.GetType();
			var properties = type.GetProperties();
			foreach (var p in properties)
			{
				if (!p.CanRead)
					continue;
				
				if (p.PropertyType == typeof(string))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var value = ((string)p.GetValue(cElement)).Replace("\n", @"\n");
					if (ImGui.InputText($"##{p.Name}", ref value, 9999, p.CanWrite ? ImGuiInputTextFlags.None : ImGuiInputTextFlags.ReadOnly))
						p.SetValue(cElement, value.Replace(@"\n", "\n"));
					ImGui.NextColumn();
				}
				else if (p.PropertyType == typeof(bool))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var value = (bool)p.GetValue(cElement);
					if (ImGui.Checkbox($"##{p.Name}", ref value) && p.CanWrite)
						p.SetValue(cElement, value);
					ImGui.NextColumn();
				}
				else if (p.PropertyType == typeof(Vector2))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var value = XYDebug.ToSystem((Vector2)p.GetValue(cElement));
					if (ImGui.DragFloat2($"##{p.Name}", ref value, 0.1F) && p.CanWrite)
						p.SetValue(cElement, XYDebug.ToXY(value));
					ImGui.NextColumn();
				}
				else if (p.PropertyType == typeof(Vector2Int))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var vector = (Vector2Int)p.GetValue(cElement);
					int[] values = [vector.x, vector.y];
					if (ImGui.DragInt2($"##{p.Name}", ref values[0], 1) && p.CanWrite)
						p.SetValue(cElement, new Vector2Int(values[0], values[1]));
					ImGui.NextColumn();
				}
				else if (p.PropertyType == typeof(RegionInt))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var region = (RegionInt)p.GetValue(cElement);
					int[] values = [region.position00.x, region.position00.y, region.position11.x, region.position11.y];
					if (ImGui.DragInt4($"##{p.Name}", ref values[0], 1) && p.CanWrite)
						p.SetValue(cElement, new RegionInt(values[0], values[1], values[2], values[3]));
					ImGui.NextColumn();
				}
				else if (p.PropertyType == typeof(float))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var value = (float)p.GetValue(cElement);
					if (ImGui.DragFloat($"##{p.Name}", ref value, 0.05F) && p.CanWrite)
						p.SetValue(cElement, value);
					ImGui.NextColumn();
				}
				else if (p.PropertyType == typeof(int))
				{
					ImGui.AlignTextToFramePadding();
					ImGui.Text(p.Name);
					ImGui.NextColumn();
					var value = (int)p.GetValue(cElement);
					if (ImGui.DragInt($"##{p.Name}", ref value) && p.CanWrite)
						p.SetValue(cElement, value);
					ImGui.NextColumn();
				}
			}
			
			// TODO > Ne prend pas en charge la rotation (；′⌒`)
			var itemPos = new Vector2(cElement.realPosition.x, GameWindow.size.y - cElement.realPosition.y - cElement.scaledSize.y);
			var itemSize = new Vector2(cElement.scaledSize.x, cElement.scaledSize.y);
			var drawList = ImGui.GetForegroundDrawList();
			drawList.AddRect(XYDebug.ToSystem(itemPos), XYDebug.ToSystem(itemPos + itemSize), ImGui.GetColorU32(new Vector4(1, 0, 0, 1)), 0, ImDrawFlags.None, 1);
		}
		
		ImGui.EndChild();
		ImGui.End();
		
		void ShowChildrenTree(UIElement element)
		{
			if (element == cElement)
				ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.7F, 0, 1));
			var nodeOpen = ImGui.TreeNodeEx(string.IsNullOrEmpty(element.name) ? element.GetType().Name : $"{element.name} [{element.GetType().Name}]",
											element.childrenArray.Length == 0 ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None | ImGuiTreeNodeFlags.OpenOnArrow);
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
}