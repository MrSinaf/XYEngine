using XYEngine.Resources;

namespace XYEngine;

public class XYObject
{
	private readonly List<Component> components = [];
	
	public bool enable = true;
	public bool isDestroyed { get; private set; }
	public bool isActif => enable && !isDestroyed;
	public bool canDraw => material != null && mesh is { isValid: true };
	
	public string name;
	public Mesh mesh;
	public Material material;
	public int drawOrder;
	
	private bool dirtyMatrix;
	
	#region GetSet
	
	public Vector2 position
	{
		get;
		set
		{
			field = value;
			dirtyMatrix = true;
		}
	} = Vector2.zero;
	
	public Vector2 scale
	{
		get;
		set
		{
			field = value;
			dirtyMatrix = true;
		}
	} = Vector2.one;
	
	public int rotation
	{
		get;
		set
		{
			field = value;
			dirtyMatrix = true;
		}
	}
	
	public Matrix3X3 matrix
	{
		get
		{
			if (dirtyMatrix)
			{
				field = Matrix3X3.CreateRotation(float.DegreesToRadians(rotation)) * Matrix3X3.CreateScale(scale) * Matrix3X3.CreateTranslation(position);
				dirtyMatrix = false;
			}
			
			return field;
		}
	} = Matrix3X3.Identity();
	
	#endregion
	
	public XYObject()
	{
		name = GetType().Name;
		Scene.AddObject(this);
	}
	
	public T AddComponent<T>() where T : Component
	{
		var component = Activator.CreateInstance<T>();
		component.AddOwner(this);
		components.Add(component);
		return component;
	}
	
	public void RemoveComponent<T>(T component) where T : Component
	{
		if (components.Remove(component))
			component.RemoveOwner();
	}
	
	public T GetComponent<T>() where T : Component => components.OfType<T>().FirstOrDefault();
	
	public T[] GetComponents<T>() where T : Component => components.OfType<T>().ToArray();
	
	protected internal virtual void Update() { }
	protected virtual void Destroyed() { }
	
	public void Destroy()
	{
		foreach (var component in components)
			component.Destroy();
		components.Clear();
		isDestroyed = true;
		Destroyed();
	}
	
	internal void InternalUpdate()
	{
		foreach (var component in components)
			if (component.enable)
				component.Update();
		
		Update();
	}
	
	internal void BeginDraw() => material.shader.gProgram.SetUniform("model", matrix);
}