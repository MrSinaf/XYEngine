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
	
	private Vector2 halfResolution;
	
	public Camera(List<ObjectBehaviour> objects)
	{
		this.objects = objects;
		Graphics.resolutionChanged += OnResolutionChanged;
		
		OnResolutionChanged();
	}
	
	internal void Render()
	{
		var matrix = Matrix3X3.CreateTranslation(-position.ToVector2Int()) *
					 Matrix3X3.CreateOrthographic(resolution.x, resolution.y);
		
		foreach (var shader in Shader.shaders)
			shader.gProgram.SetUniform("projection", matrix);
		
		foreach (var obj in objects)
		{
			if (!obj.isActif || !obj.canDraw)
				continue;
			
			obj.BeginDraw();
			Graphics.DrawMesh(obj.mesh, obj.material);
		}
	}
	
	internal void Update() => mousePosition = position + (Input.mousePosition - halfResolution) / zoom;
	
	private void OnResolutionChanged()
	{
		UpdateResolutionCamera();
		halfResolution = Graphics.resolution.ToVector2() * 0.5F;
	}
	
	private void UpdateResolutionCamera()
	{
		resolution = Graphics.resolution / zoom;
	}
	
	internal void Destroy() => Graphics.resolutionChanged -= OnResolutionChanged;
}