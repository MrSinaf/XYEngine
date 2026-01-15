using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Material : IAsset
{
	public readonly Dictionary<string, object> properties = [];
	public readonly Shader shader;
	
	public Topology topology = Topology.Triangles;
	
	public Material(Shader shader, params (string, object)[] properties)
	{
		this.shader = shader;
		shader.config?.onCreate(this);
		
		foreach (var (name, value) in properties)
			SetProperty(name, value);
	}
	
	public void SetProperty(string name, object value)
	{
		shader.config?.onPropertyAdd(this, name, value);
		properties[name] = value;
	}
	
	public T GetProperty<T>(string name) => properties[name].GetType() == typeof(T) ? (T)properties[name] : default;
	
	internal void ApplyProperties()
	{
		foreach (var (name, value) in properties)
		{
			switch (value)
			{
				case bool b:
					shader.gProgram.SetUniform(name, b);
					break;
				case float f:
					shader.gProgram.SetUniform(name, f);
					break;
				case int i:
					shader.gProgram.SetUniform(name, i);
					break;
				case Vector2 v2:
					shader.gProgram.SetUniform(name, v2);
					break;
				case Vector2Int v2:
					shader.gProgram.SetUniform(name, v2);
					break;
				case Vector3 v3:
					shader.gProgram.SetUniform(name, v3);
					break;
				case Vector4 v4:
					shader.gProgram.SetUniform(name, v4);
					break;
				case Rect v4:
					shader.gProgram.SetUniform(name, v4);
					break;
				case Region v4:
					shader.gProgram.SetUniform(name, v4);
					break;
				case RegionInt v4:
					shader.gProgram.SetUniform(name, v4);
					break;
				case Matrix3X3 mat3:
					shader.gProgram.SetUniform(name, mat3);
					break;
				case Color color:
					shader.gProgram.SetUniform(name, color);
					break;
				case Texture texture:
					shader.gProgram.SetUniform(name, texture);
					break;
				default:
					XY.InternalLog("Material", $"The property '{name}' is of an unsupported type ({value.GetType()}).",
								   TypeLog.Warning);
					break;
			}
		}
	}
	
	public Material Copy()
	{
		var newMaterial = new Material(shader);
		foreach (var (key, value) in properties)
			newMaterial.properties.Add(key, value);
		
		return newMaterial;
	}
}