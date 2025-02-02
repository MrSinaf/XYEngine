using Silk.NET.OpenGL;
using static XYEngine.Graphics;
using Texture = XYEngine.Resources.Texture;

namespace XYEngine.Rendering;

public class GProgram : IDisposable
{
	private static uint? currentHandle;
	
	public readonly uint handle = gl.CreateProgram();
	public bool isDisposed { get; protected set; }
	
	private uint vertexHandle;
	private uint fragmentHandle;
	
	public void Use()
	{
		if (currentHandle == handle)
			return;
		
		gl.UseProgram(handle);
		currentHandle = handle;
	}
	
	public void Compile(string vertexSource, string fragmentSource)
	{
		if (!string.IsNullOrEmpty(vertexSource))
			CompileShader(ShaderType.VertexShader, vertexSource);
		
		if (!string.IsNullOrEmpty(fragmentSource))
			CompileShader(ShaderType.FragmentShader, fragmentSource);
		
		gl.LinkProgram(handle);
		gl.GetProgramInfoLog(handle, out var statusInfo);
		gl.GetProgram(handle, ProgramPropertyARB.LinkStatus, out var statusCode);
		
		// Vérifie son statut :
		if (statusCode != 1)
		{
			isDisposed = true;
			gl.DeleteProgram(handle);
			
			throw new InvalidOperationException($"Failed to Link Shader.\n{statusInfo}\n\nStatus Code: {statusCode}");
		}
		
		gl.Flush();
		
		void CompileShader(ShaderType type, string source)
		{
			var shader = gl.CreateShader(type);
			gl.ShaderSource(shader, source);
			gl.CompileShader(shader);
			
			gl.GetShaderInfoLog(shader, out var statusInfo);
			gl.GetShader(shader, ShaderParameterName.CompileStatus, out var statusCode);
			
			// Vérifie son statut :
			if (statusCode != 1)
			{
				isDisposed = true;
				gl.DeleteShader(shader);
				gl.DeleteProgram(handle);
				
				throw new InvalidOperationException($"Failed to Compile {type} Source.\n{statusInfo}\n\nStatus Code: {statusCode}");
			}
			
			gl.AttachShader(handle, shader);
			
			if (type == ShaderType.VertexShader)
				vertexHandle = shader;
			else if (type == ShaderType.FragmentShader)
				fragmentHandle = shader;
		}
	}
	
	public void Decompile()
	{
		gl.DetachShader(handle, vertexHandle);
		gl.DeleteShader(vertexHandle);
		gl.DetachShader(handle, fragmentHandle);
		gl.DeleteShader(fragmentHandle);
		
		gl.Flush();
	}
	
	public void SetUniform(string name, bool value)
	{
		Use();
		if (GetUniformLocation(name, out var location))
			gl.Uniform1(location, value ? 1 : 0);
	}
	
	public void SetUniform(string name, float value)
	{
		Use();
		if (GetUniformLocation(name, out var location))
			gl.Uniform1(location, value);
	}
	
	public void SetUniform(string name, int value)
	{
		Use();
		if (GetUniformLocation(name, out var location))
			gl.Uniform1(location, value);
	}
	
	public void SetUniform(string name, Vector2 value)
	{
		Use();
		if (GetUniformLocation(name, out var location))
			gl.Uniform2(location, value.x, value.y);
	}
	
	public void SetUniform(string name, Region value)
	{
		Use();
		if (GetUniformLocation(name, out var location))
			gl.Uniform4(location, value.position00.x, value.position00.y, value.position11.x, value.position11.y);
	}
	
	public void SetUniform(string name, Matrix3X3 value)
	{
		Use();
		if (GetUniformLocation(name, out var location))
		{
			unsafe
			{
				gl.UniformMatrix3(location, 1, true, (float*)&value);
			}
		}
	}
	
	public void SetUniform(string name, Color color)
	{
		Use();
		if (GetUniformLocation(name, out var location))
			gl.Uniform4(location, color.r * Color.FACTOR, color.g * Color.FACTOR, color.b * Color.FACTOR, color.a * Color.FACTOR);
	}
	
	public void SetUniform(string name, Texture texture, ushort unit = 0)
	{
		Use();
		gl.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + unit));
		texture.gTexture.Bind();
		SetUniform(name, 0);
	}
	
	public string[] GetUniformNames()
	{
		var list = new List<string>();
		
		gl.GetProgram(handle, GLEnum.ActiveUniforms, out var uCount);
		for (var i = 0u; i < uCount; i++)
		{
			var name = gl.GetActiveUniform(handle, i, out _, out _);
			list.Add(name);
			
			// [TEST] Récupère et affiche les variables (ici les vec2) :
			// gl.GetUniform(handle, (int)i, out float fy);
			// gl.GetUniform(handle, (int)i, out float fx);
			// XY.InternalLog("Shader", $"{name} // size : {size} // type : {type.ToString()} // value : ({fx}:{fy})", TypeLog.Info);
		}
		
		return list.ToArray();
	}
	
	private bool GetUniformLocation(string name, out int location)
	{
		location = gl.GetUniformLocation(handle, name);
		var isValid = location != -1;
		// if (!isValid)
		// 	XY.InternalLog("Shader", $"Uniform '{name}' not found in shader program.", TypeLog.Warning);
		
		return isValid;
	}
	
	public void Dispose()
	{
		if (isDisposed)
			return;
		
		if (currentHandle == handle)
			currentHandle = null;
		
		gl.DeleteProgram(handle);
		isDisposed = true;
		GC.SuppressFinalize(this);
	}
}