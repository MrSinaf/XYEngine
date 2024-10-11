using XYEngine.Essentials;
using XYEngine.Graphics;
using XYEngine.UI;

namespace XYEngine;

public class Scene(Canvas canvas = null, Camera camera = null)
{
    public Camera camera { get; private set; } = camera ?? new Camera();
    public Canvas canvas { get; private set; } = canvas ?? new Canvas { size = GameWindow.defaultWindowSize };

    internal readonly ListEvolute<ObjectBehaviour> objects = new (obj => obj.Start(), obj => obj.Destroyed());

    internal void Init()
    {
        camera.objects = objects.values;
        camera.Init();
        canvas.Init();
        Start();
    }

    internal void InternalUpdate()
    {
        objects.Update();
        foreach (var obj in objects)
            obj.Update();
        Update();
        
        camera.Update();
    }

    internal void InternalRender()
    {
        camera.Render();
        canvas.Render();
        Render();
    }

    internal void InternalDestroy()
    {
        canvas.Destroy();
        objects.Clear();
        Destroy();
    }

    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void Render() { }
    
    protected virtual void Destroy() { }

    internal void AddObject(ObjectBehaviour obj)
    {
        obj.scene = this;
        objects.Add(obj);
    }

    internal void RemoveObject(ObjectBehaviour obj)
    {
        objects.Remove(obj);
    }
}