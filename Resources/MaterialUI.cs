namespace XYEngine.Resources;

public class MaterialUI(Shader shader = null, params (string, object)[] properties)
	: Material(shader ?? Shader.GetDefaultUI(), properties)
{
	public const string TEXTURE = "mainTex";
	public const string UV_RECT = "uvRect";
	public const string NINEPATCH = "ninepatch";
	public const string NINEPATCH_SCALE = "ninepatchScale";
	public const string CORNER_RADIUS = "cornerRadius";
	
	public MaterialUI SetTexture(Texture2D texture)
	{
		SetProperty(TEXTURE, texture);
		
		return this;
	}
	
	public MaterialUI SetUVRect(Rect rect)
	{
		SetProperty(UV_RECT, rect);
		return this;
	}
	
	public MaterialUI SetCornerRadius(RegionInt radius)
	{
		SetProperty(CORNER_RADIUS, radius);
		return this;
	}
	
	public MaterialUI SetNinePatch(Region padding, float? scale = null)
	{
		SetProperty(NINEPATCH, padding);
		if (scale.HasValue)
			SetProperty(NINEPATCH_SCALE, scale.Value);
		return this;
	}
}