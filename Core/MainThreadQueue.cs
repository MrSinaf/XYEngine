using System.Collections.Concurrent;

namespace XYEngine;

public static class MainThreadQueue
{
	private static readonly ConcurrentQueue<Action> commands = new ();
	private static readonly ConcurrentQueue<Action> commandsRenderer = new ();
	
	public static void Enqueue(Action command) => commands.Enqueue(command);
	public static void EnqueueRenderer(Action command) => commandsRenderer.Enqueue(command);
	
	internal static void ExecuteAll()
	{
		while (commands.TryDequeue(out var command))
			command();
	}
	
	internal static void ExecuteAllRenderer()
	{
		while (commandsRenderer.TryDequeue(out var command))
			command();
	}
}