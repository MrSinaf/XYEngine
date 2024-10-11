namespace XYEngine;

public struct Color(byte r, byte g, byte b, byte a = 255) : IEquatable<Color>
{
    public const float RATIO = 1F / 255;
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
    
    public static bool operator ==(Color a, Color b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    public static bool operator !=(Color a, Color b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
    
    public bool Equals(Color other) => r == other.r && g == other.g && b == other.b && a == other.a;
    public override bool Equals(object obj) => obj is Color other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(r, g, b, a);
}