namespace XYEngine;

public struct RegionInt(Vector2Int position00, Vector2Int position11) : ILerpable<RegionInt>
{
	public Vector2Int position00 { get; set; } = position00;
	public Vector2Int position11 { get; set; } = position11;
	
	public RegionInt(Vector2Int position) : this(position, position) { }
	public RegionInt(int x, int y, int z, int w) : this(new Vector2Int(x, y), new Vector2Int(z, w)) { }
	public RegionInt(int xz, int yw) : this(new Vector2Int(xz, yw), new Vector2Int(xz, yw)) { }
	public RegionInt(int value) : this(new Vector2Int(value), new Vector2Int(value)) { }
	
	public RegionInt Intersection(RegionInt other)
	{
		var newPosition00 = new Vector2Int(
			Math.Max(position00.x, other.position00.x),
			Math.Max(position00.y, other.position00.y)
		);
		
		var newPosition11 = new Vector2Int(
			Math.Min(position11.x, other.position11.x),
			Math.Min(position11.y, other.position11.y)
		);
		
		if (newPosition11.x <= newPosition00.x || newPosition11.y <= newPosition00.y)
			return new RegionInt(Vector2Int.zero, Vector2Int.zero);
		
		return new RegionInt(newPosition00, newPosition11);
	}
	
	public static RegionInt operator *(RegionInt m, int scalar) => new (m.position00 * scalar, m.position11 * scalar);
	public static RegionInt operator /(RegionInt m, int scalar) => new (m.position00 / scalar, m.position11 / scalar);
	
	public RegionInt Lerp(RegionInt other, float t) => new (
		position00.Lerp(other.position00, t),
		position11.Lerp(other.position11, t)
	);
	
	public override string ToString() => $"[pos00: {position00} pos11: {position11}]";
}