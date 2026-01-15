﻿using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Mesh : IDisposable
{
	public GVertexArrayObject vao { get; private set; }
	
	public uint[] indices;
	public Vector2[] vertices;
	public Vector2[] uvs;
	public Color[] colors;
	
	public Rect bounds { get; private set; }
	
	public bool isValid;
	public bool hasColors => colors?.Length > 0;
	public bool hasUvs => uvs?.Length > 0;
	
	private GBuffer<byte> vertexBuffer;
	private GBuffer<uint> indexBuffer;
	private bool isDisposed;
	
	public Mesh Apply()
	{
		CalculateBounds();
		var layout = GetVertexLayout();
		
		MainThreadQueue.EnqueueRenderer(() =>
		{
			if (isValid)
			{
				vertexBuffer.Dispose();
				indexBuffer.Dispose();
				vao.Dispose();
				isValid = false;
			}
			
			vertexBuffer = GBuffer<byte>.Create(BufferType.VertexBuffer, MakeVertexDataBlob(layout));
			indexBuffer = GBuffer<uint>.Create(BufferType.ElementsBuffer, indices);
			vao = new GVertexArrayObject(layout, vertexBuffer, indexBuffer);
			isValid = true;
		});
		
		return this;
	}
	
	private void CalculateBounds()
	{
		if (vertices == null || vertices.Length == 0)
		{
			bounds = Rect.zero;
			return;
		}
		
		var min = vertices[0];
		var max = vertices[0];
		
		foreach (var vertex in vertices)
		{
			if (vertex.x < min.x) min.x = vertex.x;
			if (vertex.y < min.y) min.y = vertex.y;
			
			if (vertex.x > max.x) max.x = vertex.x;
			if (vertex.y > max.y) max.y = vertex.y;
		}
		
		bounds = new Rect(min, max - min);
	}
	
	private byte[] MakeVertexDataBlob(VertexLayout layout)
	{
		var buffer = new byte[layout.size * vertices.Length];
		
		var position = 0;
		for (var i = 0; i < vertices.Length; i++)
		{
			var vertice = vertices[i];
			
			if (position + 4 > buffer.Length)
				throw new InvalidOperationException("Write overflow in vertex data blob generation.");
			
			BitConverter.TryWriteBytes(buffer.AsSpan(position), vertice.x);
			position += 4;
			
			if (position + 4 > buffer.Length)
				throw new InvalidOperationException("Write overflow in vertex data blob generation.");
			
			BitConverter.TryWriteBytes(buffer.AsSpan(position), vertice.y);
			position += 4;
			
			if (hasColors)
			{
				var color = colors[i];
				
				if (position + 4 > buffer.Length)
					throw new InvalidOperationException("Write overflow in vertex data blob generation.");
				
				BitConverter.TryWriteBytes(buffer.AsSpan(position), color.r * Color.FACTOR);
				position += 4;
				
				if (position + 4 > buffer.Length)
					throw new InvalidOperationException("Write overflow in vertex data blob generation.");
				
				BitConverter.TryWriteBytes(buffer.AsSpan(position), color.g * Color.FACTOR);
				position += 4;
				
				if (position + 4 > buffer.Length)
					throw new InvalidOperationException("Write overflow in vertex data blob generation.");
				
				BitConverter.TryWriteBytes(buffer.AsSpan(position), color.b * Color.FACTOR);
				position += 4;
				
				if (position + 4 > buffer.Length)
					throw new InvalidOperationException("Write overflow in vertex data blob generation.");
				
				BitConverter.TryWriteBytes(buffer.AsSpan(position), color.a * Color.FACTOR);
				position += 4;
			}
			
			if (hasUvs)
			{
				var uv = uvs[i];
				
				if (position + 4 > buffer.Length)
					throw new InvalidOperationException("Write overflow in vertex data blob generation.");
				
				BitConverter.TryWriteBytes(buffer.AsSpan(position), uv.x);
				position += 4;
				
				if (position + 4 > buffer.Length)
					throw new InvalidOperationException("Write overflow in vertex data blob generation.");
				
				BitConverter.TryWriteBytes(buffer.AsSpan(position), uv.y);
				position += 4;
			}
		}
		
		return buffer;
	}
	
	
	private VertexLayout GetVertexLayout()
	{
		var elements = new List<VertexLayout.Element> { new (VertexSemantic.Position, VertexType.Float, 2) }; // L'élément position est obligé d'exister.
		
		if (hasColors)
			elements.Add(new VertexLayout.Element(VertexSemantic.Color, VertexType.Float, 4));
		
		if (hasUvs)
			elements.Add(new VertexLayout.Element(VertexSemantic.TexCoord0, VertexType.Float, 2));
		
		return new VertexLayout(elements.ToArray());
	}
	
	public void Dispose()
	{
		if (isDisposed)
			return;
		
		MainThreadQueue.EnqueueRenderer(() =>
		{
			if (isValid)
			{
				vertexBuffer.Dispose();
				indexBuffer.Dispose();
				vao.Dispose();
				isValid = false;
			}
		});
		isDisposed = true;
		
		GC.SuppressFinalize(this);
	}
	
	~Mesh() => Dispose();
}