namespace XYEngine.Animations;

public class Animator<T> : Component
{
	public AnimationController<T> controller { get; private set; }
	public AnimationBlock<T> animationBlock { get; private set; }
	
	public float currentTime { get; private set; }
	public bool isRunning { get; private set; }
	public T obj { get; private set; }
	
	public void Play()
	{
		currentTime = 0;
		isRunning = true;
	}
	
	public void Resume() => isRunning = true;
	
	public void Pause() => isRunning = false;
	
	public void Stop()
	{
		currentTime = 0;
		isRunning = false;
	}
	
	public Animator<T> SetController(T obj, AnimationController<T> controller, bool autoPlay = true)
	{
		this.obj = obj;
		this.controller = controller;
		animationBlock = controller.firstBlock;
		
		if (autoPlay)
			Play();
		
		return this;
	}
	
	public void SetAnimation(string name)
	{
		Stop();
		animationBlock = controller.GetBlock(name);
		Play();
	}
	
	public override void Update()
	{
		if (animationBlock == null || !isRunning)
			return;
		
		currentTime += Time.delta;
		animationBlock.animation.Sample(obj, currentTime);
		
		if (currentTime > animationBlock.animation.duration)
		{
			if (animationBlock.animation.loop)
			{
				currentTime = 0;
				animationBlock.animation.ResetTracks();
			}
			else
				isRunning = false;
		}
		
		animationBlock.onUpdate(this, obj);
	}
}