namespace XYEngine;

public static class Time
{
    public static float current { get; private set; }
    public static float delta { get; private set; }

    internal static void Update(double deltaTime)
    {
        current += delta = (float)deltaTime;
    }
}