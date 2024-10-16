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

    private static readonly List<MouseButton> mouseButtonsState = [];
    private static readonly List<Key> keysState = [];

    internal static void Init(IKeyboard keyboard, IMouse mouse)
    {
        Input.keyboard = keyboard;
        Input.mouse = mouse;

        keyboard.KeyDown += (_, key, _) =>
        {
            var value = (Key)key;
            keysState.Add(value);
            keyDown(value);
        };
        keyboard.KeyUp += (_, key, _) =>
        {
            var value = (Key)key;
            keysState.Remove(value);
            keyUp(value);
        };
        mouse.MouseUp += (_, button) =>
        {
            var value = (MouseButton)button;
            mouseButtonsState.Remove(value);
            mouseUp(value);
        };
        mouse.MouseDown += (_, button) =>
        {
            var value = (MouseButton)button;
            mouseButtonsState.Add(value);
            mouseDown(value);
        };
        mouse.MouseMove += OnMouseMove;
    }

    internal static void Update()
    {
        mouseButtonsState.Clear();
        keysState.Clear();
    }

    private static void OnMouseMove(IMouse mouse, System.Numerics.Vector2 position)
    {
        var currentMousePosition = new Vector2(position.X, GameWindow.windowSize.y - position.Y - 1);
        mouseMove(mousePosition - currentMousePosition);
        mousePosition = currentMousePosition;
    }

    public static bool IsKeyPressed(Key key) => keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
    public static bool IsKeyTap(Key key) => keysState.Contains(key);
    
    public static bool IsMouseButtonPressed(MouseButton button) => mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);
    public static bool IsMouseButtonClick(MouseButton button) => mouseButtonsState.Contains(button);
}