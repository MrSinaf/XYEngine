namespace XYEngine.Debugs.Windows;

public class DebugObject(object asset) : IDebugWindow
{
	public bool visible { get; set; } = true;
	
	public void OnDebugWindowRender(string name)
	{
		var isOpen = visible;
		if (ImGui.Begin(name, ref isOpen))
		{
			var windowSize = ImGui.GetWindowSize();
			if (windowSize.x < 200)
				windowSize.x = 200;
			if (windowSize.y < 200)
				windowSize.y = 200;
			ImGui.SetWindowSize(windowSize);
			
			XYDebug.ShowObjectProperties(asset);
			
			ImGui.End();
		}
		
		if (!isOpen)
			XYDebug.Remove(name);
	}
}