﻿namespace XYEngine;

public struct Matrix3X3(float m11, float m21, float m31, float m12, float m22, float m32, float m13, float m23, float m33)
{
	public float m11 = m11;
	public float m12 = m12;
	public float m13 = m13;
	public float m21 = m21;
	public float m22 = m22;
	public float m23 = m23;
	public float m31 = m31;
	public float m32 = m32;
	public float m33 = m33;
	
	public static Matrix3X3 Identity() => new (
	1, 0, 0,
	0, 1, 0,
	0, 0, 1
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
	
	public override string ToString() => $"[{m11}, {m21}, {m31}] / [{m12}, {m22}, {m32}] / [{m13}, {m23}, {m33}]";
}