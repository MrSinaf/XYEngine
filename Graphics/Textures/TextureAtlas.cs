namespace XYEngine.Graphics;

public class TextureAtlas : Texture
{
    private readonly Dictionary<string, Rect> frames = [];
    public readonly Data data;

    public TextureAtlas(int width, int height, Color[] pixels, Data data) : base(width, height, pixels)
    {
        this.data = data;
        var ratio = 1 / new Vector2(width, height);
        foreach (var (name, frame) in data.frames)
            frames.Add(name, new Rect(frame.position.ToVector2() * ratio, (frame.position + frame.size).ToVector2() * ratio));
    }

    public bool GetUV(string name, out Rect uv) => frames.TryGetValue(name, out uv);

    public class Data
    {
        public string texturePath { get; set; }
        public Dictionary<string, FrameInt> frames { get; set; }
    }
}