using Silk.NET.Maths;
using XYEngine.Inputs;

namespace XYEngine.Graphics;

public class Camera
{
    public static int pixelsPerUnit = 16;
    public static Vector2 mousePosition;

    public Vector2 position;

    internal List<ObjectBehaviour> objects;
    private Matrix4X4<float> projection;
    private float cameraRatio;
    private Shader shader;
    
    private float _zoom;
    
    public float zoom
    {
        get => _zoom;
        set
        {
            _zoom = float.Clamp(value, 1, 16);
            cameraRatio = 1 / (pixelsPerUnit * zoom);
            CalculeProjection();
        }
    }

    internal void Init()
    {
        shader = Resources.LoadShader("default");
        zoom = 4;
    }

    internal void Render()
    {
        shader.Use();
        shader.SetUniform("projection", Matrix4X4.CreateTranslation(-position.x, -position.y, 0f) * projection);
        shader.SetUniform("texture0", 0);
        shader.SetUniform("texture1", 1);
        shader.SetUniform("texture2", 2);

        foreach (var obj in objects)
            obj.Draw(shader);
    }

    internal void Update()
    {
        var center = GameWindow.windowSize.ToVector2() * 0.5F;
        mousePosition = (Input.mousePosition - center) * cameraRatio + position ;
    }

    internal void CalculeProjection()
    {
        projection = Matrix4X4.CreateOrthographic(GameWindow.windowSize.x * cameraRatio, GameWindow.windowSize.y * cameraRatio, 0, 100F);
    }
}