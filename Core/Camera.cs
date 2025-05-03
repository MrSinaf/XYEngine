using XYEngine.Inputs;
using XYEngine.Resources;

namespace XYEngine;

public class Camera
{
	private readonly List<ObjectBehaviour> objects;
	
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
	
	public Camera(List<ObjectBehaviour> objects)
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
		
		var objectsToDraw = objects.Where(obj => obj.isActif && obj.canDraw)
								   .OrderBy(obj => obj.drawOrder)
								   .ToList();
		
		foreach (var obj in objectsToDraw)
		{
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