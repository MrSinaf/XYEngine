using Silk.NET.OpenGL;
using static XYEngine.GameWindow;

namespace XYEngine.Graphics;

public class MeshLine : Mesh
{
    public MeshLine(Vector2[] lines)
    {
        vertices = lines;
        uv = [Vector3.zero];    // Permet d'éviter une erreur par rapport au Shader.
        indices = new uint[lines.Length];
        for (uint i = 0; i < lines.Length; i++)
            indices[i] = i;

        Apply();
    }

    internal override unsafe void Use(Shader shader = null)
    {
        if (!isInit)
            throw new Exception("Un LineMesh a été utilisé, mais n'a pas utilisé Apply() !");
        
        // TODO : C'est pour le moment temporaire :
        gl.BindVertexArray(vertexArray);
        shader?.SetUniform("param", 1);
        gl.DrawElements(PrimitiveType.LineLoop, (uint)indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        shader?.SetUniform("param", 0);
    }
}