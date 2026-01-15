namespace XYEngine.Rendering;

public class VertexLayout
{
	public readonly Element[] elements;
	public readonly uint size;
	
	public VertexLayout(Element[] elements)
	{
		this.elements = elements;
		
		var size = 0;
		foreach (var element in elements)
		{
			element.offset = (short)size;
			size += element.type switch
			{
				VertexType.Float or VertexType.Int => (short)(4 * element.number), // 4 bytes = 32bits
				VertexType.Short                   => (short)(2 * element.number), // 2 bytes = 16bits
				_                                  => element.number
			};
		}
		
		this.size = (uint)size;
	}
	
	public class Element(uint semantic, VertexType type, byte number, bool normalized = false)
	{
		public readonly uint semantic = semantic;
		public readonly VertexType type = type;
		public readonly byte number = number;
		public readonly bool normalized = normalized;
		
		// Assigné par VertexLayout :
		public short offset;
		
		public Element(VertexSemantic semantic, VertexType type, byte number, bool normalized = false) : this((uint)semantic, type, number, normalized) { }
	}
}