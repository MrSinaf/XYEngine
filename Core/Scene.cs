﻿using XYEngine.UI;

namespace XYEngine;

public abstract class Scene
{
	public static Scene current => SceneManager.current;
	
	private readonly List<XYObject> objects = [];
	public Camera camera { get; private set; }
	public Canvas canvas { get; private set; }
	
	internal void InternalStart()
	{
		Time.ResetTime();
		camera = new Camera(objects);
		
		Start();
	}
	
	internal void InternalBuildUI()
	{
		canvas = new Canvas();
		BuildUI(canvas.root);
	}
	
	internal void InternalUpdate()
	{
		camera.Update();
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
			
			obj.Update();
		}
	}
	
	internal void InternalRender()
	{
		Graphics.BeginDraw();
		camera.Render();
		canvas.Render();
		Graphics.EndDraw();
	}
	
	internal void InternalDestroy()
	{
		Destroy();
		canvas.Destroy();
		camera.Destroy();
		
		foreach (var obj in objects)
			obj.Destroy();
	}
	
	protected virtual void Start() { }
	protected virtual void BuildUI(RootElement root) { }
	protected virtual void Destroy() { }
	protected virtual void Update() { }
	
	internal static void AddObject(XYObject obj) => SceneManager.current.objects.Add(obj);
}