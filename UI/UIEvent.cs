using XYEngine.Inputs;

namespace XYEngine.UI;

public static class UIEvent
{
	public enum Type
	{
		MouseEnter, MouseExit, MouseOver,
		MouseClick
	}
	
	private static readonly Dictionary<UIElement, Dictionary<Type, Action>> elements = new ();
	private static readonly List<(UIElement, Type, Action)> pendingRegistrations = [];
	private static readonly List<(UIElement, Type)> pendingUnRegistrations = [];
	private static readonly Dictionary<UIElement, HashSet<Type>> previousStatesElement = new ();
	
	public static void Register(UIElement element, Type eventType, Action action)
		=> pendingRegistrations.Add((element, eventType, action));
	
	public static void UnRegister(UIElement element, Type eventType)
		=> pendingUnRegistrations.Add((element, eventType));
	
	public static void UnRegisterAllEvents(UIElement element)
	{
		elements.Remove(element);
		previousStatesElement.Remove(element);
	}
	
	internal static void Update()
	{
		ApplyPendingLists();
		
		
		foreach (var (element, events) in elements)
		{
			if (!element.isActif)
				continue;
			
			var previousStates = previousStatesElement[element];
			var isMouseOver = element.ContainsPoint(element.canvas.mousePosition);
			var newStates = new HashSet<Type>();
			
			if (isMouseOver)
				newStates.Add(Type.MouseOver);
			
			foreach (var (eventType, action) in events)
			{
				switch (eventType)
				{
					case Type.MouseEnter:
						if (isMouseOver && !previousStates.Contains(Type.MouseOver))
						{
							action();
							newStates.Add(Type.MouseEnter);
						}
						
						break;
					case Type.MouseExit:
						if (!isMouseOver && previousStates.Contains(Type.MouseOver))
						{
							action();
							newStates.Add(Type.MouseExit);
						}
						
						break;
					case Type.MouseOver:
						if (isMouseOver)
							action();
						
						break;
					case Type.MouseClick:
						if (isMouseOver && Input.IsMouseButtonPressed(MouseButton.Left))
						{
							action();
							newStates.Add(Type.MouseClick);
						}
						
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			
			previousStatesElement[element] = newStates;
		}
	}
	
	private static void ApplyPendingLists()
	{
		foreach (var (element, eventType, action) in pendingRegistrations)
		{
			if (elements.TryGetValue(element, out var events))
			{
				if (!events.TryAdd(eventType, action))
					events[eventType] += action;
			}
			else
			{
				elements.Add(element, new Dictionary<Type, Action> { { eventType, action } });
				previousStatesElement.Add(element, []);
			}
		}
		
		pendingRegistrations.Clear();
		
		foreach (var (element, eventType) in pendingUnRegistrations)
		{
			if (elements.TryGetValue(element, out var events))
			{
				events.Remove(eventType);
				if (events.Count == 0)
				{
					elements.Remove(element);
					previousStatesElement.Remove(element);
				}
			}
		}
		
		pendingUnRegistrations.Clear();
	}
}