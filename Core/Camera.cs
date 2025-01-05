using XYEngine.Resources;

namespace XYEngine;

public class Camera(List<ObjectBehaviour> objects)
{
	public Vector2 position;
	public float zoom = 1;
	
	internal void Render()
	{
		var matrix = Matrix3X3.CreateTranslation(-position.ToVector2Int()) *
					 Matrix3X3.CreateOrthographic(Graphics.resolution.x, Graphics.resolution.y) *
					 Matrix3X3.CreateScale(new Vector2(zoom));
		
		foreach (var shader in Shader.shaders)
			shader.gProgram.SetUniform("projection", matrix);
		
		foreach (var obj in objects)
		{
			if (!obj.isActif || !obj.canDraw)
				return;
			
			obj.BeginDraw();
			Graphics.DrawMesh(obj.mesh, obj.material);
		}
	}
}