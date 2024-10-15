using Silk.NET.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using StbImageSharp;
using XYEngine.Debugs;
using XYEngine.Inputs;
using XYEngine.UI;
using Key = XYEngine.Inputs.Key;
using Window = Silk.NET.Windowing.Window;

namespace XYEngine;

public enum DisplayMode { Windowed, NoBorder, FullScreen }

public class GameWindow
{
    public const string ENGINE_VERSION = "0.1.0";
    
    public static Vector2Int defaultWindowSize { get; private set; }
    public static Vector2Int windowSize { get; private set; }
    public static ImGuiController controller { get; private set; }
    internal static GL gl => instance._gl;

    public event Action onReady;

    private static GameWindow instance;

    private readonly IWindow window;
    private GL _gl;

    public GameWindow(string name, Scene startScene, Vector2Int defaultWindowSize)
    {
        instance = this;
        window = Window.Create(WindowOptions.Default with
        {
            Title = name,
            Size = new Vector2D<int>(defaultWindowSize.x, defaultWindowSize.y)
        });
        window.Load += OnLoad;
        window.Resize += OnResize;
        window.Closing += OnClosing;
        window.Render += OnRender;
        window.Update += OnUpdate;

        onReady += () => SceneManager.SetCurrent(startScene);

        GameWindow.defaultWindowSize = windowSize = defaultWindowSize;
        window.Run();
    }

    public static void CloseGame()
    {
        instance.window.Close();
    }

    private void OnUpdate(double deltaTime)
    {
        Time.Update(deltaTime);
        SceneManager.Update();
        UIEvent.Update();
    }

    private void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        SceneManager.Render();

        var error = _gl.GetError();
        if (error != GLEnum.NoError)
        {
            Console.WriteLine($"OpenGL Error: {error}");
        }

        controller.Update((float)deltaTime);
#if DEBUG
        ImGuiRender();
#endif
        controller.Render();
    }

    private void ImGuiRender()
    {
        // TODO : Ajouter un moyen de pouvoir le configurer efficacement en externe.
    }

    private void KeyDown(Key key)
    {
        if (key == Key.Escape)
            window.Close();
    }

    private void OnLoad()
    {
        Debug.LogIntern($"XYEngine - v{ENGINE_VERSION}", TypeLog.Info);
        
        var input = window.CreateInput();
        controller = new ImGuiController(_gl = window.CreateOpenGL(), window, input);
        Input.Init(input.Keyboards[0], input.Mice[0]);
        Input.keyDown += KeyDown;


        _gl.Enable(GLEnum.CullFace);
        _gl.CullFace(TriangleFace.Front);
        _gl.ClearColor(0.1F, 0.1F, 0.15F, 1);

        SetDisplayMode(DisplayMode.Windowed);

        // Récupère l'icône de l'application :
        var result = ImageResult.FromMemory(File.ReadAllBytes("Icon.png"), ColorComponents.RedGreenBlueAlpha);
        var image = new RawImage(result.Width, result.Height, result.Data);
        window.SetWindowIcon(ref image);

        StbImage.stbi_set_flip_vertically_on_load(1);

        SceneManager.SetCurrent(new SplashScreen(onReady));
    }

    public static void SetDisplayMode(DisplayMode mode)
    {
        var window = instance.window;
        var screenSize = window.Monitor!.VideoMode!.Resolution!.Value!;

        switch (mode)
        {
            case DisplayMode.Windowed:
                window.WindowBorder = WindowBorder.Resizable;
                window.WindowState = WindowState.Normal;
                window.Position = new Vector2D<int>(screenSize.X / 2 - window.Size.X / 2, screenSize.Y / 2 - window.Size.Y / 2);
                break;
            case DisplayMode.NoBorder:
                window.WindowBorder = WindowBorder.Hidden;
                window.WindowState = WindowState.Maximized;
                window.Position = new Vector2D<int>();
                window.Size = screenSize;
                break;
            case DisplayMode.FullScreen:
                window.WindowState = WindowState.Fullscreen;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        windowSize = new Vector2Int(size.X, size.Y);
        _gl.Viewport(size);

        SceneManager.WindowSizeChanged();
    }

    private void OnClosing()
    {
        _gl.Dispose();
    }
}