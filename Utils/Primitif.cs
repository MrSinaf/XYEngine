using XYEngine.Rendering;
using XYEngine.Resources;

namespace XYEngine.Utils;

public static class Primitif
{
	public static Texture2D xyTexture { get; private set; }
	public static Texture2D whitePixel { get; private set; }
	public static Texture2D uiTexture { get; private set; }
	
	public static Mesh quad { get; private set; }
	
	public static void Init()
	{
		whitePixel = new Texture2D(1, 1, [Color.white]);
		whitePixel.Apply();
		
		uiTexture = AssetManager.LoadEmbeddedAsset<Texture2D>("textures.ui.png", new Texture2DConfig(TextureWrap.ClampToBorder, TextureMag.Nearest));
		xyTexture = AssetManager.LoadEmbeddedAsset<Texture2D>("textures.xy.png", new Texture2DConfig(TextureWrap.ClampToBorder, TextureMag.Nearest));
		
		quad = MeshFactory.CreateQuad(Vector2.one).Apply();
	}
}