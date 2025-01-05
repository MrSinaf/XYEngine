namespace XYEngine;

public static class SceneManager
{
	public static Scene current { get; private set; }
	
	public static void SetCurrentScene<T>() where T : Scene, new()
	{
		current?.InternalDestroy();
		
		current = new T();
		current.InternalStart();
	}
	
	public static void SetCurrentScene(Type type)
	{
		if (!type.IsSubclassOf(typeof(Scene)))
		{
			XY.InternalLog("Scene Manager", $"The specified type ({type.Name}) must inherit from Scene.");
			return;
		}
		
		current?.InternalDestroy();
		
		try
		{
			current = (Scene)Activator.CreateInstance(type)!;
		}
		catch
		{
			XY.InternalLog("Scene Manager", $"The type {type.Name} must have a parameterless constructor.");
			return;
		}
		
		current.InternalStart();
	}
	
	internal static void Update() => current.InternalUpdate();
	
	internal static void Render() => current.InternalRender();
	
	internal static void Dispose()
	{
		current?.InternalDestroy();
		current = null;
	}
}