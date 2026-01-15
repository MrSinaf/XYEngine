using Silk.NET.OpenGL;
using static XYEngine.Graphics;

namespace XYEngine.UI.Widgets;

public class Mask : UIElement
{
	private readonly Stack<Mask> masks = [];
	public bool masked = true;
	
	protected override void OnBeginDraw()
	{
		masks.Push(this);
		ActiveMask();
	}
	
	protected override void OnEndDraw()
	{
		masks.Pop();
		gl.Disable(GLEnum.ScissorTest);
		
		if (masks.TryPeek(out var mask))
			mask.ActiveMask();
	}
	
	private void ActiveMask()
	{
		if (masked)
		{
			gl.Scissor(realPosition.x, realPosition.y, (uint)scaledSize.x, (uint)scaledSize.y);
			gl.Enable(GLEnum.ScissorTest);
		}
	}
}