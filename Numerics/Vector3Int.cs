namespace XYEngine;

public struct Vector3Int(int x, int y, int z) : IEquatable<Vector3Int>, ILerpable<Vector3Int>
{
	public static Vector3Int zero => new ();
	public static Vector3Int one => new (1);
	public static Vector3Int left => new (-1, 0, 0);
	public static Vector3Int right => new (1, 0, 0);
	public static Vector3Int up => new (0, 1, 0);
	public static Vector3Int down => new (0, -1, 0);
	public static Vector3Int forward => new (0, 0, 1);
	public static Vector3Int backward => new (0, 0, -1);
	
	public int x { get; set; } = x;
	public int y { get; set; } = y;
	public int z { get; set; } = z;
	
	public float length => MathF.Sqrt(x * x + y * y + z * z);
	public int lengthSquared => x * x + y * y + z * z;
	
	public Vector3Int(int xyz) : this(xyz, xyz, xyz) { }
	public Vector3Int(Vector2Int xy, int z) : this(xy.x, xy.y, z) { }
	public Vector3Int(int x, Vector2Int yz) : this(x, yz.x, yz.y) { }
	
	public Vector2Int ToVector2Int() => new (x, y);
	public Vector4Int ToVector4Int(int w) => new (x, y, z, w);
	public Vector3 ToVector3() => new (x, y, z);
	
	public bool IsOutsideBounds(Vector3Int min, Vector3Int max) 
		=> x < min.x || x > max.x || y < min.y || y > max.y || z < min.z || z > max.z;
	public bool IsInsideBounds(Vector3Int min, Vector3Int max) 
		=> x >= min.x && x <= max.x && y >= min.y && y <= max.y && z >= min.z && z <= max.z;
	public int Dot(Vector3Int other) => x * other.x + y * other.y + z * other.z;
	
	public Vector3Int Abs() => new (Math.Abs(x), Math.Abs(y), Math.Abs(z));
	
	public Vector3Int Cross(Vector3Int other) => new (
		y * other.z - z * other.y,
		z * other.x - x * other.z,
		x * other.y - y * other.x
	);
	
	public float DistanceTo(Vector3Int target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		
		return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
	}
	
	public int DistanceSquaredTo(Vector3Int target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		
		return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
	}
	
	public Vector3Int Lerp(Vector3Int other, float t) => new (
		(int)(x + (other.x - x) * t),
		(int)(y + (other.y - y) * t),
		(int)(z + (other.z - z) * t)
	);
	
	public Vector3Int Clamp(Vector3Int min, Vector3Int max) => new (
		Math.Clamp(x, min.x, max.x),
		Math.Clamp(y, min.y, max.y),
		Math.Clamp(z, min.z, max.z)
	);
	
	public Vector3Int MoveTowards(Vector3Int target, int maxDistance)
	{
		var direction = target - this;
		var distanceSquared = direction.lengthSquared;
		
		if (distanceSquared <= maxDistance * maxDistance || distanceSquared == 0)
			return target;
		
		var distance = MathF.Sqrt(distanceSquared);
		var normalized = new Vector3(direction.x / distance, direction.y / distance, direction.z / distance);
		
		return new Vector3Int(
			(int)MathF.Round(x + normalized.x * maxDistance),
			(int)MathF.Round(y + normalized.y * maxDistance),
			(int)MathF.Round(z + normalized.z * maxDistance)
		);
	}
	
	public bool Equals(Vector3Int other) => x == other.x && y == other.y && z == other.z;
	public override bool Equals(object obj) => obj is Vector3Int other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y, z);
	public override string ToString() => $"({x}:{y}:{z})";
	
	public static Vector3Int operator /(Vector3Int a, int scalar) => new (a.x / scalar, a.y / scalar, a.z / scalar);
	public static Vector3 operator /(Vector3Int a, float scalar) => new (a.x / scalar, a.y / scalar, a.z / scalar);
	public static Vector3Int operator /(int scalar, Vector3Int a) => new (scalar / a.x, scalar / a.y, scalar / a.z);
	public static Vector3Int operator *(Vector3Int a, int scalar) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	public static Vector3Int operator *(int scalar, Vector3Int a) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	public static Vector3 operator *(Vector3Int a, float scalar) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	public static Vector3 operator *(float scalar, Vector3Int a) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	
	public static Vector3Int operator +(Vector3Int a, Vector3Int b) => new (a.x + b.x, a.y + b.y, a.z + b.z);
	public static Vector3Int operator -(Vector3Int a, Vector3Int b) => new (a.x - b.x, a.y - b.y, a.z - b.z);
	public static Vector3Int operator *(Vector3Int a, Vector3Int b) => new (a.x * b.x, a.y * b.y, a.z * b.z);
	public static Vector3Int operator /(Vector3Int a, Vector3Int b) => new (a.x / b.x, a.y / b.y, a.z / b.z);
	public static Vector3Int operator -(Vector3Int a) => new (-a.x, -a.y, -a.z);
	
	public static bool operator ==(Vector3Int a, Vector3Int b) => a.x == b.x && a.y == b.y && a.z == b.z;
	public static bool operator !=(Vector3Int a, Vector3Int b) => !(a == b);
	
	public static bool operator >(Vector3Int a, Vector3Int b) => a.lengthSquared > b.lengthSquared;
	public static bool operator <(Vector3Int a, Vector3Int b) => a.lengthSquared < b.lengthSquared;
	public static bool operator >=(Vector3Int a, Vector3Int b) => a.lengthSquared >= b.lengthSquared;
	public static bool operator <=(Vector3Int a, Vector3Int b) => a.lengthSquared <= b.lengthSquared;
}