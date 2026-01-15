using XYEngine.Debugs;
using XYEngine.Rendering;
using XYEngine.Resources;
using XYEngine.Utils;
using Shader = XYEngine.Resources.Shader;

namespace XYEngine;

public class Camera : IDebugProperty
{
	private static readonly Comparison<XYObject> drawOrderComparison = (a, b) => a.drawOrder.CompareTo(b.drawOrder);
	private readonly Mesh mesh;
	
	public Vector2 position;
	
	public float zoom
	{
		get;
		set
		{
			field = float.Clamp(value, 0.00001F, 1000000F);
			UpdateZoom();
		}
	} = 1;
	
	public Vector2 resolution { get; private set; }
	public Vector2 halfResolution { get; private set; }
	public RenderTexture renderTexture { get; private set; }
	
	public Material material;
	
	public Camera()
	{
		Graphics.resolutionChanged += UpdateResolution;
		
		mesh = MeshFactory.CreateQuad(new Vector2(resolution.x, resolution.y)).Apply();
		material = new Material(Vault.GetEmbeddedAsset<Shader>("camera.shader"));
		UpdateResolution();
	}
	
	internal void Render(List<XYObject> objects)
	{
		var projectionMatrix = Matrix3X3.CreateOrthographic(resolution.x, resolution.y);
		var viewMatrix = Matrix3X3.CreateTranslation(-position);
		
		foreach (var shader in Shader.shaders)
		{
			shader.gProgram.SetUniform("projection", projectionMatrix);
			shader.gProgram.SetUniform("view", viewMatrix);
		}
		
		objects.Sort(drawOrderComparison);
		
		renderTexture.Bind();
		{
			foreach (var obj in objects)
			{
				if (!obj.isActif || !obj.canDraw)
					continue;
				
				obj.BeginDraw();
				Graphics.DrawMesh(obj.mesh, obj.material);
			}
		}
		renderTexture.Unbind();
		
		Graphics.DrawMesh(mesh, material);
	}
	
	public Vector2 ScreenToWorldPosition(Vector2 screenPosition) => position + screenPosition / zoom - halfResolution;
	
	public Vector2 WorldToScreenPosition(Vector2 worldPosition)
		=> (worldPosition - position) * zoom + halfResolution * zoom;
	
	private void UpdateZoom()
	{
		resolution = Graphics.resolution / zoom;
		halfResolution = resolution * 0.5F;
		
		mesh?.SetQuadPosition(new Rect(-halfResolution, resolution)).Apply();
	}
	
	private void UpdateResolution()
	{
		UpdateZoom();
		
		renderTexture?.Dispose();
		renderTexture = new RenderTexture((uint)Graphics.resolution.x, (uint)Graphics.resolution.y,
										  new Texture2DConfig(TextureWrap.ClampToEdge, TextureMag.Linear));
		material?.SetProperty("MainTex", renderTexture);
	}
	
	internal void Destroy()
	{
		Graphics.resolutionChanged -= UpdateResolution;
		renderTexture?.Dispose();
	}
	
	void IDebugProperty.OnDebugPropertyRender()
	{
		XYDebug.ShowObjectProperties(this, false);
	}
}