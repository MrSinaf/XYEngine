using Silk.NET.OpenGL;
using XYEngine.Debugs;
using static XYEngine.GameWindow;

namespace XYEngine.Graphics;

public class Mesh
{
    public readonly uint vertexArray;
    protected bool isInit { private set; get; }

    private readonly uint vertexBuffer;
    private readonly uint elementBuffer;
    private readonly uint uvBuffer;

    protected Vector2[] vertices = [];
    protected uint[] indices = [];
    protected Vector3[] uv = [];
    
    public bool isDisposed { get; private set; }

    protected Mesh()
    {
        vertexArray = gl.GenVertexArray();
        gl.BindVertexArray(vertexArray);

        vertexBuffer = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, vertexBuffer);
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        gl.EnableVertexAttribArray(0);

        uvBuffer = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, uvBuffer);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        gl.EnableVertexAttribArray(1);

        elementBuffer = gl.GenBuffer();
    }

    public unsafe Mesh Apply()
    {
        if (isDisposed)
        {
            Debug.LogIntern("Mesh cannot be used after being disposed !", TypeLog.Error);
            return null;
        }
        
        isInit = true;

        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GLEnum.ArrayBuffer, vertexBuffer);
        fixed (Vector2* ptr = vertices)
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float) * 2), ptr, GLEnum.StaticDraw);
        }

        gl.BindBuffer(GLEnum.ArrayBuffer, uvBuffer);
        fixed (Vector3* ptr = uv)
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(uv.Length * sizeof(float) * 3), ptr, GLEnum.StaticDraw);
        }

        gl.BindBuffer(GLEnum.ElementArrayBuffer, elementBuffer);
        fixed (uint* ptr = indices)
        {
            gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);
        }

        return this;
    }

    internal virtual unsafe void Use(Shader shader)
    {
        if (!isInit)
            throw new Exception("Un Mesh a été utilisé, mais n'a pas utilisé Apply() !");

        gl.BindVertexArray(vertexArray);
        gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    internal void Dispose()
    {
        vertices = null;
        indices = null;
        uv = null;
        
        gl.DeleteBuffer(vertexBuffer);
        gl.DeleteBuffer(elementBuffer);
        gl.DeleteBuffer(uvBuffer);
        gl.DeleteVertexArray(vertexArray);
        isDisposed = true;
    }
}