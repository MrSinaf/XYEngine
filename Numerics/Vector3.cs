namespace XYEngine;

public struct Vector3(float x, float y, float z) : IEquatable<Vector3>, ILerpable<Vector3>
{
	public const float EPSILON = 1e-5F;
	
	public static Vector3 zero => new ();
	public static Vector3 one => new (1);
	public static Vector3 left => new (-1, 0, 0);
	public static Vector3 right => new (1, 0, 0);
	public static Vector3 up => new (0, 1, 0);
	public static Vector3 down => new (0, -1, 0);
	public static Vector3 forward => new (0, 0, 1);
	public static Vector3 backward => new (0, 0, -1);
	
	public float x { get; set; } = x;
	public float y { get; set; } = y;
	public float z { get; set; } = z;
	
	public float lengthSquared => x * x + y * y + z * z;
	public float length => MathF.Sqrt(lengthSquared);
	
	public Vector3 normalized
	{
		get
		{
			var l = length;
			return l > EPSILON ? this / l : zero;
		}
	}
	
	public Vector3(float xyz) : this(xyz, xyz, xyz) { }
	public Vector3(Vector2 xy, float z) : this(xy.x, xy.y, z) { }
	public Vector3(float x, Vector2 yz) : this(x, yz.x, yz.y) { }
	
	public Vector2 ToVector2() => new (x, y);
	public Vector4 ToVector4(float w) => new (x, y, z, w);
	
	public Vector3Int ToVector3Int(RoundingMode operation = RoundingMode.Round) => operation switch
	{
		RoundingMode.Round   => new Vector3Int((int)MathF.Round(x), (int)MathF.Round(y), (int)MathF.Round(z)),
		RoundingMode.Floor   => new Vector3Int((int)MathF.Floor(x), (int)MathF.Floor(y), (int)MathF.Floor(z)),
		RoundingMode.Ceiling => new Vector3Int((int)MathF.Ceiling(x), (int)MathF.Ceiling(y), (int)MathF.Ceiling(z)),
		_                    => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
	};
	
	public void Normalize() => this = normalized;
	public bool IsOutsideBounds(Vector3 min, Vector3 max) 
		=> x < min.x || x > max.x || y < min.y || y > max.y || z < min.z || z > max.z;
	public bool IsInsideBounds(Vector3 min, Vector3 max) 
		=> x >= min.x && x <= max.x && y >= min.y && y <= max.y && z >= min.z && z <= max.z;
	public float Dot(Vector3 other) => x * other.x + y * other.y + z * other.z;
	public Vector3 Abs() => new (MathF.Abs(x), MathF.Abs(y), MathF.Abs(z));
	
	public Vector3 Cross(Vector3 other) => new (
		y * other.z - z * other.y,
		z * other.x - x * other.z,
		x * other.y - y * other.x
	);
	
	public float DistanceTo(Vector3 target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		
		return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
	}
	
	public float DistanceSquaredTo(Vector3 target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		
		return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
	}
	
	public Vector3 Lerp(Vector3 other, float t) => new (
		x + (other.x - x) * t,
		y + (other.y - y) * t,
		z + (other.z - z) * t
	);
	
	public Vector3 Clamp(Vector3 min, Vector3 max) => new (
		Math.Clamp(x, min.x, max.x),
		Math.Clamp(y, min.y, max.y),
		Math.Clamp(z, min.z, max.z)
	);
	
	public Vector3 MoveTowards(Vector3 target, float maxDistance)
	{
		var direction = target - this;
		var distance = direction.length;
		
		if (distance <= maxDistance)
			return target;
		
		return this + direction.normalized * maxDistance;
	}
	
	public bool Equals(Vector3 other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
	public override bool Equals(object obj) => obj is Vector3 other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y, z);
	public override string ToString() => $"({x:0.00}:{y:0.00}:{z:0.00})";
	
	public static Vector3 operator /(Vector3 a, float scalar) => new (a.x / scalar, a.y / scalar, a.z / scalar);
	public static Vector3 operator /(float scalar, Vector3 a) => new (scalar / a.x, scalar / a.y, scalar / a.z);
	public static Vector3 operator *(Vector3 a, float scalar) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	public static Vector3 operator *(float scalar, Vector3 a) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	
	public static Vector3 operator +(Vector3 a, Vector3 b) => new (a.x + b.x, a.y + b.y, a.z + b.z);
	public static Vector3 operator -(Vector3 a, Vector3 b) => new (a.x - b.x, a.y - b.y, a.z - b.z);
	public static Vector3 operator *(Vector3 a, Vector3 b) => new (a.x * b.x, a.y * b.y, a.z * b.z);
	public static Vector3 operator /(Vector3 a, Vector3 b) => new (a.x / b.x, a.y / b.y, a.z / b.z);
	public static Vector3 operator -(Vector3 a) => new (-a.x, -a.y, -a.z);
	
	public static bool operator ==(Vector3 a, Vector3 b) =>
		MathF.Abs(a.x - b.x) <= EPSILON &&
		MathF.Abs(a.y - b.y) <= EPSILON &&
		MathF.Abs(a.z - b.z) <= EPSILON;
	
	public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
	
	public static bool operator >(Vector3 a, Vector3 b) => a.lengthSquared > b.lengthSquared;
	public static bool operator <(Vector3 a, Vector3 b) => a.lengthSquared < b.lengthSquared;
	public static bool operator >=(Vector3 a, Vector3 b) => a.lengthSquared >= b.lengthSquared;
	public static bool operator <=(Vector3 a, Vector3 b) => a.lengthSquared <= b.lengthSquared;
}