namespace XYEngine.Animations;

public abstract class AnimationTrack
{
    protected internal abstract int currentIndex { get; set; }
    protected internal abstract int maxTime { get; }
    protected internal abstract void Update(int currentTick);
}