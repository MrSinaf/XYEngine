namespace XYEngine;

public struct Region(Vector2 position00, Vector2 position11) : ILerpable<Region>, IEquatable<Region>
{
	public static Region zero => new (Vector2.zero, Vector2.zero);
	public static Region one => new (Vector2.zero, Vector2.one);
	
	public float left => Math.Min(position00.x, position11.x);
	public float right => Math.Max(position00.x, position11.x);
	public float top => Math.Min(position00.y, position11.y);
	public float bottom => Math.Max(position00.y, position11.y);
	public Vector2 center => (position00 + position11) * 0.5f;
	public Vector2 min => new (left, top);
	public Vector2 max => new (right, bottom);
	public Vector2 size => max - min;
	
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
			return new Region(Vector2.zero, Vector2.zero);
		
		return new Region(newPosition00, newPosition11);
	}
	
	public bool Contains(Vector2 point)
		=> point.x >= left && point.x <= right && point.y >= top && point.y <= bottom;
	
	public bool Contains(Region other)
		=> other.left >= left && other.right <= right && other.top >= top && other.bottom <= bottom;
	
	public bool Overlaps(Region other)
		=> left < other.right && right > other.left && top < other.bottom && bottom > other.top;
	
	public Region Encapsulate(Vector2 point)
	{
		var newMin = new Vector2(Math.Min(left, point.x), Math.Min(top, point.y));
		var newMax = new Vector2(Math.Max(right, point.x), Math.Max(bottom, point.y));
		return new Region(newMin, newMax);
	}
	
	public Region Lerp(Region other, float t) => new (
		position00.Lerp(other.position00, t),
		position11.Lerp(other.position11, t)
	);
	
	public override string ToString() => $"[pos00: {position00} pos11: {position11}]";
	public bool Equals(Region other)
		=> position00.Equals(other.position00) && position11.Equals(other.position11);
	public override bool Equals(object obj) => obj is Region other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(position00, position11);
	
	public static bool operator ==(Region a, Region b) => a.Equals(b);
	public static bool operator !=(Region a, Region b) => !a.Equals(b);
	
	public static Region operator *(Region m, float scalar) => new (m.position00 * scalar, m.position11 * scalar);
	public static Region operator /(Region m, float scalar) => new (m.position00 / scalar, m.position11 / scalar);
	
	public static implicit operator Region(RegionInt value) => new (value.position00, value.position11);
}