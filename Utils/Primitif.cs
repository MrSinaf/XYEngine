using XYEngine.Resources;

namespace XYEngine.Utils;

public static class Primitif
{
	public static Texture2D whitePixel { get; private set; }
	
	public static void Init()
	{
		whitePixel = new Texture2D(1, 1, [Color.white]);
		whitePixel.Apply();
	}
}