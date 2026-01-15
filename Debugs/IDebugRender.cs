namespace XYEngine.Debugs;

public interface IDebugRender : IImGuiRenderable
{
	public void OnDebugRender(string name);
}