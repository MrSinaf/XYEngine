using XYEngine.Inputs;
using XYEngine.Rendering;
using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI;

public class Canvas
{
	internal readonly RootElement root;
	private readonly Mesh mesh;
	
	public Vector2 resolution { get; private set; }
	public Vector2 halfResolution { get; private set; }
	public RenderTexture renderTexture { get; private set; }
	public Vector2Int mousePosition { get; private set; }
	
	public Material material;
	private Matrix3X3 matrix;
	
	public Canvas()
	{
		Graphics.resolutionChanged += UpdateResolution;
		root = new RootElement(this);
		
		mesh = MeshFactory.CreateQuad(new Vector2(resolution.x, resolution.y)).Apply();
		material = new Material(Vault.GetEmbeddedAsset<Shader>("canvas.shader"));
		UpdateResolution();
	}
	
	internal void Render()
	{
		mousePosition = Input.mousePosition.ToVector2Int();
		foreach (var shader in Shader.shaders)
		{
			shader.gProgram.SetUniform("projection", matrix);
			shader.gProgram.SetUniform("view", Matrix3X3.Identity());
		}
		
		renderTexture.Bind();
		{
			root.Draw();
		}
		renderTexture.Unbind();
		
		Graphics.DrawMesh(mesh, material);
	}
	
	public Vector2Int ScreenToCanvasPosition(Vector2 screenPosition) => screenPosition.ToVector2Int();
	public Vector2Int CanvasToScreenPosition(Vector2 canvasPosition) => canvasPosition.ToVector2Int();
	
	internal void Destroy()
	{
		Graphics.resolutionChanged -= UpdateResolution;
		root.Destroy();
	}
	
	private void UpdateResolution()
	{
		resolution = Graphics.resolution;
		halfResolution = resolution * 0.5F;
		
		matrix = Matrix3X3.CreateOrthographic(resolution.x, resolution.y, false);
		root.UpdateSize(resolution.ToVector2Int());
		
		mesh?.SetQuadPosition(new Rect(-halfResolution, resolution)).Apply();
		renderTexture?.Dispose();
		renderTexture = new RenderTexture((uint)resolution.x, (uint)resolution.y, 
										  new Texture2DConfig(TextureWrap.ClampToEdge, TextureMag.Linear));
		material?.SetProperty("MainTex", renderTexture);
	}
}