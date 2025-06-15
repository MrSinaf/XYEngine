namespace XYEngine.Animations;

public class AnimationController
{
	private readonly Dictionary<string, Animation> animations = new ();
	private readonly Dictionary<string, AnimationLink> links = new ();
	private readonly List<AnimationLink> currents = [];
	
	private Animation animation;
	
	public void Add(string name, Animation animation) => animations.Add(name, animation);
	public void Link(string begin, string end, Func<bool> condition) => links.Add(begin, new AnimationLink(animations[begin], animations[end], condition));
	
	public void Update()
	{
		animation.Update();
		
		foreach (var current in currents)
			if (current.condition())
			{
				Set(current.end);
				break;
			}
	}
	
	public void Set(string animationName) => Set(animations[animationName]);
	
	public void Set(Animation animation)
	{
		this.animation?.Stop();
		this.animation = animation;
		this.animation.Play();
		
		currents.Clear();
		foreach (var link in links.Values)
			if (link.begin == animation)
				currents.Add(link);
	}
	
	private class AnimationLink(Animation begin, Animation end, Func<bool> condition)
	{
		public readonly Animation begin = begin;
		public readonly Animation end = end;
		
		public readonly Func<bool> condition = condition;
	}
}