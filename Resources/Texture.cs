using XYEngine.Rendering;

namespace XYEngine.Resources;

public abstract class Texture
{
	public GTexture gTexture;
	
	public void SetWrap(TextureWrap wrap) => GCommandQueue.Enqueue(() =>
	{
		gTexture.SetWrapS(wrap);
		gTexture.SetWrapT(wrap);
	});
	
	public void SetFilter(TextureMin minFilter, TextureMag magFilter) => GCommandQueue.Enqueue(() =>
	{
		gTexture.SetMinFilter(minFilter);
		gTexture.SetMagFilter(magFilter);
	});
}

public record class TextureMeta(TextureWrap wrap, TextureMag filter);