namespace XYEngine.Resources;

public class MaterialUI(Shader shader = null, params (string, object)[] properties) : Material(shader ?? Shader.GetDefaultUI(), properties)
{
	public const string TEXTURE = "mainTex";
	public const string UV_REGION = "uvRegion";
	public const string PADDING = "padding";
	public const string PADDING_SCALE = "paddingScale";
	
	public MaterialUI SetTexture(Texture2D texture, Region? region = null)
	{
		SetProperty(TEXTURE, texture);
		if (region.HasValue)
			SetUVRegion(new Region(region.Value.position00 * texture.texel, region.Value.position11 * texture.texel));
		
		return this;
	}
	
	public MaterialUI SetUVRegion(Region region)
	{
		SetProperty(UV_REGION, region);
		return this;
	}
	
	public MaterialUI SetPadding(Region padding, float? scale)
	{
		SetProperty(PADDING, padding);
		if (scale.HasValue)
			SetProperty(PADDING_SCALE, scale.Value);
		
		return this;
	}
}