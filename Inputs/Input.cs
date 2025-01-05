using Silk.NET.Input;

namespace XYEngine.Inputs;

public static class Input
{
	public static Vector2 mousePosition { get; private set; }
	public static float mouseScroll => mouse.ScrollWheels[0].Y;
	
	public static event Action<Key> keyDown = _ => { };
	public static event Action<Key> keyUp = _ => { };
	public static event Action<MouseButton> mouseDown = _ => { };
	public static event Action<MouseButton> mouseUp = _ => { };
	public static event Action<Vector2> mouseMove = _ => { };
	
	private static IKeyboard keyboard;
	private static IMouse mouse;
	
	private static readonly HashSet<Key> currentKeys = [];
	private static readonly HashSet<Key> previousKeys = [];
	private static readonly HashSet<MouseButton> currentMouseButtons = [];
	private static readonly HashSet<MouseButton> previousMouseButtons = [];
	
	internal static void Initialize(IInputContext context)
	{
		keyboard = context.Keyboards[0];
		mouse = context.Mice[0];
		
		keyboard.KeyDown += (_, key, _) =>
		{
			currentKeys.Add((Key)key);
			keyDown((Key)key);
		};
		
		keyboard.KeyUp += (_, key, _) => keyUp((Key)key);
		
		mouse.MouseDown += (_, button) =>
		{
			currentMouseButtons.Add((MouseButton)button);
			mouseDown((MouseButton)button);
		};
		
		mouse.MouseUp += (_, button) => mouseUp((MouseButton)button);
		
		mouse.MouseMove += (_, position) =>
		{
			var newMousePosition = new Vector2(position.X, GameWindow.size.y - position.Y - 1);
			mouseMove(newMousePosition - mousePosition);
			mousePosition = newMousePosition;
		};
	}
	
	internal static void EndInput()
	{
		previousKeys.Clear();
		foreach (var key in currentKeys)
			previousKeys.Add(key);
		
		currentKeys.Clear();
		
		previousMouseButtons.Clear();
		foreach (var button in currentMouseButtons)
			previousMouseButtons.Add(button);
		
		currentMouseButtons.Clear();
	}
	
	public static bool IsKeyPressed(Key key) => keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
	public static bool IsKeyDown(Key key) => !previousKeys.Contains(key) && currentKeys.Contains(key);
	public static bool IsKeyUp(Key key) => previousKeys.Contains(key) && !currentKeys.Contains(key);
	
	public static bool IsMouseButtonPressed(MouseButton button) => previousMouseButtons.Contains(button) && currentMouseButtons.Contains(button);
	public static bool IsMouseButtonDown(MouseButton button) => !previousMouseButtons.Contains(button) && currentMouseButtons.Contains(button);
	public static bool IsMouseButtonUp(MouseButton button) => previousMouseButtons.Contains(button) && !currentMouseButtons.Contains(button);
}