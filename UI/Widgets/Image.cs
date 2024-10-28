using XYEngine.Graphics;

namespace XYEngine.UI.Widgets;

public class Image : UIElement
{
    public Image(Texture texture)
    {
        render.mesh = new MeshQuad().SetQuad(0, Vector2.zero, Vector2.one, Vector2.zero, Vector2.one).Apply();
        render.texture = texture;
        size = texture.size;
    }

    public Image(TextureAtlas texture, string name)
    {
        if (!texture.GetUV(name, out var uv))
            throw new Exception($"{name} does not exist.");
        
        render.mesh = new MeshQuad().SetQuad(0, Vector2.zero, Vector2.one, uv.position00, uv.position11).Apply();
        render.texture = texture;
        size = texture.data.frames[name].size;
    }
}