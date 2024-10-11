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
}