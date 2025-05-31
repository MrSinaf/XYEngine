namespace XYEngine.Debugs;

public interface IDebugWindow
{
	public string name { get; }
	public Vector2 size { get; }
	public bool visible { get; set; }
	public bool notFirstDraw { get; set; }
	
	public void Create();
	public void Render();
}