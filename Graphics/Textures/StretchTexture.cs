namespace XYEngine.Graphics;

public class StretchTexture
{
    public readonly Texture texture;
    public readonly Rect corner00;
    public readonly Rect corner11;
    public readonly Vector2Int cornerSize00;
    public readonly Vector2Int cornerSize11;

    public StretchTexture(Texture texture, RectInt corner00, RectInt corner11, int scale = 1)
    {
        this.texture = texture;
        cornerSize00 = (corner00.position11 - corner00.position00) * scale;
        cornerSize11 = (corner11.position11 - corner11.position00) * scale;
        
        this.corner00 = new Rect(corner00.position00.ToVector2() * texture.texelSize, corner00.position11.ToVector2() * texture.texelSize);
        this.corner11 = new Rect(corner11.position00.ToVector2() * texture.texelSize, corner11.position11.ToVector2() * texture.texelSize);
    }

    public StretchTexture(TextureAtlas texture, string target, RectInt corner00, RectInt corner11, int scale = 1)
    {
        texture.data.frames.TryGetValue(target, out var frame);
        this.texture = texture;
        corner00.position00 += frame.position;
        corner00.position11 += frame.position;
        corner11.position00 += frame.position;
        corner11.position11 += frame.position;
        
        cornerSize00 = (corner00.position11 - corner00.position00) * scale;
        cornerSize11 = (corner11.position11 - corner11.position00) * scale;
        
        this.corner00 = new Rect(corner00.position00.ToVector2() * texture.texelSize, corner00.position11.ToVector2() * texture.texelSize);
        this.corner11 = new Rect(corner11.position00.ToVector2() * texture.texelSize, corner11.position11.ToVector2() * texture.texelSize);
    }
}