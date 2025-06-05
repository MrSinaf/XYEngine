namespace XYEngine;

public struct Vector3(float x, float y, float z) : IEquatable<Vector3>, ILerpable<Vector3>
{
	public float x { get; set; } = x;
	public float y { get; set; } = y;
	public float z { get; set; } = z;
	
	private const float EPSILON = 1e-5f;
	
	public float length => MathF.Sqrt(x * x + y * y + z * z);
	
	public static Vector3 operator /(Vector3 a, float scalar) => new (a.x / scalar, a.y / scalar, a.z / scalar);
	public static Vector3 operator /(float scalar, Vector3 a) => new (scalar / a.x, scalar / a.y, scalar / a.z);
	public static Vector3 operator *(Vector3 a, float scalar) => new (a.x * scalar, a.y * scalar, a.z * scalar);
	public static Vector3 operator *(float scalar, Vector3 a) => new (scalar * a.x, scalar * a.y, scalar * a.z);
	
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
	
	public static bool operator >(Vector3 a, Vector3 b) => a.length > b.length;
	public static bool operator <(Vector3 a, Vector3 b) => a.length < b.length;
	public static bool operator >=(Vector3 a, Vector3 b) => a.length >= b.length;
	public static bool operator <=(Vector3 a, Vector3 b) => a.length <= b.length;
	
	public Vector3 Lerp(Vector3 other, float t) => new (
		x + (other.x - x) * t,
		y + (other.y - y) * t,
		z + (other.z - z) * t
	);
	
	public override bool Equals(object obj) => obj is Vector3 v && this == v;
	
	public override int GetHashCode() => HashCode.Combine(x, y, z);
	
	public override string ToString() => $"({x}, {y}, {z})";
	
	public bool Equals(Vector3 other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
}