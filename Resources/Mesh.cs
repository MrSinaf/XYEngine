using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Mesh : IDisposable
{
	public GVertexArray vertexArray { get; private set; }
	
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
		
		if (isValid)
		{
			vertexBuffer.Dispose();
			indexBuffer.Dispose();
			vertexArray.Dispose();
			isValid = false;
		}
		
		GCommandQueue.Enqueue(() =>
		{
			vertexBuffer = GBuffer<byte>.Create(BufferType.VertexBuffer, MakeVertexDataBlob(layout));
			indexBuffer = GBuffer<uint>.Create(BufferType.ElementsBuffer, indices);
			vertexArray = new GVertexArray(layout, vertexBuffer, indexBuffer);
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
		var spanBuffer = new Span<byte>(buffer);
		
		var position = 0;
		for (var i = 0; i < vertices.Length; i++)
		{
			var vertice = vertices[i];
			Copy(BitConverter.GetBytes(vertice.x), ref spanBuffer);
			Copy(BitConverter.GetBytes(vertice.y), ref spanBuffer);
			
			if (hasColors)
			{
				var color = colors[i];
				Copy(BitConverter.GetBytes(color.r * Color.FACTOR), ref spanBuffer);
				Copy(BitConverter.GetBytes(color.g * Color.FACTOR), ref spanBuffer);
				Copy(BitConverter.GetBytes(color.b * Color.FACTOR), ref spanBuffer);
				Copy(BitConverter.GetBytes(color.a * Color.FACTOR), ref spanBuffer);
			}
			
			if (hasUvs)
			{
				var uv = uvs[i];
				Copy(BitConverter.GetBytes(uv.x), ref spanBuffer);
				Copy(BitConverter.GetBytes(uv.y), ref spanBuffer);
			}
		}
		
		return buffer;
		
		void Copy(byte[] source, ref Span<byte> spanBuffer)
		{
			if (position + source.Length > spanBuffer.Length)
				throw new InvalidOperationException("Write overflow in vertex data blob generation.");
			
			source.CopyTo(spanBuffer.Slice(position, source.Length));
			position += source.Length;
		}
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
		
		vertexBuffer?.Dispose();
		indexBuffer?.Dispose();
		vertexArray?.Dispose();
		isDisposed = true;
		
		GC.SuppressFinalize(this);
	}
	
	~Mesh() => Dispose();
}