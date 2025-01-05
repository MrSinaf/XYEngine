namespace XYEngine;

public struct Region(Vector2 position00, Vector2 position11)
{
	public Vector2 position00 { get; set; } = position00;
	public Vector2 position11 { get; set; } = position11;
	
	public Region(Vector2 position) : this(position, position) { }
	public Region(float x, float y, float z, float w) : this(new Vector2(x, y), new Vector2(z, w)) { }
	
	public override string ToString() => $"[pos00: {position00} pos11: {position11}]";
}