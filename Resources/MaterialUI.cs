namespace XYEngine.Resources;

public class MaterialUI(Shader shader = null, params (string, object)[] properties) : Material(shader ?? Shader.GetDefaultUI(), properties)
{
	public const string TEXTURE = "mainTex";
	public const string UV_RECT = "uvRect";
	public const string PADDING = "padding";
	public const string PADDING_SCALE = "paddingScale";
	
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
	
	public MaterialUI SetPadding(Region padding, float? scale = null)
	{
		SetProperty(PADDING, padding);
		if (scale.HasValue)
			SetProperty(PADDING_SCALE, scale.Value);
		
		return this;
	}
}