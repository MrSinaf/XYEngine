using System.Runtime.InteropServices;

namespace XYEngine;

public struct Color(byte r, byte g, byte b, byte a = byte.MaxValue) : IEquatable<Color>
{
	public const float FACTOR = 1 / 255F;
	public static readonly Color white = new (255, 255, 255);
	public static readonly Color grey = new (200, 200, 200);
	public static readonly Color red = new (255, 0, 0);
	public static readonly Color green = new (0, 255, 0);
	public static readonly Color blue = new (0, 0, 255);
	public static readonly Color black = new (0, 0, 0);
	
	public byte r = r;
	public byte g = g;
	public byte b = b;
	public byte a = a;
	
	public Color(uint hex) : this((byte)(hex >> 16 & 255), (byte)(hex >> 8 & 255), (byte)(hex & 255), hex > 0xFFFFFF ? (byte)(hex >> 24 & 255) : byte.MaxValue) { }
	
	public static Color[] ConvertBytesToColors(Span<byte> bytes)
	{
		if (bytes.Length % 4 != 0)
			throw new ArgumentException("Byte array length must be a multiple of 4.");
		
		return MemoryMarshal.Cast<byte, Color>(bytes).ToArray();
	}
	
	public static bool operator ==(Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
	public static bool operator !=(Color a, Color b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
	
	public override string ToString() => $"rgba({r}, {g}, {b}, {a})";
	public bool Equals(Color other) => r == other.r && g == other.g && b == other.b && a == other.a;
	public override bool Equals(object obj) => obj is Color other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(r, g, b, a);
}