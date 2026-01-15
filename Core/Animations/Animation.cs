namespace XYEngine.Animations;

public class Animation<T>
{
	private readonly List<IAnimationTrack<T>> animationTracks = [];
	
	public float duration { get; private set; }
	public bool loop;
	
	public Animation<T> AddTrack(IAnimationTrack<T> track)
	{
		if (duration < track.maxTime)
			duration = track.maxTime;
		
		animationTracks.Add(track);
		return this;
	}
	
	public void RemoveTrack(IAnimationTrack<T> track)
	{
		animationTracks.Remove(track);
		
		duration = 0;
		foreach (var animationTrack in animationTracks)
			if (duration < animationTrack.maxTime)
				duration = animationTrack.maxTime;
	}
	
	public void Sample(T obj, float time)
	{
		foreach (var animationTrack in animationTracks)
			animationTrack.Update(obj, time);
	}
	
	public void ResetTracks()
	{
		foreach (var animationTrack in animationTracks)
			animationTrack.Reset();
	}
}