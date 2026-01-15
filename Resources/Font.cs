using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using XYEngine.Debugs;
using XYEngine.Rendering;
using XYEngine.Utils;
using static FreeTypeSharp.FT;

namespace XYEngine.Resources;

public unsafe class Font : IResource, IDebugProperty
{
	public const string CHARS = "☒0123456789abcdefghijklmnopqrstuvwxyzáàäâãåçéèëêíìïîóòöôõúùüûñABCDEFGHIJKLMNOPQRSTU" +
								"VWXYZÁÀÄÂÃÅÇÉÈËÊÍÌÏÎÓÒÖÔÕÚÙÜÛÑ\"@#&%.:,;_-^*+=\\/|()[]{}<>~`'°€¥$¢£?!¿¡";
	
	private static FT_LibraryRec_* lib;
	private static bool libIsInit;
	
	public string assetPath { get; set; }
	public string name { get; private set; }
	
	private readonly Dictionary<uint, FontBitmap> fontBitmaps = [];
	private FT_FaceRec_* face;
	
	void IResource.Load(Resource ressource)
	{
		if (ressource.extension != ".ttf")
			throw new Exception("The font format must be '.ttf'.");
		
		if (!libIsInit)
		{
			fixed (FT_LibraryRec_** fixedLib = &lib)
			{
				FT_Init_FreeType(fixedLib);
			}
			
			libIsInit = true;
		}
		
		var bytes = ressource.stream.ToByteArray();
		fixed (FT_FaceRec_** fixedFace = &face)
		fixed (byte* bytesPtr = bytes)
		{
			var error = FT_New_Memory_Face(lib, bytesPtr, bytes.Length, 0, fixedFace);
			if (error != 0)
			{
				if (error == FT_Error.FT_Err_Unknown_File_Format)
					throw new Exception("The font format is not supported.");
				
				throw new Exception($"An error occurred while loading the font: {error}");
			}
		}
		
		name = Marshal.PtrToStringAnsi((IntPtr)face->family_name);
	}
	
	public FontBitmap GetBitmap(uint size)
	{
		if (fontBitmaps.TryGetValue(size, out var fontSize))
			return fontSize;
		
		throw new Exception($"The size of a bitmap in {size}px for '{name}' does not exist.");
	}
	
	public FontBitmap GenerateBitmap(uint size, int bitmapSize = 1024)
	{
		XY.InternalLog("Font", $"Generating a bitmap {size}px for '{name}'.");
		
		var bitmap = new Texture2D(bitmapSize, bitmapSize, new Color[bitmapSize * bitmapSize]);
		bitmap.SetFilter(TextureMin.Linear, TextureMag.Linear);
		bitmap.SetWrap(TextureWrap.ClampToEdge);
		
		FT_Set_Pixel_Sizes(face, 0, size);
		var slot = face->glyph;
		
		var maxHeight = 0;
		foreach (var c in CHARS)
		{
			FT_Load_Char(face, c, FT_LOAD.FT_LOAD_RENDER);
			
			var ftBitmap = slot->bitmap;
			var height = (int)ftBitmap.rows;
			var totalHeight = height / 4 + slot->bitmap_top;
			
			maxHeight = Math.Max(maxHeight, totalHeight);
		}
		maxHeight += 2;
		
		var pen = new Vector2Int(0, maxHeight);
		var glyphs = new Dictionary<char, Glyph>();
		var slotAdvanceX = (int)slot->advance.x >> 6;
		
		foreach (var c in CHARS)
		{
			FT_Load_Char(face, c, FT_LOAD.FT_LOAD_RENDER);
			
			var ftBitmap = slot->bitmap;
			var buffer = ftBitmap.buffer;
			var width = ftBitmap.pitch;
			var height = (int)ftBitmap.rows;
			
			if (pen.x + slot->bitmap_left + width > bitmapSize)
			{
				pen.x = 0;
				pen.y += maxHeight;
			}
			
			var glyphX = pen.x + slot->bitmap_left;
			var glyphY = pen.y - slot->bitmap_top;
			
			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
			{
				var alpha = buffer[y * ftBitmap.pitch + x];
				bitmap[glyphX + x, glyphY + y] = new Color(255, 255, 255, alpha);
			}
			
			var uv = new Region(new Vector2(glyphX * bitmap.texel.x, glyphY * bitmap.texel.y),
								new Vector2((glyphX + width) * bitmap.texel.x, (glyphY + height) * bitmap.texel.y));
			var atlasRect = new RectInt(new Vector2Int(glyphX, glyphY), new Vector2Int(width, height));
			
			glyphs.Add(c, new Glyph(atlasRect, uv, new Vector2Int(slot->bitmap_left, slot->bitmap_top),
									(int)(slot->advance.x >> 6)));
			pen.x += (int)size;
		}
		FT_Load_Char(face, ' ', FT_LOAD.FT_LOAD_RENDER);
		glyphs.Add(' ', new Glyph(new RectInt(0, 0, slotAdvanceX, 0), new Region(),
								  new Vector2Int(slot->bitmap_left, slot->bitmap_top), slotAdvanceX));
		bitmap.Apply();
		
		var fontSize = new FontBitmap(size, (int)face->size->metrics.height >> 6,
									  -(int)face->size->metrics.descender >> 6, bitmap,
									  new ReadOnlyDictionary<char, Glyph>(glyphs));
		fontBitmaps.Add(size, fontSize);
		
		return fontSize;
	}
	
	
	public uint[] GetBitmapSizes() => fontBitmaps.Keys.ToArray();
	
	public bool ContainsBitmapSize(uint size) => fontBitmaps.ContainsKey(size);
	
	void IDebugProperty.OnDebugPropertyRender()
	{
		foreach (var (size, fontBitmap) in fontBitmaps)
		{
			if (ImGui.CollapsingHeader($"Bitmap {size}px"))
			{
				ImGui.Indent(10);
				XYDebug.ShowObjectProperties(fontBitmap);
			}
		}
	}
	
	void IAsset.Destroy() => FT_Done_Face(face);
}

public struct Glyph(RectInt atlasRect, Region uv, Vector2Int bearing, int advanceX)
{
	public readonly RectInt atlasRect = atlasRect;
	public readonly Region uv = uv;
	public readonly int width = atlasRect.size.x;
	public readonly int height = atlasRect.size.y;
	
	public readonly Vector2Int bearing = bearing;
	public readonly int advanceX = advanceX;
}

public class FontBitmap(
	uint fontSize,
	int lineHeight,
	int baseline,
	Texture2D bitmap,
	ReadOnlyDictionary<char, Glyph> glyphs
)
{
	public readonly uint fontSize = fontSize;
	public readonly ReadOnlyDictionary<char, Glyph> glyphs = glyphs;
	public readonly Texture2D bitmap = bitmap;
	public int lineHeight = lineHeight;
	public int baseline = baseline;
	
	public int CalculTextSize(string text)
	{
		// TODO > Ne prends pas en charge les retours à la ligne
		
		var width = 0;
		foreach (var c in text)
			width += glyphs[c].advanceX;
		
		return width;
	}
	
	public int GetCharInPositionText(string text, float x)
	{
		var width = 0F;
		for (var i = 0; i < text.Length; i++)
		{
			var glyphWidth = glyphs[text[i]].advanceX;
			
			if (width + glyphWidth * 0.5F > x)
				return i;
			
			width += glyphWidth;
		}
		
		return text.Length;
	}
}