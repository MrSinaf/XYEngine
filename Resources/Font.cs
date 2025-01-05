using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using XYEngine.Rendering;
using XYEngine.Utils;
using static FreeTypeSharp.FT;

namespace XYEngine.Resources;

public unsafe class Font : IAsset
{
	public const string CHARS = "☒0123456789abcdefghijklmnopqrstuvwxyzáàäâãåçéèëêíìïîóòöôõúùüûñABCDEFGHIJKLMNOPQRSTUVWXYZÁÀÄÂÃÅÇÉÈËÊÍÌÏÎÓÒÖÔÕÚÙÜÛÑ\"@#&%.:,;_-^*+=\\/|()[]{}<>" +
								"~`'°€¥$¢£?!¿¡";
	
	private static FT_LibraryRec_* lib;
	private static bool libIsInit;
	
	public string name { get; private set; }
	
	private readonly Dictionary<uint, FontBitmap> fontBitmaps = [];
	private FT_FaceRec_* face;
	
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
		
		var glyphs = new Dictionary<char, Glyph>();
		
		FT_Set_Pixel_Sizes(face, 0, size);
		
		// Ajoute l'espace pour... espacer ᓚᘏᗢ
		FT_Load_Glyph(face, FT_Get_Char_Index(face, ' '), FT_LOAD.FT_LOAD_DEFAULT);
		// TODO: J'ai ajouté 0.75F, pour rendre l'espacement moins grand, car je le trouve très grand pour rien, (#｀-_ゝ-) il est clair que ce n'est pas la bonne méthode...
		glyphs.Add(' ', new Glyph(new Rect(Vector2.zero, new Vector2((face->glyph->advance.x >> 6) * 0.75F, 0)), new Region()));
		
		var maxHeight = 0;
		var position = Vector2Int.zero;
		var baseLine = 0;
		foreach (var c in CHARS)
		{
			FT_Load_Glyph(face, FT_Get_Char_Index(face, c), FT_LOAD.FT_LOAD_DEFAULT);
			FT_Render_Glyph(face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
			var ftBitmap = face->glyph->bitmap;
			var width = (int)ftBitmap.width;
			var height = (int)ftBitmap.rows;
			
			if (position.x + width > bitmap.width)
			{
				position.x = 0;
				position.y += maxHeight + 1;
			}
			
			if (height > maxHeight)
				maxHeight = height;
			
			var buffer = ftBitmap.buffer;
			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
			{
				var grayscale = buffer[y * ftBitmap.pitch + x];
				bitmap[position.x + x, position.y + height - y - 1] = new Color(255, 255, 255, grayscale);
			}
			
			var offsetY = face->glyph->metrics.height - face->glyph->metrics.horiBearingY >> 6;
			if (baseLine < offsetY)
				baseLine = (int)offsetY;
			
			glyphs.Add(c, new Glyph(new Rect(new Vector2(0, -offsetY), new Vector2(width, height)),
									new Region(position * bitmap.texel, (position + new Vector2(width, height)) * bitmap.texel)));
			
			position.x += width + 1;
		}
		
		bitmap.Apply();
		
		var fontSize = new FontBitmap(size, baseLine, bitmap, new ReadOnlyDictionary<char, Glyph>(glyphs));
		fontBitmaps.Add(size, fontSize);
		
		return fontSize;
	}
	
	public uint[] GetBitmapSizes() => fontBitmaps.Keys.ToArray();
	public bool ContainsBitmapSize(uint size) => fontBitmaps.ContainsKey(size);
	
	void IAsset.Load(AssetProperty property)
	{
		if (property.extension != ".ttf")
			throw new Exception("The font format must be '.ttf'.");
		
		if (!libIsInit)
		{
			fixed (FT_LibraryRec_** fixedLib = &lib)
			{
				FT_Init_FreeType(fixedLib);
			}
			
			libIsInit = true;
		}
		
		var bytes = property.stream.ToByteArray();
		fixed (FT_FaceRec_** fixedFace = &face)
		fixed (byte* ptr = bytes)
		{
			FT_New_Memory_Face(lib, ptr, bytes.Length, 0, fixedFace);
		}
		
		name = Marshal.PtrToStringAnsi((IntPtr)face->family_name);
	}
	
	void IAsset.UnLoad() => FT_Done_Face(face);
}

public struct Glyph(Rect position, Region uv)
{
	public readonly Rect position = position;
	public readonly Region uv = uv;
}

public class FontBitmap(uint fontSize, int baseLine, Texture2D bitmap, ReadOnlyDictionary<char, Glyph> glyphs)
{
	public readonly uint fontSize = fontSize;
	public readonly int baseLine = baseLine;
	public readonly ReadOnlyDictionary<char, Glyph> glyphs = glyphs;
	public readonly Texture2D bitmap = bitmap;
}