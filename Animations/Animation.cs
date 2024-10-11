namespace XYEngine.Animations;

public class Animation
{
    private const float TIME_PER_TICK = 0.1F;

    private List<AnimationTrack> animationTracks = [];
    private int duration = 3;
    private int currentTick = -1;
    private float time;

    public bool isRunning { get; private set; }
    public bool loop;

    public event Action onFinish;

    /// <summary>
    /// Joue l'animation.
    /// <remarks>L'animation est joué à partir du début.</remarks>
    /// </summary>
    public void Play()
    {
        isRunning = true;
        currentTick = 0;
    }

    /// <summary>
    /// Stoppe l'animation.
    /// </summary>
    public void Stop()
    {
        isRunning = false;
    }

    /// <summary>
    /// Ajoute une nouvelle piste à l'animation.
    /// </summary>
    /// <param name="track">Piste à ajouter.</param>
    public void AddTrack(AnimationTrack track)
    {
        if (duration < track.maxTime)
            duration = track.maxTime;

        animationTracks.Add(track);
    }

    /// <summary>
    /// Supprime une piste ciblée de l'animation.
    /// </summary>
    /// <param name="track">Pise à supprimer.</param>
    public void RemoveTrack(AnimationTrack track)
    {
        animationTracks.Remove(track);
    }

    // TODO : Il serait peut-être plus judicieux de gérer Update soit même plutôt que de laisser l'appelant le faire.
    public void Update()
    {
        if (!isRunning)
            return;

        time += Time.delta;
        if (time < TIME_PER_TICK)
            return;

        time = 0;
        currentTick++;
        if (currentTick > duration)
        {
            if (!loop)
            {
                isRunning = false;
                onFinish?.Invoke();
            }
            
            currentTick = 0;
            foreach (var track in animationTracks)
                track.currentIndex = 0;
        }

        foreach (var track in animationTracks)
            track.Update(currentTick);
    }
}