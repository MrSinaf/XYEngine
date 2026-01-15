namespace XYEngine;

public struct Rect(Vector2 position, Vector2 size) : ILerpable<Rect>, IEquatable<Rect>
{
	public static Rect zero => new (Vector2.zero, Vector2.zero);
	public static Rect one => new (Vector2.zero, Vector2.one);
	
	public float x => position.x;
	public float y => position.y;
	public float width => size.x;
	public float height => size.y;
	public float left => position.x;
	public float right => position.x + size.x;
	public float top => position.y;
	public float bottom => position.y + size.y;
	public Vector2 center => position + size * 0.5f;
	public Vector2 min => position;
	public Vector2 max => position + size;
	
	public Vector2 position { get; set; } = position;
	public Vector2 size { get; set; } = size;
	
	public Rect(float x, float y, float width, float height) : this(new Vector2(x, y), new Vector2(width, height)) { }
	public Rect(float x, float y, float size) : this(new Vector2(x, y), new Vector2(size)) { }
	
	public Rect Lerp(Rect other, float t) => new (
		position.Lerp(other.position, t),
		size.Lerp(other.size, t)
	);
	
	public bool Contains(Vector2 point)
		=> point.x >= left && point.x <= right && point.y >= top && point.y <= bottom;
	
	public bool Contains(Rect other)
		=> other.left >= left && other.right <= right && other.top >= top && other.bottom <= bottom;
	
	public bool Overlaps(Rect other)
		=> left < other.right && right > other.left && top < other.bottom && bottom > other.top;
	
	public Rect Encapsulate(Vector2 point)
	{
		var newMin = new Vector2(Math.Min(left, point.x), Math.Min(top, point.y));
		var newMax = new Vector2(Math.Max(right, point.x), Math.Max(bottom, point.y));
		return new Rect(newMin, newMax - newMin);
	}
	
	public bool Equals(Rect other) => position.Equals(other.position) && size.Equals(other.size);
	public override bool Equals(object obj) => obj is Rect other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(position, size);
	public override string ToString() => $"[pos: {position} size: {size}]";
	
	public static bool operator ==(Rect a, Rect b) => a.Equals(b);
	public static bool operator !=(Rect a, Rect b) => !a.Equals(b);
	
	public static implicit operator Rect(RectInt value) => new (value.position, value.size);
}