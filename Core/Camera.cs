using XYEngine.Inputs;
using XYEngine.Resources;

namespace XYEngine;

public class Camera
{
	private static readonly Comparison<XYObject> drawOrderComparison = (a, b) => a.drawOrder.CompareTo(b.drawOrder);
	private readonly List<XYObject> objects;
	
	public Vector2 position;
	
	public float zoom
	{
		get;
		set
		{
			field = value;
			UpdateResolutionCamera();
		}
	} = 1;
	
	public Vector2 mousePosition { get; private set; }
	public Vector2 resolution { get; private set; }
	public Vector2 halfResolution { get; private set; }
	
	public Camera(List<XYObject> objects)
	{
		this.objects = objects;
		Graphics.resolutionChanged += UpdateResolutionCamera;
		
		UpdateResolutionCamera();
	}
	
	internal void Render()
	{
		var matrix = Matrix3X3.CreateTranslation(-position.ToVector2Int()) *
					 Matrix3X3.CreateOrthographic(resolution.x, resolution.y);
		
		foreach (var shader in Shader.shaders)
			shader.gProgram.SetUniform("projection", matrix);
		
		objects.Sort(drawOrderComparison);
		foreach (var obj in objects)
		{
			if (!obj.isActif || !obj.canDraw)
			
				continue;
			
			obj.BeginDraw();
			Graphics.DrawMesh(obj.mesh, obj.material);
		}
	}
	
	internal void Update()
	{
		mousePosition = position + Input.mousePosition / zoom - halfResolution;
	}
	
	private void UpdateResolutionCamera()
	{
		resolution = Graphics.resolution / zoom;
		halfResolution = resolution * 0.5F;
	}
	
	internal void Destroy()
	{
		Graphics.resolutionChanged -= UpdateResolutionCamera;
	}
}