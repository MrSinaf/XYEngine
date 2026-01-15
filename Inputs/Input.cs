using Silk.NET.GLFW;
using Silk.NET.Input;

namespace XYEngine.Inputs;

public static class Input
{
	public static Vector2 mousePosition { get; private set; }
	internal static IInputContext context { get; private set; }
	
	public static event Action<Key> keyDown = delegate { };
	public static event Action<Key> keyUp = delegate { };
	public static event Action<MouseButton> clickDown = delegate { };
	public static event Action<MouseButton> clickUp = delegate { };
	public static event Action<Vector2> mouseMove = delegate { };
	public static event Action<Vector2> mouseScroll = delegate { };
	public static Action<char> charDown = delegate { };
	
	private static readonly HashSet<Key> currentKeys = [];
	private static readonly HashSet<Key> previousKeys = [];
	private static readonly HashSet<MouseButton> currentMouseButtons = [];
	private static readonly HashSet<MouseButton> previousMouseButtons = [];
	
	internal static IKeyboard keyboard;
	internal static IMouse mouse;
	
	internal static void Initialize(IInputContext context)
	{
		Input.context = context;
		keyboard = context.Keyboards[0];
		mouse = context.Mice[0];
		
		var window = GameWindow.GetWindow().Native?.Glfw;
		if (window.HasValue)
		{
			unsafe
			{
				Glfw.GetApi().SetCharCallback((WindowHandle*)window.Value, OnCharacterReceived);
			}
		}
		
		keyboard.KeyDown += (_, key, _) =>
		{
			currentKeys.Add((Key)key);
			keyDown((Key)key);
		};
		
		keyboard.KeyUp += (_, key, _) => keyUp((Key)key);
		
		mouse.MouseDown += (_, button) =>
		{
			currentMouseButtons.Add((MouseButton)button);
			clickDown((MouseButton)button);
		};
		
		mouse.MouseUp += (_, button) => clickUp((MouseButton)button);
		
		mouse.MouseMove += (_, position) =>
		{
			var newMousePosition = new Vector2(position.X, GameWindow.size.y - position.Y - 1);
			mouseMove(newMousePosition - mousePosition);
			mousePosition = newMousePosition;
		};
		
		mouse.Scroll += (_, scroll) => mouseScroll(new Vector2(scroll.X, scroll.Y));
	}
	
	internal static void Update() => EndInput();
	
	private static void EndInput()
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
	
	private static unsafe void OnCharacterReceived(WindowHandle* window, uint codepoint) => charDown((char)codepoint);
	
	public static bool IsKeyHeldDown(Key key) => keyboard.IsKeyPressed((Silk.NET.Input.Key)key);
	public static bool IsKeyPressed(Key key) => !previousKeys.Contains(key) && currentKeys.Contains(key);
	public static bool IsKeyReleased(Key key) => previousKeys.Contains(key) && !currentKeys.Contains(key);
	
	public static bool IsMouseButtonHeldDown(MouseButton button) 
		=> mouse.IsButtonPressed((Silk.NET.Input.MouseButton)button);
	public static bool IsMouseButtonPressed(MouseButton button)
		=> !previousMouseButtons.Contains(button) && currentMouseButtons.Contains(button);
	public static bool IsMouseButtonReleased(MouseButton button)
		=> previousMouseButtons.Contains(button) && !currentMouseButtons.Contains(button);
}