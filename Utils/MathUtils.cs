namespace XYEngine.Utils;

public static class MathUtils
{
	public static float Range(float value, float min, float max) => value < min ? min : value > max ? max : value;
}