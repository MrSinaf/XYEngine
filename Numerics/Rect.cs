namespace XYEngine;

public struct Rect(Vector2 position, Vector2 size)
{
	public static Rect zero => new (Vector2.zero, Vector2.zero);
	public static Rect one => new (Vector2.zero, Vector2.one);
	
	public Vector2 position { get; set; } = position;
	public Vector2 size { get; set; } = size;
	
	public override string ToString() => $"[pos: {position} size: {size}]";
}