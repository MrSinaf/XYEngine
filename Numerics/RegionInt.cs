namespace XYEngine;

public struct RegionInt(Vector2Int position00, Vector2Int position11)
{
	public Vector2Int position00 { get; set; } = position00;
	public Vector2Int position11 { get; set; } = position11;
	
	public RegionInt(Vector2Int position) : this(position, position) { }
	public RegionInt(int x, int y, int z, int w) : this(new Vector2Int(x, y), new Vector2Int(z, w)) { }
	public RegionInt(int xz, int yw) : this(new Vector2Int(xz, yw), new Vector2Int(xz, yw)) { }
	public RegionInt(int value) : this(new Vector2Int(value), new Vector2Int(value)) { }
	
	
	public static RegionInt operator *(RegionInt m, int scalar) => new (m.position00 * scalar, m.position11 * scalar);
	public static RegionInt operator /(RegionInt m, int scalar) => new (m.position00 / scalar, m.position11 / scalar);
	
	public override string ToString() => $"[pos00: {position00} pos11: {position11}]";
}