namespace XYEngine;

public struct Vector2Int(int x, int y) : IEquatable<Vector2Int>
{
	public static Vector2Int zero => new ();
	public static Vector2Int one => new (1);
	
	public Vector2Int(int xy) : this(xy, xy) { }
	
	public int x { get; set; } = x;
	public int y { get; set; } = y;
	
	public readonly Vector2 ToVector2() => new (x, y);
	
	public static Vector2Int operator /(Vector2Int a, int scalar) => scalar == 0 ? a : new Vector2Int(a.x / scalar, a.y / scalar);
	public static Vector2Int operator /(Vector2Int a, float scalar) => scalar == 0 ? a : new Vector2Int((int)(a.x / scalar), (int)(a.y / scalar));
	public static Vector2Int operator /(int scalar, Vector2Int a) => scalar == 0 ? a : new Vector2Int(scalar / a.x, scalar / a.y);
	public static Vector2Int operator *(Vector2Int a, int scalar) => new (a.x * scalar, a.y * scalar);
	
	public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new (a.x + b.x, a.y + b.y);
	public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new (a.x - b.x, a.y - b.y);
	public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new (a.x * b.x, a.y * b.y);
	public static Vector2Int operator /(Vector2Int a, Vector2Int b) => new (a.x / b.x, a.y / b.y);
	public static Vector2Int operator -(Vector2Int a) => new (-a.x, -a.y);
	
	public static bool operator ==(Vector2Int a, Vector2Int b) => a.x == b.x && a.y == b.y;
	public static bool operator !=(Vector2Int a, Vector2Int b) => a.x != b.x && a.y != b.y;
	
	public static bool operator >(Vector2Int a, Vector2Int b) => a.x > b.x && a.y > b.y;
	public static bool operator <(Vector2Int a, Vector2Int b) => a.x < b.x && a.y < b.y;
	public static bool operator >=(Vector2Int a, Vector2Int b) => a.x >= b.x && a.y >= b.y;
	public static bool operator <=(Vector2Int a, Vector2Int b) => a.x <= b.x && a.y <= b.y;
	
	public bool Equals(Vector2Int other) => x.Equals(other.x) && y.Equals(other.y);
	public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y);
	
	public override string ToString() => $"({x}:{y})";
}