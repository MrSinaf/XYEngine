using System.Collections.Concurrent;

namespace XYEngine.Rendering;

public static class GCommandQueue
{
	private static readonly ConcurrentQueue<Action> commands = new ();
	
	public static void Enqueue(Action command) => commands.Enqueue(command);
	
	public static void ExecuteAll()
	{
		while (commands.TryDequeue(out var command))
			command();
	}
}