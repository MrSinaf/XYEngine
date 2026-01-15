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
		
		uiTexture = Vault.LoadEmbeddedResource<Texture2D>("ui.png", "textures.ui.png",
														  new Texture2DConfig(
															  TextureWrap.ClampToBorder, TextureMag.Nearest));
		xyTexture = Vault.LoadEmbeddedResource<Texture2D>("xy.png", "textures.xy.png",
														  new Texture2DConfig(
															  TextureWrap.ClampToBorder, TextureMag.Nearest));
		
		quad = MeshFactory.CreateQuad(Vector2.one).Apply();
	}
}