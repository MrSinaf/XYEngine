using Silk.NET.Core;
using Silk.NET.Input;
using XYEngine.Resources;
using static XYEngine.Inputs.Input;

namespace XYEngine;

public enum CursorMode { Normal, Hidden, Disabled, Raw }

public static class Cursor
{
	private static readonly List<(Vector2Int offset, RawImage raw)> textures = [];
	
	public static CursorMode mode
	{
		get => (CursorMode)mouse.Cursor.CursorMode;
		set => mouse.Cursor.CursorMode = (Silk.NET.Input.CursorMode)value;
	}
	
	public static bool isConfined { get => mouse.Cursor.IsConfined; set => mouse.Cursor.IsConfined = value; }
	
	public static bool isCustom
	{
		get => mouse.Cursor.Type == CursorType.Custom;
		set => mouse.Cursor.Type = value ? CursorType.Custom : CursorType.Standard;
	}
	
	public static void AddTexture(Texture2D texture, Vector2Int offset = new ())
	{
		var pixels = texture.pixels;
		var bytes = new byte[texture.width * texture.height * 4];
		for (var i = 0; i < pixels.Length; i++)
		{
			var index = i * 4;
			var color = pixels[i];
			bytes[index] = color.r;
			bytes[index + 1] = color.g;
			bytes[index + 2] = color.b;
			bytes[index + 3] = color.a;
		}
		
		textures.Add((offset, new RawImage((int)texture.width, (int)texture.height, bytes)));
	}
	
	public static void SetTexture(int index)
	{
		var (offset, raw) = textures[index];
		mouse.Cursor.Image = raw;
		mouse.Cursor.HotspotX = offset.x;
		mouse.Cursor.HotspotY = offset.y;
	}
}