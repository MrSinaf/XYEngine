using XYEngine.Graphics;
using XYEngine.Graphics.Fonts;

namespace XYEngine.UI.Widgets;

public class Label : UIElement
{
    private readonly Font font;
    public string text { get => _text; set => ConstructText(value); }

    public override Vector2 scale { get => base.scale; set => throw new Exception("It is not allowed to modify the scale of a Label."); }
    public override Vector2Int size { get => base.size; set => throw new Exception("It is not allowed to modify the size of a Label."); }

    public int fontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize == value)
                return;

            _fontSize = value;
            base.scale = new Vector2(1F * value / font.lineHeight);
        }
    }
    
    private string _text;
    private int _fontSize;

    public Label()
    {
        scaleWithSize = false;
        font = new Font("upheavtt");
        font.texture.SetFilter(TextureFilter.Linear);
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
        var advance = Vector2.zero;
        for (var c = 0; c < _text.Length; c++)
        {
            if (_text[c] == ' ')
            {
                advance.x += 10;
            }
            else if (font.characters.TryGetValue(_text[c], out var character))
            {
                if (c == 0)
                    advance.x += character.start;
                
                meshQuad.SetQuad(c, advance + new Vector2(0, character.bearing.y), character.size, character.uv.position00, character.uv.position11);
                advance.x += character.bearing.x;
            }
        }

        advance.y += font.lineHeight;
        base.size = advance.ToVector2Int();
        return meshQuad.Apply();
    }

    // TODO : C'est une option temporaire :
    protected internal override void Draw(Shader shader)
    {
        shader.Use();
        shader.SetUniform("param", 2);
        base.Draw(shader);
        shader.SetUniform("param", 0);
    }
}