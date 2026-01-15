namespace XYEngine;

public readonly struct Matrix3X3(
	float m11,
	float m21,
	float m31,
	float m12,
	float m22,
	float m32,
	float m13,
	float m23,
	float m33
) : IEquatable<Matrix3X3>
{
	public readonly float m11 = m11;
	public readonly float m12 = m12;
	public readonly float m13 = m13;
	public readonly float m21 = m21;
	public readonly float m22 = m22;
	public readonly float m23 = m23;
	public readonly float m31 = m31;
	public readonly float m32 = m32;
	public readonly float m33 = m33;
	
	public static Matrix3X3 Identity() => new (
		1, 0, 0,
		0, 1, 0,
		0, 0, 1
	);
	
	public Matrix3X3 Inverse()
	{
		var determinant = Determinant();
		if (MathF.Abs(determinant) < float.Epsilon)
			return Identity();
		
		var invDet = 1.0f / determinant;
		
		return new Matrix3X3(
			(m22 * m33 - m32 * m23) * invDet,
			(m31 * m23 - m21 * m33) * invDet,
			(m21 * m32 - m31 * m22) * invDet,
			(m32 * m13 - m12 * m33) * invDet,
			(m11 * m33 - m31 * m13) * invDet,
			(m31 * m12 - m11 * m32) * invDet,
			(m12 * m23 - m22 * m13) * invDet,
			(m21 * m13 - m11 * m23) * invDet,
			(m11 * m22 - m21 * m12) * invDet
		);
	}
	
	public bool TryInverse(out Matrix3X3 result)
	{
		var determinant = Determinant();
		if (MathF.Abs(determinant) < float.Epsilon)
		{
			result = Identity();
			return false;
		}
		
		var invDet = 1.0f / determinant;
		
		result = new Matrix3X3(
			(m22 * m33 - m32 * m23) * invDet,
			(m31 * m23 - m21 * m33) * invDet,
			(m21 * m32 - m31 * m22) * invDet,
			(m32 * m13 - m12 * m33) * invDet,
			(m11 * m33 - m31 * m13) * invDet,
			(m31 * m12 - m11 * m32) * invDet,
			(m12 * m23 - m22 * m13) * invDet,
			(m21 * m13 - m11 * m23) * invDet,
			(m11 * m22 - m21 * m12) * invDet
		);
		
		return true;
	}
	
	public Vector2 TransformPoint(Vector2 point) => new (
		m11 * point.x + m21 * point.y + m31,
		m12 * point.x + m22 * point.y + m32
	);
	
	public static Matrix3X3 CreateTranslation(Vector2 translation) => new (
		1, 0, translation.x,
		0, 1, translation.y,
		0, 0, 1
	);
	
	public static Matrix3X3 CreateScale(Vector2 scale) => new (
		scale.x, 0, 0,
		0, scale.y, 0,
		0, 0, 1
	);
	
	public static Matrix3X3 CreateRotation(float radian)
	{
		var cos = MathF.Cos(radian);
		var sin = MathF.Sin(radian);
		
		return new Matrix3X3(
			cos, -sin, 0,
			sin, cos, 0,
			0, 0, 1
		);
	}
	
	public Matrix3X3 Transpose() => new (
		m11, m21, m31,
		m12, m22, m32,
		m13, m23, m33
	);
	
	public float Determinant()
		=> m11 * (m22 * m33 - m32 * m23) - m21 * (m12 * m33 - m32 * m13) + m31 * (m12 * m23 - m22 * m13);
	
	public static Matrix3X3 CreateOrthographic(float width, float height, bool center = true)
	{
		if (width <= 0 || height <= 0)
			throw new ArgumentException("'width' and 'height' must be strictly positive.");
		
		return new Matrix3X3(
			2 / width, 0, center ? 0 : -1,
			0, 2 / height, center ? 0 : -1,
			0, 0, 1
		);
	}
	
	
	public bool Equals(Matrix3X3 other)
		=> m11.Equals(other.m11) &&
		   m12.Equals(other.m12) &&
		   m13.Equals(other.m13) &&
		   m21.Equals(other.m21) &&
		   m22.Equals(other.m22) &&
		   m23.Equals(other.m23) &&
		   m31.Equals(other.m31) &&
		   m32.Equals(other.m32) &&
		   m33.Equals(other.m33);
	
	public override bool Equals(object obj) => obj is Matrix3X3 other && Equals(other);
	
	public override int GetHashCode()
	{
		var hashCode = new HashCode();
		hashCode.Add(m11);
		hashCode.Add(m12);
		hashCode.Add(m13);
		hashCode.Add(m21);
		hashCode.Add(m22);
		hashCode.Add(m23);
		hashCode.Add(m31);
		hashCode.Add(m32);
		hashCode.Add(m33);
		return hashCode.ToHashCode();
	}
	
	public override string ToString() => $"[{m11}, {m21}, {m31}] / [{m12}, {m22}, {m32}] / [{m13}, {m23}, {m33}]";
	
	public static bool operator ==(Matrix3X3 a, Matrix3X3 b) => a.Equals(b);
	public static bool operator !=(Matrix3X3 a, Matrix3X3 b) => !a.Equals(b);
	
	public static Matrix3X3 operator *(Matrix3X3 a, Matrix3X3 b) => new (
		a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31,
		a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31,
		a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31,
		a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32,
		a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32,
		a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32,
		a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33,
		a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33,
		a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33
	);
}