using Silk.NET.Input;

namespace XYEngine.Inputs;

public static class Input
{
    private static IKeyboard keyboard;
    private static IMouse mouse;

    public static Vector2 mousePosition;

    public static event Action<Key> keyDown = _ => { };
    public static event Action<Key> keyUp = _ => { };

    public static float mouseScroll => mouse.ScrollWheels[0].Y;
    public static event Action<MouseButton> mouseUp = _ => { };
    public static event Action<MouseButton> mouseDown = _ => { };
    public static event Action<Vector2> mouseMove = _ => { };

    internal static void Init(IKeyboard keyboard, IMouse mouse)
    {
        Input.keyboard = keyboard;
        Input.mouse = mouse;

        keyboard.KeyDown += (_, key, _) => keyDown((Key)key);
        keyboard.KeyUp += (_, key, _) => keyUp((Key)key);
        mouse.MouseUp += (_, button) => mouseUp((MouseButton)button);
        mouse.MouseDown += (_, button) => mouseDown((MouseButton)button);
        mouse.MouseMove += OnMouseMove;
    }

    private static void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        var currentMousePosition = new Vector2(position.X, GameWindow.windowSize.y - position.Y - 1);
        mouseMove(mousePosition - currentMousePosition);
        mousePosition = currentMousePosition;
    }

    public static bool IsKeyPressed(Key key) => keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
    public static bool IsMouseButtonPressed(MouseButton button) => mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);
}