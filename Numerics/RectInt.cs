namespace XYEngine;

public struct RectInt(Vector2Int position, Vector2Int size) : ILerpable<RectInt>
{
	public static RectInt zero => new (Vector2Int.zero, Vector2Int.zero);
	public static RectInt one => new (Vector2Int.zero, Vector2Int.one);
	
	public RectInt(int x, int y, int widht, int height) : this(new Vector2Int(x, y), new Vector2Int(widht, height)) { }
	public RectInt(int x, int y, int size) : this(new Vector2Int(x, y), new Vector2Int(size)) { }
	
	public Vector2Int position { get; set; } = position;
	public Vector2Int size { get; set; } = size;
	
	public RectInt Lerp(RectInt other, float t) => new (
		position.Lerp(other.position, t),
		size.Lerp(other.size, t)
	);
	
	public override string ToString() => $"[pos: {position} size: {size}]";
}