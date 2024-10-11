using Silk.NET.OpenGL;

namespace XYEngine.Graphics;

public class Render
{
    public Texture texture;
    public Texture texture1;
    public Texture texture2;
    public Mesh mesh;

    public void Draw()
    {
        texture?.Use();
        texture1?.Use(TextureUnit.Texture1);
        texture2?.Use(TextureUnit.Texture2);
        mesh?.Use();
    }
}