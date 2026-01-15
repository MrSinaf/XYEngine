using XYEngine.UI;

namespace XYEngine;

public abstract class Scene
{
	public static Scene current => SceneManager.current;
	
	private readonly List<XYObject> objects = [];
	
	public Canvas canvas { get; private set; }
	public Camera camera;
	
	internal void InternalStart()
	{
		Time.ResetTime();
		Start();
	}
	
	internal void InternalBuildUI()
	{
		canvas = new Canvas();
		BuildUI(canvas.root);
	}
	
	internal void InternalUpdate()
	{
		Update();
		for (var i = 0; i < objects.Count; i++)
		{
			var obj = objects[i];
			
			if (obj.isDestroyed)
			{
				objects.RemoveAt(i--);
				continue;
			}
			
			if (!obj.isActif)
				continue;
			
			obj.InternalUpdate();
		}
	}
	
	internal void InternalRender()
	{
		Graphics.BeginDraw();
		camera?.Render(objects);
		canvas.Render();
		Graphics.EndDraw();
	}
	
	internal void InternalDestroy()
	{
		Destroy();
		canvas.Destroy();
		camera?.Destroy();
		
		foreach (var obj in objects)
			obj.Destroy();
	}
	
	internal static void AddObject(XYObject obj) => SceneManager.current.objects.Add(obj);
	
	protected virtual void Start() { }
	protected virtual void BuildUI(RootElement root) { }
	protected virtual void Destroy() { }
	protected virtual void Update() { }
}