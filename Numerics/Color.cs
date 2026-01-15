using System.Runtime.InteropServices;

namespace XYEngine;

[StructLayout(LayoutKind.Sequential)]
public struct Color(byte r, byte g, byte b, byte a = byte.MaxValue) : IEquatable<Color>, ILerpable<Color>
{
	public const float FACTOR = 1 / 255F;
	public static readonly Color white = new (255, 255, 255);
	public static readonly Color grey = new (200, 200, 200);
	public static readonly Color red = new (255, 0, 0);
	public static readonly Color green = new (0, 255, 0);
	public static readonly Color blue = new (0, 0, 255);
	public static readonly Color black = new (0, 0, 0);
	public static readonly Color yellow = new (255, 255, 0);
	public static readonly Color cyan = new (0, 255, 255);
	public static readonly Color magenta = new (255, 0, 255);
	public static readonly Color orange = new (255, 165, 0);
	public static readonly Color empty = new (0, 0, 0, 0);
	
	public byte r = r;
	public byte g = g;
	public byte b = b;
	public byte a = a;
	
	public Color(uint hex) : this((byte)(hex >> 16 & 255), (byte)(hex >> 8 & 255), (byte)(hex & 255),
								  hex > 0xFFFFFF ? (byte)(hex >> 24 & 255) : byte.MaxValue) { }
	
	public Color(float r, float g, float b, float a = 1) : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255),
																(byte)(a * 255)) { }
	
	public Color Lerp(Color other, float t) => new (
		(byte)(r + (other.r - r) * t),
		(byte)(g + (other.g - g) * t),
		(byte)(b + (other.b - b) * t),
		(byte)(a + (other.a - a) * t)
	);
	
	public (float r, float g, float b, float a) ToFloats() => (r * FACTOR, g * FACTOR, b * FACTOR, a * FACTOR);
	public uint ToUInt32() => (uint)(a << 24 | r << 16 | g << 8 | b);
	public string ToHex() => $"#{r:X2}{g:X2}{b:X2}{a:X2}";
	
	public static Color[] FromBytes(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length % 4 != 0)
			throw new ArgumentException("Byte array length must be a multiple of 4.");
		
		return MemoryMarshal.Cast<byte, Color>(bytes).ToArray();
	}
	
	public static Color FromHex(string hex)
	{
		hex = hex.TrimStart('#');
		return hex.Length switch
		{
			6 => new Color(Convert.ToUInt32(hex + "FF", 16)),
			8 => new Color(Convert.ToUInt32(hex, 16)),
			_ => throw new ArgumentException("Invalid hex color format.", nameof(hex))
		};
	}
	
	public bool Equals(Color other) => r == other.r && g == other.g && b == other.b && a == other.a;
	public override bool Equals(object obj) => obj is Color other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(r, g, b, a);
	public override string ToString() => $"rgba({r}, {g}, {b}, {a})";
	
	public static bool operator ==(Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
	public static bool operator !=(Color a, Color b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
	
	public static Color operator *(Color c, float factor)
		=> new ((byte)Math.Clamp(c.r * factor, 0, 255),
				(byte)Math.Clamp(c.g * factor, 0, 255),
				(byte)Math.Clamp(c.b * factor, 0, 255),
				c.a);
	
	public static Color operator +(Color a, Color b)
		=> new ((byte)Math.Min(a.r + b.r, 255),
				(byte)Math.Min(a.g + b.g, 255),
				(byte)Math.Min(a.b + b.b, 255),
				(byte)Math.Min(a.a + b.a, 255));
	
	public static Color operator *(Color a, Color b)
		=> new ((byte)(a.r * b.r / 255),
				(byte)(a.g * b.g / 255),
				(byte)(a.b * b.b / 255),
				(byte)(a.a * b.a / 255));
}