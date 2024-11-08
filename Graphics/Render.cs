using Silk.NET.OpenGL;

namespace XYEngine.Graphics;

public class Render
{
    /* TODO : Solidifier la class Render
     * J'aimerais la rendre plus fonctionnel, car en vérité elle ne sert à rien, mis à part séparer une petit logique.
     * Il faudrait la rendre plus complète, avec une personnalisation poussé, par exemple avoir des Shaders modifiables efficacement.
     * Peut-être à effectuer via un material.
     */

    public Texture texture;
    public Texture texture1;
    public Texture texture2;
    public Mesh mesh;

    public void Draw(Shader shader)
    {
        texture?.Use();
        texture1?.Use(TextureUnit.Texture1);
        texture2?.Use(TextureUnit.Texture2);
        mesh?.Use(shader);
    }

    public void Dispose()
    {
        texture = null;
        texture1 = null;
        texture2 = null;
        mesh?.Dispose();
    }
}