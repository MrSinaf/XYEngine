namespace XYEngine;

public struct RegionInt(Vector2Int position00, Vector2Int position11) : ILerpable<RegionInt>, IEquatable<RegionInt>
{
	public static RegionInt zero => new (Vector2Int.zero, Vector2Int.zero);
	public static RegionInt one => new (Vector2Int.zero, Vector2Int.one);
	
	public int left => Math.Min(position00.x, position11.x);
	public int right => Math.Max(position00.x, position11.x);
	public int top => Math.Min(position00.y, position11.y);
	public int bottom => Math.Max(position00.y, position11.y);
	public Vector2Int center => (position00 + position11) / 2;
	public Vector2Int min => new (left, top);
	public Vector2Int max => new (right, bottom);
	public Vector2Int size => max - min;
	
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
	
	public bool Contains(Vector2Int point)
		=> point.x >= left && point.x <= right && point.y >= top && point.y <= bottom;
	
	public bool Contains(RegionInt other)
		=> other.left >= left && other.right <= right && other.top >= top && other.bottom <= bottom;
	
	public bool Overlaps(RegionInt other)
		=> left < other.right && right > other.left && top < other.bottom && bottom > other.top;
	
	public RegionInt Encapsulate(Vector2Int point)
	{
		var newMin = new Vector2Int(Math.Min(left, point.x), Math.Min(top, point.y));
		var newMax = new Vector2Int(Math.Max(right, point.x), Math.Max(bottom, point.y));
		return new RegionInt(newMin, newMax);
	}
	
	public RegionInt Lerp(RegionInt other, float t) => new (
		position00.Lerp(other.position00, t),
		position11.Lerp(other.position11, t)
	);
	
	public override string ToString() => $"[pos00: {position00} pos11: {position11}]";
	
	public bool Equals(RegionInt other)
		=> position00.Equals(other.position00) && position11.Equals(other.position11);
	
	public override bool Equals(object obj) => obj is RegionInt other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(position00, position11);
	
	public static bool operator ==(RegionInt a, RegionInt b) => a.Equals(b);
	public static bool operator !=(RegionInt a, RegionInt b) => !a.Equals(b);
	public static RegionInt operator *(RegionInt m, int scalar) => new (m.position00 * scalar, m.position11 * scalar);
	public static RegionInt operator /(RegionInt m, int scalar) => new (m.position00 / scalar, m.position11 / scalar);
	
	public static explicit operator RegionInt(Region value) => new (
		new Vector2Int((int)value.position00.x, (int)value.position00.y),
		new Vector2Int((int)value.position11.x, (int)value.position11.y)
	);
}