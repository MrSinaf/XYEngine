namespace XYEngine.Animations;

public class AnimationTrack<T> : IAnimationTrack
{
	public readonly KeyFrame<T>[] keyFrames;
	public readonly Action<T> applyProperty;
	
	public float maxTime { get; }
	
	private int currentIndex;
	
	public AnimationTrack(KeyFrame<T>[] keyFrames, Action<T> applyProperty)
	{
		this.keyFrames = keyFrames;
		this.applyProperty = applyProperty;
		
		foreach (var key in keyFrames)
			if (key.time > maxTime)
				maxTime = key.time;
	}
	
	void IAnimationTrack.Update(float targetTime)
	{
		if (keyFrames.Length == 0)
			return;
		
		if (targetTime <= keyFrames[0].time)
		{
			applyProperty(keyFrames[0].value);
			currentIndex = 0;
			return;
		}
		
		if (targetTime >= maxTime)
		{
			applyProperty(keyFrames[^1].value);
			currentIndex = keyFrames.Length - 1;
			return;
		}
		
		if (targetTime < keyFrames[currentIndex].time)
			currentIndex = 0;
		
		while (currentIndex < keyFrames.Length - 1 && keyFrames[currentIndex + 1].time <= targetTime)
			currentIndex++;
		
		if (currentIndex == keyFrames.Length - 1)
		{
			applyProperty(keyFrames[currentIndex].value);
			return;
		}
		
		var currentFrame = keyFrames[currentIndex];
		var nextFrame = keyFrames[currentIndex + 1];
		applyProperty(currentFrame.lerp ? Lerp(currentFrame.value, nextFrame.value, (targetTime - currentFrame.time) / (nextFrame.time - currentFrame.time)) : currentFrame.value);
	}
	
	void IAnimationTrack.Reset()
	{
		currentIndex = 0;
	}
	
	private static T Lerp(T start, T end, float t)
	{
		if (start is ILerpable<T> lerpable)
			return lerpable.Lerp(end, t);
		if (typeof(T) == typeof(float))
			return (T)(object)((float)(object)start * (1 - t) + (float)(object)end * t);
		if (typeof(T) == typeof(double))
			return (T)(object)((double)(object)start * (1 - t) + (double)(object)end * t);
		if (typeof(T) == typeof(int))
			return (T)(object)(int)Math.Round((int)(object)start * (1 - t) + (int)(object)end * t);
		
		return start;
	}
}

public interface IAnimationTrack
{
	public float maxTime { get; }
	
	public void Update(float time);
	internal void Reset();
}