namespace XYEngine;

public struct RectInt(Vector2Int position, Vector2Int size)
{
	public static RectInt zero => new (Vector2Int.zero, Vector2Int.zero);
	public static RectInt one => new (Vector2Int.zero, Vector2Int.one);
	
	public Vector2Int position { get; set; } = position;
	public Vector2Int size { get; set; } = size;
	
	public override string ToString() => $"[pos: {position} size: {size}]";
}