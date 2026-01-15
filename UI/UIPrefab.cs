using System.Reflection;
using JetBrains.Annotations;

namespace XYEngine.UI;

public static class UIPrefab
{
	private static readonly Dictionary<(Type, string), Action<UIElement>> prefabs = [];
	
	public static void Add<T>(string name, Action<T> action) where T : UIElement
		=> prefabs[(typeof(T), name)] = element => action(element as T);
	
	public static void Remove<T>(string name) where T : UIElement => prefabs.Remove((typeof(T), name));
	
	public static bool Contains<T>(string name) where T : UIElement => prefabs.ContainsKey((typeof(T), name));
	
	public static void Apply<T>(T element, string name) where T : UIElement
	{
		if (!prefabs.TryGetValue((typeof(T), name), out var action))
		{
			if (name == null)
			{
				// Regarde si une méthode DefaultPrefab existe pour le type T (‾◡◝)
				var method = typeof(T).GetMethods()
									  .FirstOrDefault(method => method.GetCustomAttribute<IsDefaultPrefab>() != null);
				if (method != null)
				{
					Add<T>(null, element => method.Invoke(null, [element]));
					method.Invoke(null, [element]);
					return;
				}
			}
			
			throw new NullReferenceException(
				$"The {(name == null ? "default prefab" : $"prefab '{name}'")} for {typeof(T)} is not found!");
		}
		
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

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class IsDefaultPrefab : Attribute;