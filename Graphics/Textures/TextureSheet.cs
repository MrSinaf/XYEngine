namespace XYEngine.Graphics;

public class TextureSheet : Texture
{
    private readonly Rect[,] frames;
    public readonly Data data;

    public TextureSheet(int width, int height, Color[] pixels, Data data) : base(width, height, pixels)
    {
        this.data = data;
        var textureSize = new Vector2(width, height);
        var ratio = data.frameSize / textureSize;
        var nFrames = textureSize.ToVector2Int() / data.frameSize;

        frames = new Rect[nFrames.x, nFrames.y];
        for (var y = 0; y < nFrames.y; y++)
        for (var x = 0; x < nFrames.x; x++)
        {
            var position = new Vector2(x, y);
            frames[x, y] = new Rect(position * ratio, position * ratio + ratio);
        }
    }

    public Rect GetUV(int x, int y = 0) => frames[x, y];

    public class Data
    {
        public string texturePath { get; set; }
        public Vector2Int frameSize { get; set; }
    }
}