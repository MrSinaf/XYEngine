using Silk.NET.OpenGL;
using static XYEngine.Graphics;

namespace XYEngine.Rendering;

public class GVertexArrayObject : IDisposable
{
	public readonly uint handle;
	
	public bool isDisposed { get; protected set; }
	
	public GVertexArrayObject(VertexLayout layout, GBuffer<byte> vertices, GBuffer<uint> indices)
	{
		handle = gl.GenVertexArray();
		if (handle == 0)
			throw new Exception("Failed to generate Vertex Array Object (VAO)");
		
		gl.BindVertexArray(handle);
		
		gl.BindBuffer(GLEnum.ElementArrayBuffer, indices.handle);
		gl.BindBuffer(GLEnum.ArrayBuffer, vertices.handle);
		
		foreach (var element in layout.elements)
		{
			gl.EnableVertexAttribArray(element.semantic);
			
			var offset = element.offset;
			unsafe
			{
				if (element.type == VertexType.Float)
					gl.VertexAttribPointer(element.semantic, element.number, (GLEnum)element.type, element.normalized, layout.size, (void*)offset);
				else
					gl.VertexAttribIPointer(element.semantic, element.number, (GLEnum)element.type, layout.size, (void*)offset);
			}
		}
		
		gl.BindVertexArray(0);
		
		gl.BindBuffer(GLEnum.ArrayBuffer, 0);
		gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
	}
	
	public void Dispose()
	{
		if (isDisposed)
			return;
		
		if (handle == 0)
			throw new Exception("Failed to generate Vertex Array Object (VAO)");
		
		MainThreadQueue.EnqueueRenderer(() => gl.DeleteVertexArray(handle));
		isDisposed = true;
		
		GC.SuppressFinalize(this);
	}
}