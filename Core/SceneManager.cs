namespace XYEngine;

public static class SceneManager
{
    public static Scene current { get; private set; }

    /// <summary>
    /// Change la scène courante, par une nouvelle.
    /// </summary>
    /// <param name="scene">Scene à appliquer.</param>
    public static void SetCurrent(Scene scene)
    {
        current?.InternalDestroy();
        current = scene;
        current.Init();
    }

    internal static void Update()
    {
        current.InternalUpdate();
    }

    internal static void Render()
    {
        current.InternalRender();
    }

    internal static void WindowSizeChanged()
    {
        current.canvas.CalculeProjection();
        current.camera.CalculeProjection();
    }
}