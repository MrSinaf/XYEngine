using StbImageSharp;
using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Texture2D : Texture, IAsset
{
	public static readonly Texture2DConfig internalConfig = new (TextureWrap.Repeat, TextureMag.Nearest);
	public static Texture2DConfig defaultConfig = internalConfig;
	
	public Color this[int x, int y] { set => pixels[x + y * size.x] = value; get => pixels[x + y * size.x]; }
	
	public Vector2Int size { get; private set; }
	public uint width { get; private set; }
	public uint height { get; private set; }
	public Vector2 texel { get; private set; }
	
	private Color[] pixels;
	
	public void Apply()
	{
		if (pixels == null)
			throw new NullReferenceException();
		
		GCommandQueue.Enqueue(() =>
		{
			if (gTexture == null)
			{
				gTexture = new GTexture();
				gTexture.SetImage2D(width, height, pixels);
			}
			else
				gTexture.SetSubImage2D(0, 0, width, height, pixels);
		});
	}
	
	public Texture2D() { }
	
	public Texture2D(int width, int height, Color[] pixels)
	{
		size = new Vector2Int(width, height);
		this.width = (uint)width;
		this.height = (uint)height;
		texel = Vector2.one / size;
		
		this.pixels = pixels;
		Apply();
	}
	
	public void Load(AssetProperty property)
	{
		var image = ImageResult.FromStream(property.stream, ColorComponents.RedGreenBlueAlpha);
		size = new Vector2Int(image.Width, image.Height);
		width = (uint)size.x;
		height = (uint)size.y;
		texel = Vector2.one / size;
		pixels = Color.ConvertBytesToColors(image.Data);
		
		Apply();
		
		var config = property.config as Texture2DConfig ?? defaultConfig;
		SetWrap(config.wrap);
		SetFilter((TextureMin)config.filter, config.filter);
	}
	
	public void UnLoad()
	{
		pixels = null;
		GCommandQueue.Enqueue(() => gTexture.Dispose());
	}
}

public record class Texture2DConfig(TextureWrap wrap, TextureMag filter) : IAssetConfig;