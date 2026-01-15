namespace XYEngine.Utils;

public static class Utility
{
	public static byte[] ToByteArray(this Stream stream)
	{
		using var memory = new MemoryStream();
		stream.CopyTo(memory);
		
		return memory.ToArray();
	}
	
	public static void JustDoIt(Action action)
	{
		try
		{
			action();
		}
		catch
		{
			// (￣_￣|||)
		}
	}
}