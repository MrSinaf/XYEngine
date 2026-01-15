namespace XYEngine.Debugs;

public abstract class DebugWindow
{
	public virtual string menuItem => "Windows";
	public abstract string name { get; }
	public abstract Vector2 size { get; }
	
	public virtual ImGuiWindowFlags flags { get; set; } = ImGuiWindowFlags.None;
	
	public bool notFirstDraw { internal get; set; }
	public bool visible;
	
	public abstract void Render();
}