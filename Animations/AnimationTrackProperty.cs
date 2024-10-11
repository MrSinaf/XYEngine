namespace XYEngine.Animations;

public class AnimationTrackProperty<T> : AnimationTrack
{
    private readonly Keyframe<T>[] keyFrames;
    private readonly Action<T> applyProperty;

    public AnimationTrackProperty(Keyframe<T>[] keyFrames, Action<T> applyProperty)
    {
        this.keyFrames = keyFrames;
        this.applyProperty = applyProperty;

        foreach (var key in keyFrames)
            if (key.tick > maxTime)
                maxTime = key.tick;
    }
    
    protected internal override int currentIndex { get; set; }

    protected internal override int maxTime { get; }

    protected internal override void Update(int currentTick)
    {
        if (keyFrames[currentIndex].tick < currentTick)
        {
            if (keyFrames.Length > currentIndex + 1 && keyFrames[currentIndex + 1].tick <= currentTick)
                currentIndex++;
        }

        applyProperty(keyFrames[currentIndex].value);
    }
}