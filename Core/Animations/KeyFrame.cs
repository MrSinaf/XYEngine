namespace XYEngine.Animations;

public class KeyFrame<T>(T value, float time, bool lerp = false)
{
	public T value = value;
	public float time = time;
	public bool lerp = lerp;
}