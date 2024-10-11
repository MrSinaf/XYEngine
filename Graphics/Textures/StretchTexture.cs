namespace XYEngine.Graphics;

public class StretchTexture
{
    public readonly Texture texture;
    public readonly Rect corner00;
    public readonly Rect corner11;
    public readonly Vector2Int cornerSize00;
    public readonly Vector2Int cornerSize11;

    public StretchTexture(Texture texture, RectInt corner00, RectInt corner11)
    {
        this.texture = texture;
        cornerSize00 = corner00.position11 - corner00.position00;
        cornerSize11 = corner11.position11 - corner11.position00;
        
        this.corner00 = new Rect(corner00.position00.ToVector2() * texture.texelSize, corner00.position11.ToVector2() * texture.texelSize);
        this.corner11 = new Rect(corner11.position00.ToVector2() * texture.texelSize, corner11.position11.ToVector2() * texture.texelSize);
    }

    [Obsolete("Il est au fait pas encore implémenté !")]
    public StretchTexture(TextureAtlas texture, Rect target, RectInt border)
    {
        this.texture = texture;
        corner00 = new Rect(target.position00, target.position00 + border.position00.ToVector2() * texture.texelSize);
        corner00 = new Rect(target.position11, target.position11 + border.position11.ToVector2() * texture.texelSize);
    }
}