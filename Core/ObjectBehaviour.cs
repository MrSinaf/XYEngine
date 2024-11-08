using Silk.NET.Maths;
using XYEngine.Graphics;

namespace XYEngine;

public class ObjectBehaviour
{
    public readonly Render render = new ();
    
    public string name;

    internal Scene scene;
    private bool needingUpdate;
    
    private Vector2 _position;
    private Vector2 _scale = Vector2.one;
    private Matrix4X4<float> _matrix = Matrix4X4<float>.Identity;

    #region GetSet Public
    public Vector2 position
    {
        get => _position;
        set
        {
            needingUpdate = true;
            _position = value;
        }
    }

    public Vector2 scale
    {
        get => _scale;
        set
        {
            needingUpdate = true;
            _scale = value;
        }
    }

    public Matrix4X4<float> matrix
    {
        get
        {
            if (needingUpdate)
                _matrix = Matrix4X4.CreateScale(scale.x, scale.y, 1F) * Matrix4X4.CreateTranslation(position.x, position.y, 0);
            
            return _matrix;
        }
    }
    #endregion
    
    public ObjectBehaviour(string name = "Object")
    {
        this.name = name;
        SceneManager.current?.AddObject(this);
    }

    protected internal virtual void Start() { }
    protected internal virtual void Update() { }
    protected internal virtual void Destroyed() { }

    public void Destroy()
    {
        scene.RemoveObject(this);
        render.Dispose();
        scene = null;
    }

    internal void Draw(Shader shader)
    {
        shader.Use();
        shader.SetUniform("model", matrix);
        shader.SetUniform("colorTarget", new Color(25, 25, 68));
        render.Draw(shader);
    }
}