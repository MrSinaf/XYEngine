namespace XYEngine;

public abstract class Component
{
	public virtual XYObject owner { get; private set; }
	public bool enable = true;
	
	public virtual void Start() { }
	public virtual void Update() { }
	public virtual void Destroy() { }
	
	internal void AddOwner(XYObject obj)
	{
		owner = obj;
		Start();
	}
	
	internal void RemoveOwner()
	{
		owner = null;
		Destroy();
	}
	
	public T GetComponent<T>() where T : Component => owner?.GetComponent<T>();
	public T[] GetComponents<T>() where T : Component => owner?.GetComponents<T>();
}