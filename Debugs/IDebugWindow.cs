namespace XYEngine.Debugs;

public interface IDebugWindow : IImGuiRenderable
{
	public bool visible { get; }
	public void OnDebugWindowRender(string name);
}