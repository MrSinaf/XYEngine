using XYEngine.Graphics;
using XYEngine.Graphics.Fonts;

namespace XYEngine.UI.Widgets;

public class Label : UIElement
{
    private readonly Font font;
    public string text { get => _text; set => ConstructText(value); }
    private string _text;

    public Label()
    {
        scaleWithSize = false;
        font = new Font();
    }

    private void ConstructText(string text)
    {
        if (text == _text) return;

        _text = text;
        render.texture = font.texture;
        render.mesh = GenerateMesh();
    }

    private Mesh GenerateMesh()
    {
        var meshQuad = new MeshQuad(_text.Length);
        scale = new Vector2(0.5F);
        
        var advance = Vector2.zero;
        for (var c = 0; c < _text.Length; c++)
        {
            if (_text[c] == ' ')
            {
                advance.x += 10;
            }
            else if (font.characters.TryGetValue(_text[c], out var character))
            {
                meshQuad.SetQuad(c, advance + new Vector2(0, character.bearing.y), character.size, character.uv.position00, character.uv.position11);
                advance.x += character.bearing.x + 2;
            }
        }

        advance.y += font.lineHeight;
        size = advance.ToVector2Int();
        return meshQuad.Apply();
    }
}