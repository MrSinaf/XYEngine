namespace XYEngine;

public struct RectInt(Vector2Int position, Vector2Int size) : ILerpable<RectInt>, IEquatable<RectInt>
{
	public static RectInt zero => new (Vector2Int.zero, Vector2Int.zero);
	public static RectInt one => new (Vector2Int.zero, Vector2Int.one);
	
	public int x => position.x;
	public int y => position.y;
	public int width => size.x;
	public int height => size.y;
	public int left => position.x;
	public int right => position.x + size.x;
	public int top => position.y;
	public int bottom => position.y + size.y;
	public Vector2Int center => position + size / 2;
	public Vector2Int min => position;
	public Vector2Int max => position + size;
	
	public Vector2Int position { get; set; } = position;
	public Vector2Int size { get; set; } = size;
	
	public RectInt(int x, int y, int width, int height) : this(new Vector2Int(x, y), new Vector2Int(width, height)) { }
	public RectInt(int x, int y, int size) : this(new Vector2Int(x, y), new Vector2Int(size)) { }
	
	public RectInt Lerp(RectInt other, float t) => new (
		position.Lerp(other.position, t),
		size.Lerp(other.size, t)
	);
	
	public bool Contains(Vector2Int point)
		=> point.x >= left && point.x <= right && point.y >= top && point.y <= bottom;
	
	public bool Contains(RectInt other)
		=> other.left >= left && other.right <= right && other.top >= top && other.bottom <= bottom;
	
	public bool Overlaps(RectInt other)
		=> left < other.right && right > other.left && top < other.bottom && bottom > other.top;
	
	public RectInt Encapsulate(Vector2Int point)
	{
		var newMin = new Vector2Int(Math.Min(left, point.x), Math.Min(top, point.y));
		var newMax = new Vector2Int(Math.Max(right, point.x), Math.Max(bottom, point.y));
		return new RectInt(newMin, newMax - newMin);
	}
	
	public override string ToString() => $"[pos: {position} size: {size}]";
	public bool Equals(RectInt other) => position.Equals(other.position) && size.Equals(other.size);
	public override bool Equals(object obj) => obj is RectInt other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(position, size);
	
	public static bool operator ==(RectInt a, RectInt b) => a.Equals(b);
	public static bool operator !=(RectInt a, RectInt b) => !a.Equals(b);
	
	public static explicit operator RectInt(Rect value) => new (
		new Vector2Int((int)value.position.x, (int)value.position.y),
		new Vector2Int((int)value.size.x, (int)value.size.y)
	);
}