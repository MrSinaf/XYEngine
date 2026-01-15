namespace XYEngine;

public struct Vector2Int(int x, int y) : IEquatable<Vector2Int>, ILerpable<Vector2Int>
{
	public static Vector2Int zero => new ();
	public static Vector2Int one => new (1);
	public static Vector2Int left => new (-1, 0);
	public static Vector2Int right => new (1, 0);
	public static Vector2Int top => new (0, 1);
	public static Vector2Int bottom => new (0, -1);
	
	public int x { get; set; } = x;
	public int y { get; set; } = y;
	
	public float length => MathF.Sqrt(x * x + y * y);
	public int lengthSquared => x * x + y * y;
	
	public Vector2Int(int xy) : this(xy, xy) { }
	
	public Vector2 ToVector2() => new (x, y);
	
	public bool IsOutsideBounds(Vector2Int min, Vector2Int max) => x < min.x || x > max.x || y < min.y || y > max.y;
	public bool IsInsideBounds(Vector2Int min, Vector2Int max) => x >= min.x && x <= max.x && y >= min.y && y <= max.y;
	public int Dot(Vector2Int other) => x * other.x + y * other.y;
	public int Cross(Vector2Int other) => x * other.y - y * other.x;
	public float AngleTo(Vector2Int target) => MathF.Atan2(target.y - y, target.x - x);
	public Vector2Int Abs() => new (Math.Abs(x), Math.Abs(y));
	
	public float DistanceTo(Vector2Int target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		
		return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
	}
	
	public int DistanceSquaredTo(Vector2Int target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		
		return deltaX * deltaX + deltaY * deltaY;
	}
	
	public Vector2Int Lerp(Vector2Int other, float t) => new (
		(int)(x + (other.x - x) * t),
		(int)(y + (other.y - y) * t)
	);
	
	public Vector2Int Clamp(Vector2Int min, Vector2Int max) => new (
		Math.Clamp(x, min.x, max.x),
		Math.Clamp(y, min.y, max.y)
	);
	
	public Vector2Int MoveTowards(Vector2Int target, int maxDistance)
	{
		var direction = target - this;
		var distanceSquared = direction.lengthSquared;
		
		if (distanceSquared <= maxDistance * maxDistance || distanceSquared == 0)
			return target;
		
		var distance = MathF.Sqrt(distanceSquared);
		var normalized = new Vector2(direction.x / distance, direction.y / distance);
		
		return new Vector2Int(
			(int)MathF.Round(x + normalized.x * maxDistance),
			(int)MathF.Round(y + normalized.y * maxDistance)
		);
	}
	
	public bool Equals(Vector2Int other) => x == other.x && y == other.y;
	public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y);
	public override string ToString() => $"({x}:{y})";
	
	public static Vector2Int operator /(Vector2Int a, int scalar) => new (a.x / scalar, a.y / scalar);
	public static Vector2 operator /(Vector2Int a, float scalar) => new (a.x / scalar, a.y / scalar);
	public static Vector2Int operator /(int scalar, Vector2Int a) => new (scalar / a.x, scalar / a.y);
	public static Vector2Int operator *(Vector2Int a, int scalar) => new (a.x * scalar, a.y * scalar);
	public static Vector2Int operator *(int scalar, Vector2Int a) => new (a.x * scalar, a.y * scalar);
	public static Vector2 operator *(Vector2Int a, float scalar) => new (a.x * scalar, a.y * scalar);
	public static Vector2 operator *(float scalar, Vector2Int a) => new (a.x * scalar, a.y * scalar);
	
	public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new (a.x + b.x, a.y + b.y);
	public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new (a.x - b.x, a.y - b.y);
	public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new (a.x * b.x, a.y * b.y);
	public static Vector2Int operator /(Vector2Int a, Vector2Int b) => new (a.x / b.x, a.y / b.y);
	public static Vector2Int operator -(Vector2Int a) => new (-a.x, -a.y);
	
	public static bool operator ==(Vector2Int a, Vector2Int b) => a.x == b.x && a.y == b.y;
	public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);
	
	public static bool operator >(Vector2Int a, Vector2Int b) => a.lengthSquared > b.lengthSquared;
	public static bool operator <(Vector2Int a, Vector2Int b) => a.lengthSquared < b.lengthSquared;
	public static bool operator >=(Vector2Int a, Vector2Int b) => a.lengthSquared >= b.lengthSquared;
	public static bool operator <=(Vector2Int a, Vector2Int b) => a.lengthSquared <= b.lengthSquared;
}