namespace XYEngine;

public struct Vector4Int(int x, int y, int z, int w) : IEquatable<Vector4Int>, ILerpable<Vector4Int>
{
	public static Vector4Int zero => new ();
	public static Vector4Int one => new (1);
	
	public int x { get; set; } = x;
	public int y { get; set; } = y;
	public int z { get; set; } = z;
	public int w { get; set; } = w;
	
	public float length => MathF.Sqrt(x * x + y * y + z * z + w * w);
	public int lengthSquared => x * x + y * y + z * z + w * w;
	
	public Vector4Int(int xyzw) : this(xyzw, xyzw, xyzw, xyzw) { }
	public Vector4Int(Vector2Int xy, int z, int w) : this(xy.x, xy.y, z, w) { }
	public Vector4Int(int x, Vector2Int yz, int w) : this(x, yz.x, yz.y, w) { }
	public Vector4Int(int x, int y, Vector2Int zw) : this(x, y, zw.x, zw.y) { }
	public Vector4Int(Vector2Int xy, Vector2Int zw) : this(xy.x, xy.y, zw.x, zw.y) { }
	public Vector4Int(Vector3Int xyz, int w) : this(xyz.x, xyz.y, xyz.z, w) { }
	public Vector4Int(int x, Vector3Int yzw) : this(x, yzw.x, yzw.y, yzw.z) { }
	
	public Vector2Int ToVector2Int() => new (x, y);
	public Vector3Int ToVector3Int() => new (x, y, z);
	public Vector4 ToVector4() => new (x, y, z, w);
	
	public bool IsOutsideBounds(Vector4Int min, Vector4Int max) => x < min.x || x > max.x || y < min.y || y > max.y ||
																   z < min.z || z > max.z || w < min.w || w > max.w;
	
	public bool IsInsideBounds(Vector4Int min, Vector4Int max) => x >= min.x && x <= max.x && y >= min.y &&
																  y <= max.y && z >= min.z && z <= max.z &&
																  w >= min.w && w <= max.w;
	
	public int Dot(Vector4Int other) => x * other.x + y * other.y + z * other.z + w * other.w;
	
	public Vector4Int Abs() => new (Math.Abs(x), Math.Abs(y), Math.Abs(z), Math.Abs(w));
	
	public float DistanceTo(Vector4Int target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		var deltaW = target.w - w;
		
		return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + deltaW * deltaW);
	}
	
	public int DistanceSquaredTo(Vector4Int target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		var deltaW = target.w - w;
		
		return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + deltaW * deltaW;
	}
	
	public Vector4Int Lerp(Vector4Int other, float t) => new (
		(int)(x + (other.x - x) * t),
		(int)(y + (other.y - y) * t),
		(int)(z + (other.z - z) * t),
		(int)(w + (other.w - w) * t)
	);
	
	public Vector4Int Clamp(Vector4Int min, Vector4Int max) => new (
		Math.Clamp(x, min.x, max.x),
		Math.Clamp(y, min.y, max.y),
		Math.Clamp(z, min.z, max.z),
		Math.Clamp(w, min.w, max.w)
	);
	
	public Vector4Int MoveTowards(Vector4Int target, int maxDistance)
	{
		var direction = target - this;
		var distanceSquared = direction.lengthSquared;
		
		if (distanceSquared <= maxDistance * maxDistance || distanceSquared == 0)
			return target;
		
		var distance = MathF.Sqrt(distanceSquared);
		var normalized = new Vector4(direction.x / distance, direction.y / distance, direction.z / distance,
									 direction.w / distance);
		
		return new Vector4Int(
			(int)MathF.Round(x + normalized.x * maxDistance),
			(int)MathF.Round(y + normalized.y * maxDistance),
			(int)MathF.Round(z + normalized.z * maxDistance),
			(int)MathF.Round(w + normalized.w * maxDistance)
		);
	}
	
	public bool Equals(Vector4Int other) => x == other.x && y == other.y && z == other.z && w == other.w;
	public override bool Equals(object obj) => obj is Vector4Int other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y, z, w);
	public override string ToString() => $"({x}:{y}:{z}:{w})";
	
	public static Vector4Int operator /(Vector4Int a, int scalar)
		=> new (a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
	
	public static Vector4 operator /(Vector4Int a, float scalar)
		=> new (a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
	
	public static Vector4Int operator /(int scalar, Vector4Int a)
		=> new (scalar / a.x, scalar / a.y, scalar / a.z, scalar / a.w);
	
	public static Vector4Int operator *(Vector4Int a, int scalar)
		=> new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	
	public static Vector4Int operator *(int scalar, Vector4Int a)
		=> new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	
	public static Vector4 operator *(Vector4Int a, float scalar)
		=> new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	
	public static Vector4 operator *(float scalar, Vector4Int a)
		=> new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	
	public static Vector4Int operator +(Vector4Int a, Vector4Int b) => new (a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	public static Vector4Int operator -(Vector4Int a, Vector4Int b) => new (a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	public static Vector4Int operator *(Vector4Int a, Vector4Int b) => new (a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	public static Vector4Int operator /(Vector4Int a, Vector4Int b) => new (a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
	public static Vector4Int operator -(Vector4Int a) => new (-a.x, -a.y, -a.z, -a.w);
	
	public static bool operator ==(Vector4Int a, Vector4Int b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
	public static bool operator !=(Vector4Int a, Vector4Int b) => !(a == b);
	
	public static bool operator >(Vector4Int a, Vector4Int b) => a.lengthSquared > b.lengthSquared;
	public static bool operator <(Vector4Int a, Vector4Int b) => a.lengthSquared < b.lengthSquared;
	public static bool operator >=(Vector4Int a, Vector4Int b) => a.lengthSquared >= b.lengthSquared;
	public static bool operator <=(Vector4Int a, Vector4Int b) => a.lengthSquared <= b.lengthSquared;
}