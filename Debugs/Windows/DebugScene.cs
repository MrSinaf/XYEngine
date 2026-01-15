using System.Reflection;

namespace XYEngine.Debugs.Windows;

public class DebugScene : IDebugWindow, IDebugMainBar
{
	public bool visible { get; set; }
	public int priority => 0;
	
	private List<XYObject> objects;
	private XYObject cObject;
	
	public void OnDebugWindowRender(string name)
	{
		var scene = SceneManager.current;
		var isOpen = visible;
		if (ImGui.Begin(name, ref isOpen, ImGuiWindowFlags.MenuBar))
		{
			ShowMenuBar();
			if (ImGui.BeginTabBar("SceneTabs"))
			{
				if (ImGui.BeginTabItem("Properties"))
				{
					XYDebug.ShowObjectProperties(scene);
					ImGui.EndTabItem();
				}
				if (ImGui.BeginTabItem("Hierarchy"))
				{
					ShowHierarchy();
					ImGui.EndTabItem();
				}
				
				ImGui.EndTabBar();
			}
			
			ImGui.End();
		}
		visible = isOpen;
	}
	
	private void ShowHierarchy()
	{
		var objectsField = typeof(Scene).GetField("objects", BindingFlags.NonPublic | BindingFlags.Instance);
		objects = objectsField?.GetValue(SceneManager.current) as List<XYObject>;
		
		if (objects == null || objects.Count == 0)
		{
			ImGui.Text("Not XYObject found");
			return;
		}
		
		ImGui.Columns(2, "ObjectsColumns");
		if (ImGui.BeginChild("List"))
		{
			ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"Count: {objects.Count}");
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
		
		ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Inspector");
		ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  {cObject.GetType().Name}");
		ImGui.Separator();
		
		if (ImGui.Button("Focus"))
			Scene.current.camera.position = cObject.position;
		ImGui.SameLine();
		if (ImGui.Button("Destroy"))
			cObject.Destroy();
		var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						  .Where(m => m.DeclaringType == type && m.GetParameters().Length == 0).ToArray();
		if (methods.Length > 0)
		{
			ImGui.SameLine();
			if (ImGui.BeginCombo("##UnitFonctions", "Functions"))
			{
				foreach (var methodInfo in methods)
					if (!methodInfo.Name.StartsWith("get_") && ImGui.Selectable(methodInfo.Name))
						methodInfo.Invoke(cObject, null);
				
				ImGui.EndCombo();
			}
		}
		
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Spacing();
		
		if (cObject is IDebugProperty renderable)
			renderable.OnDebugPropertyRender();
		else
			XYDebug.ShowObjectProperties(cObject);
		
		var components = typeof(XYObject).GetField("components", BindingFlags.NonPublic | BindingFlags.Instance)
										 .GetValue(cObject) as List<Component>;
		foreach (var component in components)
		{
			if (component is IDebugProperty imGuiRenderable)
				imGuiRenderable.OnDebugPropertyRender();
			else
				XYDebug.ShowCollapsingProperties(component.GetType().Name, component);
		}
	}
	
	private static void ShowMenuBar()
	{
		if (ImGui.BeginMenuBar())
		{
			if (ImGui.MenuItem("Reload"))
				SceneManager.SetCurrentScene(SceneManager.current.GetType());
			
			if (ImGui.BeginMenu("Go to"))
			{
				foreach (var scene in Assembly.GetEntryAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Scene)))
											  .ToList())
					if (ImGui.MenuItem(scene.Name))
						SceneManager.SetCurrentScene(scene);
				
				ImGui.EndMenu();
			}
			ImGui.EndMenuBar();
		}
	}
	
	public void OnDebugMainBarRender()
	{
		if (ImGui.BeginMenu("Windows"))
		{
			if (ImGui.MenuItem("Scene"))
				visible = !visible;
			
			ImGui.EndMenu();
		}
	}
}