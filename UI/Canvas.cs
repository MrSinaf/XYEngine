using XYEngine.Resources;

namespace XYEngine.UI;

public class Canvas
{
	public readonly RootElement root = new ();
	
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
		
		root.clipArea = new RegionInt(Vector2Int.zero, Graphics.resolution);
		root.size = Graphics.resolution;
		root.UnmarkMatrixIsDirty();
	}
}