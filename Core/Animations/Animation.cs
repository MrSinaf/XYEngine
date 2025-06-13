namespace XYEngine.Animations;

public class Animation
{
	private readonly List<IAnimationTrack> animationTracks = [];
	
	public bool isRunning { get; private set; }
	public float duration { get; private set; }
	public float currentTime { get; private set; }
	
	public bool loop;
	
	public event Action onFinish = delegate { };
	
	public void AddTrack(IAnimationTrack track)
	{
		if (duration < track.maxTime)
			duration = track.maxTime;
		
		animationTracks.Add(track);
	}
	
	public void RemoveTrack(IAnimationTrack track)
	{
		animationTracks.Remove(track);
		
		duration = 0;
		foreach (var animationTrack in animationTracks)
			if (duration < animationTrack.maxTime)
				duration = animationTrack.maxTime;
	}
	
	public void Play()
	{
		currentTime = 0;
		isRunning = true;
	}
	
	public void Resume()
	{
		isRunning = false;
	}
	
	public void Pause()
	{
		isRunning = false;
	}
	
	public void Stop()
	{
		currentTime = 0;
		isRunning = false;
	}
	
	public void Update()
	{
		if (!isRunning)
			return;
		
		currentTime += Time.delta;
		
		foreach (var animationTrack in animationTracks)
			animationTrack.Update(currentTime);
		
		if (currentTime > duration)
		{
			if (loop)
			{
				currentTime = 0;
				foreach (var animationTrack in animationTracks)
					animationTrack.Reset();
			}
			else
				isRunning = false;
			
			onFinish();
		}
	}
}