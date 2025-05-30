using XYEngine.Resources;

namespace XYEngine.UI;

public class Canvas
{
	internal readonly RootElement root = new ();
	
	private Matrix3X3 matrix;
	
	public Canvas()
	{
		ApplyResolution();
		Graphics.resolutionChanged += ApplyResolution;
	}
	
	internal void Render()
	{
		foreach (var shader in Shader.shaders)
			shader.gProgram.SetUniform("projection", matrix);
		
		root.Draw();
	}
	
	internal void Destroy()
	{
		Graphics.resolutionChanged -= ApplyResolution;
		root.Destroy();
	}
	
	private void ApplyResolution()
	{
		matrix = Matrix3X3.CreateOrthographic(Graphics.resolution.x, Graphics.resolution.y, false);
		root.UpdateSize(Graphics.resolution);
	}
}