namespace XYEngine;

public static class Time
{
	public static float delta { get; private set; }
	public static float total { get; private set; }
	
	internal static void Update(double deltaTime) => total += delta = (float)deltaTime;
	
	internal static void ResetTime() => total = 0;
}