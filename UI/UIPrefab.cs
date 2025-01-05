namespace XYEngine.UI;

public static class UIPrefab
{
	private static readonly Dictionary<(Type, string), Action<UIElement>> prefabs = [];
	
	public static void Add<T>(string name, Action<T> action) where T : UIElement => prefabs[(typeof(T), name)] = element => action(element as T);
	
	public static void Remove<T>(string name) where T : UIElement => prefabs.Remove((typeof(T), name));
	
	public static bool Contains<T>(string name) where T : UIElement => prefabs.ContainsKey((typeof(T), name));
	
	public static void Apply<T>(T element, string name) where T : UIElement
	{
		if (!prefabs.TryGetValue((typeof(T), name), out var action))
			throw new NullReferenceException($"The {(name == null ? "default prefab" : $"prefab '{name}'")} for {typeof(T)} is not found!");
		
		action.Invoke(element);
	}
	
	public static bool TryApply<T>(T element, string name) where T : UIElement
	{
		var found = prefabs.TryGetValue((typeof(T), name), out var action);
		if (found)
			action.Invoke(element);
		
		return found;
	}
}