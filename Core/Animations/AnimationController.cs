namespace XYEngine.Animations;

public class AnimationController
{
	private readonly List<AnimationLink> links = [];
	private readonly List<AnimationLink> currents = [];
	
	private Animation animation;
	
	public void Add(AnimationLink link)
	{
		links.Add(link);
	}
	
	public void Update()
	{
		animation.Update();
		
		for (var index = 0; index < currents.Count; index++)
		{
			var current = currents[index];
			if (current.condition())
			{
				Set(current.end);
				break;
			}
		}
	}
	
	public void Set(Animation animation)
	{
		animation?.Stop();
		this.animation = animation;
		animation.Play();
		
		currents.Clear();
		foreach (var link in links)
			if (link.begin == animation)
				currents.Add(link);
	}
}

public class AnimationLink(Animation begin, Animation end, Func<bool> condition)
{
	public readonly Animation begin = begin;
	public readonly Animation end = end;
	
	public readonly Func<bool> condition = condition;
}