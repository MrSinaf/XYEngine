namespace XYEngine.Debugs;

public interface IDebugMainBar : IImGuiRenderable
{
	public int priority { get; }
	public void OnDebugMainBarRender();
}