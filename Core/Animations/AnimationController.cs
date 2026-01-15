namespace XYEngine.Animations;

public class AnimationController<T> : IAsset
{
	private readonly Dictionary<string, AnimationBlock<T>> blocks = [];
	public AnimationBlock<T> firstBlock;
	
	public void AddBlock(string name, Animation<T> animation, Action<Animator<T>, T> onUpdate = null)
	{
		var newBlock = new AnimationBlock<T>(name, animation, onUpdate ?? delegate { });
		blocks[name] = newBlock;
		firstBlock ??= newBlock;
	}
	
	public AnimationBlock<T> GetBlock(string name) => blocks.TryGetValue(name, out var block) ? block : null;
	
	public void Destroy() => blocks.Clear();
}

public class AnimationBlock<T>(string name, Animation<T> animation, Action<Animator<T>, T> onUpdate)
{
	public readonly string name = name;
	public readonly Animation<T> animation = animation;
	
	public readonly Action<Animator<T>, T> onUpdate = onUpdate;
}