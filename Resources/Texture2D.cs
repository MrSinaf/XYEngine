using StbImageSharp;
using XYEngine.Debugs;
using XYEngine.Rendering;

namespace XYEngine.Resources;

public class Texture2D : Texture, IResource, IDebugProperty
{
	public static readonly Texture2DConfig internalConfig = new (TextureWrap.Repeat, TextureMag.Nearest);
	public static Texture2DConfig defaultConfig = internalConfig;
	
	public Color this[int x, int y] { set => pixels[x + y * size.x] = value; get => pixels[x + y * size.x]; }
	
	public string assetPath { get; set; }
	public Vector2Int size { get; private set; }
	public uint width { get; private set; }
	public uint height { get; private set; }
	public Vector2 texel { get; private set; }
	
	public Color[] pixels;
	
	public void Apply()
	{
		if (pixels == null)
			throw new NullReferenceException();
		
		MainThreadQueue.EnqueueRenderer(() =>
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
	
	public Texture2D(int width, int height, Color[] pixels, Texture2DConfig config = null)
	{
		size = new Vector2Int(width, height);
		this.width = (uint)width;
		this.height = (uint)height;
		texel = Vector2.one / size;
		
		this.pixels = pixels;
		
		Apply();
		
		config ??= defaultConfig;
		SetWrap(config.wrap);
		SetFilter((TextureMin)config.filter, config.filter);
	}
	
	public Region GetUVRegion(RectInt target) 
		=> new (target.position * texel, (target.position + target.size) * texel);
	public Rect GetUVRect(RectInt target) => new (target.position * texel, target.size * texel);
	
	void IResource.Load(Resource ressource)
	{
		var image = ImageResult.FromStream(ressource.stream, ColorComponents.RedGreenBlueAlpha);
		size = new Vector2Int(image.Width, image.Height);
		width = (uint)size.x;
		height = (uint)size.y;
		texel = Vector2.one / size;
		pixels = Color.FromBytes(image.Data);
		
		Apply();
		
		var config = ressource.config as Texture2DConfig ?? defaultConfig;
		SetWrap(config.wrap);
		SetFilter((TextureMin)config.filter, config.filter);
	}
	
	void IAsset.Destroy()
	{
		pixels = null;
		MainThreadQueue.EnqueueRenderer(() => gTexture.Dispose());
	}
	
	void IDebugProperty.OnDebugPropertyRender()
	{
		XYDebug.ShowValue("Dimensions", $"{size.x}x{size.y}");
		
		float sizeInBytes = size.x * size.y * 4;
		string sizeText;
		if (sizeInBytes < 1048576F)
		{
			var sizeInKo = sizeInBytes / 1024f;
			sizeText = $"{sizeInKo:F2} Ko";
		}
		else
		{
			var sizeInMo = sizeInBytes / 1048576F;
			sizeText = $"{sizeInMo:F2} Mo";
		}
		XYDebug.ShowValue("Size", sizeText);
		
		ImGui.Spacing();
		
		var ratio = (float)height / width;
		var availableSpace = ImGui.GetContentRegionAvail().x;
		var drawList = ImGui.GetWindowDrawList();
		drawList.AddRectFilled(ImGui.GetCursorScreenPos(),
							   ImGui.GetCursorScreenPos() + new Vector2(availableSpace, availableSpace * ratio),
							   ImGui.GetColorU32(ImGuiCol.FrameBg));
		ImGui.Image((IntPtr)gTexture.handle, new Vector2(availableSpace, (int)(availableSpace * ratio)));
	}
}

public record class Texture2DConfig(TextureWrap wrap, TextureMag filter) : IResourceConfig;