namespace XYEngine;

public struct Region(Vector2 position00, Vector2 position11)
{
	public Vector2 position00 { get; set; } = position00;
	public Vector2 position11 { get; set; } = position11;
	
	public Region(Vector2 position) : this(position, position) { }
	public Region(float x, float y, float z, float w) : this(new Vector2(x, y), new Vector2(z, w)) { }
	public Region(float xz, float yw) : this(new Vector2(xz, yw), new Vector2(xz, yw)) { }
	public Region(float value) : this(new Vector2(value), new Vector2(value)) { }
	
	public Region Intersection(Region other)
	{
		var newPosition00 = new Vector2(
			Math.Max(position00.x, other.position00.x),
			Math.Max(position00.y, other.position00.y)
		);
		
		var newPosition11 = new Vector2(
			Math.Min(position11.x, other.position11.x),
			Math.Min(position11.y, other.position11.y)
		);
		
		if (newPosition11.x <= newPosition00.x || newPosition11.y <= newPosition00.y)
			return new Region(Vector2Int.zero, Vector2Int.zero);
		
		return new Region(newPosition00, newPosition11);
	}
	
	public override string ToString() => $"[pos00: {position00} pos11: {position11}]";
}