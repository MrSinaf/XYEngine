using Silk.NET.Maths;
using Silk.NET.OpenGL;
using XYEngine.Debugs;
using static XYEngine.GameWindow;

namespace XYEngine.Graphics;

public class Shader
{
    public readonly uint handle;

    public Shader(string vertex, string frag)
    {
        handle = gl.CreateProgram();
        CompileShader(vertex, frag);
    }

    internal void CompileShader(string vertex, string frag)
    {
        var vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertex);
        gl.CompileShader(vertexShader);

        gl.GetShader(vertexShader, GLEnum.CompileStatus, out var vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception($"Vertex shader failed to compile : {gl.GetShaderInfoLog(vertexShader)}");
        
        var fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, frag);
        gl.CompileShader(fragmentShader);

        gl.GetShader(fragmentShader, GLEnum.CompileStatus, out var fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception($"Fragment shader failed to compile : {gl.GetShaderInfoLog(fragmentShader)}");
        
        gl.AttachShader(handle, vertexShader);
        gl.AttachShader(handle, fragmentShader);
        
        gl.LinkProgram(handle);
        
        gl.GetProgram(handle, GLEnum.LinkStatus, out var hStatus);
        if (hStatus != (int)GLEnum.True)
            throw new Exception($"Program failed to link : {gl.GetProgramInfoLog(handle)}");
        
        gl.DetachShader(handle, vertexShader);
        gl.DetachShader(handle, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }

    private int GetUniformLocation(string name) => gl.GetUniformLocation(handle, name);

    #region Uniform Fonction
    public void SetUniform(string name, float value) => gl.Uniform1(GetUniformLocation(name), value);
    
    public void SetUniform(string name, int value) => gl.Uniform1(GetUniformLocation(name), value);
    
    public void SetUniform(string name, Vector2 value) => gl.Uniform2(GetUniformLocation(name), value.x, value.y);
    
    public void SetUniform(string name, Color value)
        => gl.Uniform4(GetUniformLocation(name), value.r * Color.RATIO, value.g  * Color.RATIO, value.b  * Color.RATIO, value.a  * Color.RATIO);
    
    public void SetUniform(string name, Matrix4X4<float> matrix)
    {
        unsafe
        {
            gl.UniformMatrix4(GetUniformLocation(name), 1, true, (float*)&matrix);
        }
    }
    #endregion

    internal void Use()
    {
        gl.UseProgram(handle);
        if (gl.GetError() != GLEnum.NoError)
            Debug.LogIntern("Shader Error\n" + gl.GetError(), TypeLog.Error);
    }
}