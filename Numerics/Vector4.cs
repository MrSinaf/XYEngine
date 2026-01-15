namespace XYEngine;

public struct Vector4(float x, float y, float z, float w) : IEquatable<Vector4>, ILerpable<Vector4>
{
	public const float EPSILON = 1e-5F;
	
	public static Vector4 zero => new ();
	public static Vector4 one => new (1);
	
	public float x { get; set; } = x;
	public float y { get; set; } = y;
	public float z { get; set; } = z;
	public float w { get; set; } = w;
	
	public float lengthSquared => x * x + y * y + z * z + w * w;
	public float length => MathF.Sqrt(lengthSquared);
	
	public Vector4 normalized
	{
		get
		{
			var l = length;
			return l > EPSILON ? this / l : zero;
		}
	}
	
	public Vector4(float xyzw) : this(xyzw, xyzw, xyzw, xyzw) { }
	public Vector4(Vector2 xy, float z, float w) : this(xy.x, xy.y, z, w) { }
	public Vector4(float x, Vector2 yz, float w) : this(x, yz.x, yz.y, w) { }
	public Vector4(float x, float y, Vector2 zw) : this(x, y, zw.x, zw.y) { }
	public Vector4(Vector2 xy, Vector2 zw) : this(xy.x, xy.y, zw.x, zw.y) { }
	public Vector4(Vector3 xyz, float w) : this(xyz.x, xyz.y, xyz.z, w) { }
	public Vector4(float x, Vector3 yzw) : this(x, yzw.x, yzw.y, yzw.z) { }
	
	public Vector2 ToVector2() => new (x, y);
	public Vector3 ToVector3() => new (x, y, z);
	
	public Vector4Int ToVector4Int(RoundingMode operation = RoundingMode.Round) => operation switch
	{
		RoundingMode.Round => new Vector4Int((int)MathF.Round(x), (int)MathF.Round(y), (int)MathF.Round(z),
											 (int)MathF.Round(w)),
		RoundingMode.Floor => new Vector4Int((int)MathF.Floor(x), (int)MathF.Floor(y), (int)MathF.Floor(z),
											 (int)MathF.Floor(w)),
		RoundingMode.Ceiling => new Vector4Int((int)MathF.Ceiling(x), (int)MathF.Ceiling(y), (int)MathF.Ceiling(z),
											   (int)MathF.Ceiling(w)),
		_ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
	};
	
	public void Normalize() => this = normalized;
	
	public bool IsOutsideBounds(Vector4 min, Vector4 max) => x < min.x || x > max.x || y < min.y || y > max.y ||
															 z < min.z || z > max.z || w < min.w || w > max.w;
	
	public bool IsInsideBounds(Vector4 min, Vector4 max) => x >= min.x && x <= max.x && y >= min.y && y <= max.y &&
															z >= min.z && z <= max.z && w >= min.w && w <= max.w;
	
	public float Dot(Vector4 other) => x * other.x + y * other.y + z * other.z + w * other.w;
	public Vector4 Abs() => new (MathF.Abs(x), MathF.Abs(y), MathF.Abs(z), MathF.Abs(w));
	
	public float DistanceTo(Vector4 target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		var deltaW = target.w - w;
		
		return MathF.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + deltaW * deltaW);
	}
	
	public float DistanceSquaredTo(Vector4 target)
	{
		var deltaX = target.x - x;
		var deltaY = target.y - y;
		var deltaZ = target.z - z;
		var deltaW = target.w - w;
		
		return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ + deltaW * deltaW;
	}
	
	public Vector4 Lerp(Vector4 other, float t) => new (
		x + (other.x - x) * t,
		y + (other.y - y) * t,
		z + (other.z - z) * t,
		w + (other.w - w) * t
	);
	
	public Vector4 Clamp(Vector4 min, Vector4 max) => new (
		Math.Clamp(x, min.x, max.x),
		Math.Clamp(y, min.y, max.y),
		Math.Clamp(z, min.z, max.z),
		Math.Clamp(w, min.w, max.w)
	);
	
	public Vector4 MoveTowards(Vector4 target, float maxDistance)
	{
		var direction = target - this;
		var distance = direction.length;
		
		if (distance <= maxDistance)
			return target;
		
		return this + direction.normalized * maxDistance;
	}
	
	public bool Equals(Vector4 other)
		=> x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
	
	public override bool Equals(object obj) => obj is Vector4 other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(x, y, z, w);
	public override string ToString() => $"({x:0.00}:{y:0.00}:{z:0.00}:{w:0.00})";
	
	public static Vector4 operator /(Vector4 a, float scalar)
		=> new (a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
	
	public static Vector4 operator /(float scalar, Vector4 a)
		=> new (scalar / a.x, scalar / a.y, scalar / a.z, scalar / a.w);
	
	public static Vector4 operator *(Vector4 a, float scalar)
		=> new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	
	public static Vector4 operator *(float scalar, Vector4 a)
		=> new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	
	public static Vector4 operator +(Vector4 a, Vector4 b) => new (a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	public static Vector4 operator -(Vector4 a, Vector4 b) => new (a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	public static Vector4 operator *(Vector4 a, Vector4 b) => new (a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	public static Vector4 operator /(Vector4 a, Vector4 b) => new (a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
	public static Vector4 operator -(Vector4 a) => new (-a.x, -a.y, -a.z, -a.w);
	
	public static bool operator ==(Vector4 a, Vector4 b) =>
		MathF.Abs(a.x - b.x) <= EPSILON &&
		MathF.Abs(a.y - b.y) <= EPSILON &&
		MathF.Abs(a.z - b.z) <= EPSILON &&
		MathF.Abs(a.w - b.w) <= EPSILON;
	
	public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);
	
	public static bool operator >(Vector4 a, Vector4 b) => a.lengthSquared > b.lengthSquared;
	public static bool operator <(Vector4 a, Vector4 b) => a.lengthSquared < b.lengthSquared;
	public static bool operator >=(Vector4 a, Vector4 b) => a.lengthSquared >= b.lengthSquared;
	public static bool operator <=(Vector4 a, Vector4 b) => a.lengthSquared <= b.lengthSquared;
}