using Silk.NET.OpenGL;
using XYEngine.Resources;
using Shader = XYEngine.Resources.Shader;

namespace XYEngine;

public static class Graphics
{
	public static GL gl { get; private set; }
	public static Vector2Int resolution { get; private set; }
	
	public static event Action resolutionChanged = () => { };
	
	internal static void Init(GL gl)
	{
		Graphics.gl = gl;
		
		gl.Enable(EnableCap.Blend);
		gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
	}
	
	internal static void SetBackgroundColor(Color color)
		=> gl.ClearColor(color.r * Color.FACTOR, color.g * Color.FACTOR, color.b * Color.FACTOR, 1);
	
	internal static void BeginDraw()
	{
		gl.Clear(ClearBufferMask.ColorBufferBit);
		
		foreach (var shader in Shader.shaders)
		{
			shader.gProgram.SetUniform("resolution", resolution);
			shader.gProgram.SetUniform("time", Time.total);
		}
	}
	
	internal static void DrawMesh(Mesh mesh, Material material)
	{
		material.ApplyProperties();
		
		gl.BindVertexArray(mesh.vao.handle);
		unsafe
		{
			gl.DrawElements((PrimitiveType)material.topology, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt,
							(void*)0);
		}
		
		gl.BindVertexArray(0);
	}
	
	internal static void EndDraw() { }
	
	public static void Viewport(Vector2Int size)
	{
		if (size.x == 0 || size.y == 0)
			return;
		
		gl.Viewport(0, 0, (uint)size.x, (uint)size.y);
		resolution = size;
		resolutionChanged();
	}
}