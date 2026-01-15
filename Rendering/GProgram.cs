using System.Text.RegularExpressions;
using Silk.NET.OpenGL;
using static XYEngine.Graphics;
using Texture = XYEngine.Resources.Texture;

namespace XYEngine.Rendering;

public class GProgram : IDisposable
{
	private static uint? currentHandle;
	
	private readonly Dictionary<string, int> uniformLocations = [];
	private readonly Dictionary<string, int> attribLocations = [];
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
			Decompile();
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
				Decompile();
				var match = Regex.Match(statusInfo, @"\((\d+)\)");
				if (!match.Success)
					throw new InvalidOperationException($"Failed to Compile {type} Source.\n{statusInfo}\n" +
														$"Status Code: {statusCode}");
				
				var lineIndex = int.Parse(match.Groups[1].Value) - 1;
				var lines = fragmentSource.Replace("\r\n", "\n").Split('\n');
				
				throw new InvalidOperationException($"Failed to Compile {type} Source." +
													$"\n{statusInfo}```\n{lines[lineIndex]}\n```\n" +
													$"Status Code: {statusCode}");
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
	
	public void GetUniform(string name, out bool value)
	{
		value = false;
		if (GetUniformLocation(name, out var location))
		{
			gl.GetUniform(handle, location, out int data);
			value = data != 0;
		}
	}
	
	public void SetUniform(string name, float value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform1(location, value);
	}
	
	public void GetUniform(string name, out float value)
	{
		value = 0;
		if (GetUniformLocation(name, out var location))
			gl.GetUniform(handle, location, out value);
	}
	
	public void SetUniform(string name, int value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform1(location, value);
	}
	
	public void GetUniform(string name, out int value)
	{
		value = 0;
		if (GetUniformLocation(name, out var location))
			gl.GetUniform(handle, location, out value);
	}
	
	public void SetUniform(string name, Vector2 value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform2(location, value.x, value.y);
	}
	
	public void GetUniform(string name, out Vector2 value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[2];
			gl.GetUniform(handle, location, data);
			value = new Vector2(data[0], data[1]);
		}
	}
	
	public void SetUniform(string name, Vector3 value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform3(location, value.x, value.y, value.z);
	}
	
	public void GetUniform(string name, out Vector3 value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[3];
			gl.GetUniform(handle, location, data);
			value = new Vector3(data[0], data[1], data[2]);
		}
	}
	
	public void SetUniform(string name, Vector4 value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform4(location, value.x, value.y, value.z, value.w);
	}
	
	public void GetUniform(string name, out Vector4 value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[4];
			gl.GetUniform(handle, location, data);
			value = new Vector4(data[0], data[1], data[2], data[3]);
		}
	}
	
	public void SetUniform(string name, Region value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform4(location, value.position00.x, value.position00.y, value.position11.x, value.position11.y);
	}
	
	public void GetUniform(string name, out Region value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[4];
			gl.GetUniform(handle, location, data);
			value = new Region(data[0], data[1], data[2], data[3]);
		}
	}
	
	public void SetUniform(string name, Rect value)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
			gl.Uniform4(location, value.position.x, value.position.y, value.size.x, value.size.y);
	}
	
	public void GetUniform(string name, out Rect value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[4];
			gl.GetUniform(handle, location, data);
			value = new Rect(data[0], data[1], data[2], data[3]);
		}
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
	
	public void GetUniform(string name, out Matrix3X3 value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[9];
			gl.GetUniform(handle, location, data);
			value = new Matrix3X3(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8]);
		}
	}
	
	public void SetUniform(string name, Color color)
	{
		Use();
		
		if (GetUniformLocation(name, out var location))
		{
			var (r, g, b, a) = color.ToFloats();
			gl.Uniform4(location, r, g, b, a);
		}
	}
	
	public void GetUniform(string name, out Color value)
	{
		value = default;
		if (GetUniformLocation(name, out var location))
		{
			var data = new float[4];
			gl.GetUniform(handle, location, data);
			value = new Color(data[0], data[1], data[2], data[3]);
		}
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
		}
		
		return list.ToArray();
	}
	
	public (string name, Type type, int arraySize)[] GetUniforms()
	{
		var list = new List<(string, Type, int)>();
		
		gl.GetProgram(handle, GLEnum.ActiveUniforms, out var uCount);
		
		for (var i = 0u; i < uCount; i++)
		{
			var name = gl.GetActiveUniform(handle, i, out _, out var uniformType);
			var (type, arraySize) = UnitformTypeToType(uniformType);
			list.Add((name, type, arraySize));
		}
		
		return list.ToArray();
		
		(Type, int) UnitformTypeToType(UniformType type) => type switch
		{
			UniformType.Int             => (typeof(int), 1),
			UniformType.UnsignedInt     => (typeof(uint), 1),
			UniformType.Float           => (typeof(float), 1),
			UniformType.Double          => (typeof(double), 1),
			UniformType.FloatVec2       => (typeof(Vector2), 1),
			UniformType.FloatVec3       => (typeof(Vector3), 1),
			UniformType.FloatVec4       => (typeof(Color), 1),
			UniformType.IntVec2         => (typeof(Vector2Int), 1),
			UniformType.IntVec3         => (typeof(int), 3),
			UniformType.IntVec4         => (typeof(int), 4),
			UniformType.UnsignedIntVec2 => (typeof(Vector2Int), 1),
			UniformType.UnsignedIntVec3 => (typeof(uint), 3),
			UniformType.UnsignedIntVec4 => (typeof(uint), 3),
			UniformType.Bool            => (typeof(bool), 1),
			UniformType.BoolVec2        => (typeof(bool), 2),
			UniformType.BoolVec3        => (typeof(bool), 3),
			UniformType.BoolVec4        => (typeof(bool), 4),
			UniformType.FloatMat3       => (typeof(Matrix3X3), 1),
			_                           => (null, 0)
		};
	}
	
	public bool GetUniformLocation(string name, out int location)
	{
		if (uniformLocations.TryGetValue(name, out location))
			return location != -1;
		
		location = gl.GetUniformLocation(handle, name);
		uniformLocations[name] = location;
		return location != -1;
	}
	
	public bool GetAttribLocation(string name, out int location)
	{
		if (attribLocations.TryGetValue(name, out location))
			return location != -1;
		
		location = gl.GetAttribLocation(handle, name);
		attribLocations[name] = location;
		return location != -1;
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