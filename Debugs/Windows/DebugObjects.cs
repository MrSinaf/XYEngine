using System.Reflection;

namespace XYEngine.Debugs.Windows;

public class DebugObjects : DebugWindow
{
	public override string name => "Objects";
	public override Vector2 size => new (600, 300);
	
	public List<XYObject> objects;
	private XYObject cObject;
	
	public override void Render()
	{
		var objectsField = typeof(Scene).GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance);
		objects = objectsField?.GetValue(SceneManager.current) as List<XYObject>;
		
		if (objects == null || objects.Count == 0)
		{
			ImGui.Text("Aucun XYObject trouvé");
			return;
		}
		
		ImGui.Columns(2, "ObjectsColumns");
		if (!notFirstDraw)
			ImGui.SetColumnWidth(0, 200);
		
		if (ImGui.BeginChild("List"))
		{
			ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Hierarchy");
			ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  Count: {objects.Count}");
			ImGui.Separator();
			
			for (var i = objects.Count - 1; i >= 0; i--)
			{
				var obj = objects[i];
				var displayName = $"{obj.name}";
				
				ImGui.PushID(i);
				if (ImGui.Selectable(displayName, cObject == obj))
				{
					cObject = obj;
				}
				ImGui.PopID();
				
				if (cObject == obj)
				{
					ImGui.SetItemDefaultFocus();
				}
			}
			ImGui.EndChild();
		}
		
		ImGui.NextColumn();
		if (cObject != null && ImGui.BeginChild("Inspector"))
		{ 
			ShowObjectInspector();
			ImGui.EndChild();
		}
	}
	
	private void ShowObjectInspector()
	{
		var type = cObject.GetType();
		var properties = type.GetProperties();
		var fields = type.GetFields();
		
		
		ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Inspector");
		ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  {cObject.GetType().Name}");
		ImGui.Separator();
		
		if (ImGui.Button("Focus"))
			Scene.current.camera.position = cObject.position;
		ImGui.SameLine();
		if (ImGui.Button("Destroy"))
			cObject.Destroy();
		
		ImGui.Separator();
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
				p.SetValue(cObject, XYDebug.GetInput(p.Name, p.GetValue(cObject)));
				ImGui.NextColumn();
			}
		}
		
		foreach (var f in fields)
		{
			if (XYDebug.IsCompatibleInput(f.FieldType))
			{
				ImGui.AlignTextToFramePadding();
				ImGui.Text(f.Name);
				ImGui.NextColumn();
				f.SetValue(cObject, XYDebug.GetInput(f.Name, f.GetValue(cObject)));
				ImGui.NextColumn();
			}
		}
	}
}