using Silk.NET.Maths;
using XYEngine.Graphics;
using XYEngine.Inputs;

namespace XYEngine.UI;

public class Canvas
{
    public readonly RootElement root = new ();
    public Vector2Int mousePosition;
    
    public float heightRatio { get; private set; }
    private Shader shader;

    // TODO : C'est temporaire, je cherche à mieux l'implémenter.
    private bool expandToWindow;
    private Vector2Int _size;

    public Vector2Int size
    {
        get => _size;
        set
        {
            _size = value;
            if (shader != null)
                CalculeProjection();
        }
    }

    internal void Init()
    {
        shader = Resources.LoadShader("ui");
        CalculeProjection();
        Input.mouseMove += OnMouseMove;
    }

    internal void Render()
    {
        root.Draw(shader);
    }

    internal void Destroy()
    {
        Input.mouseMove -= OnMouseMove;
        root.Destroy();
    }

    internal void CalculeProjection()
    {
        heightRatio = expandToWindow ? Math.Min((float)GameWindow.windowSize.x / size.x, (float)GameWindow.windowSize.y / size.y) : 1;
        root.size = GameWindow.windowSize / heightRatio;
        
        shader.Use();
        shader.SetUniform("projection", Matrix4X4.CreateOrthographicOffCenter(0, root.size.x, 0, root.size.y, 0, 100F));
    }

    private void OnMouseMove(Vector2 position)
    {
        mousePosition = (Input.mousePosition / heightRatio).ToVector2Int();
    }

    public static Vector2Int ScaleToCanvas(Vector2 value) => (value * SceneManager.current.canvas.heightRatio).ToVector2Int();
}