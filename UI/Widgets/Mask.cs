using Silk.NET.OpenGL;
using static XYEngine.Graphics;

namespace XYEngine.UI.Widgets;

public class Mask : UIElement
{
	public bool masked = true;
	
	protected override void OnBeginDraw()
	{
		if (!active)
			return;
		
		if (masked)
		{
			gl.Scissor(realPosition.x, realPosition.y, (uint)scaledSize.x, (uint)scaledSize.y);
			gl.Enable(GLEnum.ScissorTest);
		}
	}
	
	protected override void OnEndDraw() => gl.Disable(GLEnum.ScissorTest);
}