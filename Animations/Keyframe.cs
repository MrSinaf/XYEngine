namespace XYEngine.Animations;

public class Keyframe<T>(int tick, T value)
{
    public int tick = tick;
    public T value = value;
}