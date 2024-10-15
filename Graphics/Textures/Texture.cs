using Silk.NET.OpenGL;
using static XYEngine.GameWindow;

namespace XYEngine.Graphics;

public enum TextureFilter { Nearest = 9728, Linear = 9729 }

public class Texture
{
    public readonly Vector2Int size;
    public readonly Vector2 texelSize;
    public TextureFilter filter = TextureFilter.Nearest;

    private readonly uint handle;
    private readonly Color[] pixels;

    public Color this[int x, int y]
    {
        set => pixels[x + y * size.x] = value;
        get => pixels[x + y * size.x];
    }

    public Texture(int width, int height) : this(width, height, new Color[width * height]) { }

    public Texture(int width, int height, Color[] pixels)
    {
        handle = gl.GenTexture();
        size = new Vector2Int(width, height);
        this.pixels = pixels;
        texelSize = 1 / size.ToVector2();

        gl.BindTexture(TextureTarget.Texture2D, handle);
        
        var wrapMode = (int)TextureWrapMode.ClampToEdge;
        
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, in wrapMode);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, in wrapMode);
        SetFilter(filter);

        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        ApplyPixels();
    }

    public void ApplyPixels()
    {
        gl.BindTexture(TextureTarget.Texture2D, handle);
        unsafe
        {
            fixed (Color* ptr = pixels)
            {
                gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, (uint)size.x, (uint)size.y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }
        gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void SetFilter(TextureFilter filter)
    {
        this.filter = filter;
        var filterIndex = (int)filter;
        gl.BindTexture(TextureTarget.Texture2D, handle);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, in filterIndex);
        gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, in filterIndex);
    }

    public void Use(TextureUnit unit = TextureUnit.Texture0)
    {
        gl.ActiveTexture(unit);
        gl.BindTexture(TextureTarget.Texture2D, handle);
    }
}