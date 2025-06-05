namespace XYEngine;

public struct Vector4(float x, float y, float z, float w) : IEquatable<Vector4>, ILerpable<Vector4>
{
	public float x { get; set; } = x;
	public float y { get; set; } = y;
	public float z { get; set; } = z;
	public float w { get; set; } = w;
	
	private const float EPSILON = 1e-5f;
	
	public float length => MathF.Sqrt(x * x + y * y + z * z + w * w);
	
	public static Vector4 operator /(Vector4 a, float scalar) => new (a.x / scalar, a.y / scalar, a.z / scalar, a.w / scalar);
	public static Vector4 operator /(float scalar, Vector4 a) => new (scalar / a.x, scalar / a.y, scalar / a.z, scalar / a.w);
	public static Vector4 operator *(Vector4 a, float scalar) => new (a.x * scalar, a.y * scalar, a.z * scalar, a.w * scalar);
	public static Vector4 operator *(float scalar, Vector4 a) => new (scalar * a.x, scalar * a.y, scalar * a.z, scalar * a.w);
	
	public static Vector4 operator +(Vector4 a, Vector4 b) => new (a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	public static Vector4 operator -(Vector4 a, Vector4 b) => new (a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	public static Vector4 operator *(Vector4 a, Vector4 b) => new (a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	public static Vector4 operator /(Vector4 a, Vector4 b) => new (a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
	public static Vector4 operator -(Vector4 a) => new (-a.x, -a.y, -a.z, -a.w);
	
	public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);
	
	public static bool operator ==(Vector4 a, Vector4 b) => MathF.Abs(a.x - b.x) <= EPSILON && MathF.Abs(a.y - b.y) <= EPSILON && MathF.Abs(a.z - b.z) <= EPSILON &&
															MathF.Abs(a.w - b.w) <= EPSILON;
	
	
	public static bool operator >(Vector4 a, Vector4 b) => a.length > b.length;
	public static bool operator <(Vector4 a, Vector4 b) => a.length < b.length;
	public static bool operator >=(Vector4 a, Vector4 b) => a.length >= b.length;
	public static bool operator <=(Vector4 a, Vector4 b) => a.length <= b.length;
	
	public Vector4 Lerp(Vector4 other, float t) => new (
		x + (other.x - x) * t,
		y + (other.y - y) * t,
		z + (other.z - z) * t,
		w + (other.w - w) * t
	);
	
	public override bool Equals(object obj) => obj is Vector4 v && this == v;
	
	public override int GetHashCode() => HashCode.Combine(x, y, z, w);
	
	public override string ToString() => $"({x}, {y}, {z}, {w})";
	
	public bool Equals(Vector4 other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
}