namespace XYEngine;

public struct Vector2(float x, float y) : IEquatable<Vector2>, ILerpable<Vector2>
{
	public const float EPSILON = 1e-5F;
	
	/// <summary>
	///     Vector2(0, 0)
	/// </summary>
	public static Vector2 zero => new ();
	/// <summary>
	///     Vector2(1, 1)
	/// </summary>
	public static Vector2 one => new (1);
	/// <summary>
	///     Vector2(-1, 0)
	/// </summary>
	public static Vector2 left => new (-1, 0);
	/// <summary>
	///     Vector2(1, 0)
	/// </summary>
	public static Vector2 right => new (1, 0);
	/// <summary>
	///     Vector2(0, 1)
	/// </summary>
	public static Vector2 top => new (0, 1);
	/// <summary>
	///     Vector2(0, -1)
	/// </summary>
	public static Vector2 bottom => new (0, -1);
	
	public float x { get; set; } = x;
	public float y { get; set; } = y;
	
	public float length => MathF.Sqrt(x * x + y * y);
	
	public Vector2 normalized => length > 0 ? this / length : new Vector2(0, 0);
	
	public Vector2(float xy) : this(xy, xy) { }
	
	public void Normalize() => this = normalized;
	
	public bool IsOutsideBounds(Vector2 min, Vector2 max) => x < min.x || x > max.x || y < min.y || y > max.y;
	public bool IsInsideBounds(Vector2 min, Vector2 max) => x >= min.x && x <= max.x && y >= min.y && y <= max.y;
	
	
	public Vector2 Lerp(Vector2 other, float t) => new (
		x + (other.x - x) * t,
		y + (other.y - y) * t
	);
	
	public Vector2 Clamp(Vector2 min, Vector2 max) => new (
		Math.Clamp(x, min.x, max.x),
		Math.Clamp(y, min.y, max.y)
	);
	
	public Vector2Int ToVector2Int(RoundingMode operation = RoundingMode.Round) => operation switch
	{
		RoundingMode.Round   => new Vector2Int((int)MathF.Round(x), (int)MathF.Round(y)),
		RoundingMode.Floor   => new Vector2Int((int)MathF.Floor(x), (int)MathF.Floor(y)),
		RoundingMode.Ceiling => new Vector2Int((int)MathF.Ceiling(x), (int)MathF.Ceiling(y)),
		_                    => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
	};
	
	public static implicit operator Vector2(Vector2Int value) => new (value.x, value.y);
	
	public static Vector2 operator /(Vector2 a, float scalar) => new (a.x / scalar, a.y / scalar);
	public static Vector2 operator /(float scalar, Vector2 a) => new (scalar / a.x, scalar / a.y);
	public static Vector2 operator *(Vector2 a, float scalar) => new (a.x * scalar, a.y * scalar);
	
	public static Vector2 operator +(Vector2 a, Vector2 b) => new (a.x + b.x, a.y + b.y);
	public static Vector2 operator -(Vector2 a, Vector2 b) => new (a.x - b.x, a.y - b.y);
	public static Vector2 operator *(Vector2 a, Vector2 b) => new (a.x * b.x, a.y * b.y);
	public static Vector2 operator /(Vector2 a, Vector2 b) => new (a.x / b.x, a.y / b.y);
	public static Vector2 operator -(Vector2 a) => new (-a.x, -a.y);
	
	public static bool operator ==(Vector2 a, Vector2 b) => MathF.Abs(a.x - b.x) <= EPSILON && MathF.Abs(a.y - b.y) <= EPSILON;
	public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);
	
	public static bool operator >(Vector2 a, Vector2 b) => a.length > b.length;
	public static bool operator <(Vector2 a, Vector2 b) => a.length < b.length;
	public static bool operator >=(Vector2 a, Vector2 b) => a.length >= b.length;
	public static bool operator <=(Vector2 a, Vector2 b) => a.length <= b.length;
	
	public bool Equals(Vector2 other) => x.Equals(other.x) && y.Equals(other.y);
	public override bool Equals(object obj) => obj is Vector2 other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y);
	
	public override string ToString() => $"({x:0.00}:{y:0.00})";
}