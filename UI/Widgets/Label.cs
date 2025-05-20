using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class Label : UIElement
{
	public enum Case { Normal, Lower, Upper }
	
	private bool isDirty;
	
	public Case letterCase
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	public FontBitmap font
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	public string text
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			isDirty = true;
		}
	}
	
	private Label()
	{
		scaleWithoutSize = true;
		base.material = new Material(Shader.GetDefaultUI());
	}
	
	public Label(string text, FontBitmap font) : this()
	{
		this.font = font;
		this.text = text;
	}
	
	public Label(string text = "", string prefab = null) : this()
	{
		this.text = text;
		UIPrefab.Apply(this, prefab);
	}
	
	protected override void OnBeginDraw()
	{
		if (isDirty)
			GenerateText();
		
		material.shader.gProgram.SetUniform("mainTex", font.bitmap);
	}
	
	private void GenerateText()
	{
		if (font == null)
			return;
		
		var meshes = new List<(Rect vertices, Region uvs)>();
		var position = Vector2.zero;
		var text = letterCase switch
		{
			Case.Upper => this.text.ToUpper(),
			Case.Lower => this.text.ToLower(),
			_          => this.text
		};
		var linesHeight = 0;
		foreach (var c in text)
		{
			if (c == '\n')
			{
				linesHeight += font.lineHeight;
				position.y -= font.fontSize + font.baseline;
				position.x = 0;
				continue;
			}
			
			if (!font.glyphs.TryGetValue(c, out var glyph))
				glyph = font.glyphs['☒'];
			
			var x = position.x + glyph.bearing.x;
			var y = position.y - (glyph.height - glyph.bearing.y) + font.baseline;
			
			meshes.Add((new Rect(new Vector2(x, y), glyph.atlasRect.size), glyph.uv));
			position.x += glyph.advanceX;
		}
		
		mesh = MeshFactory.CreateQuads(meshes.ToArray()).Apply();
		size = new Vector2Int((int)mesh.bounds.size.x, linesHeight + font.lineHeight);
		offset = new Vector2Int(0, linesHeight);
		isDirty = false;
	}
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(Label e) => e.font = AssetManager.GetEmbeddedAsset<Font>("fonts.jetbrains.ttf").GetBitmap(16);
}