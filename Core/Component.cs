namespace XYEngine;

public abstract class Component
{
    public ObjectBehaviour obj { get; private set; }
    
    protected virtual void OnCreate() {}
    protected virtual void OnDestroy() {}

    internal void Init(ObjectBehaviour obj)
    {
        this.obj = obj;
        OnCreate();
    }

    internal void Destroy()
    {
        OnDestroy();
        obj = null;
    }
}