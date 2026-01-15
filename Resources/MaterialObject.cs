namespace XYEngine.Resources;

public class MaterialObject(Shader shader = null, params (string, object)[] properties)
	: Material(shader ?? Shader.GetDefault(), properties)
{
	public const string TEXTURE = "mainTex";
	public const string TINT = "tint";
	public const string UV_RECT = "uvRect";
	public const string ALPHA = "alpha";
	
	public MaterialObject SetTexture(Texture texture)
	{
		SetProperty(TEXTURE, texture);
		return this;
	}
	
	public MaterialObject SetUVRect(Rect rect)
	{
		SetProperty(UV_RECT, rect);
		return this;
	}
	
	public MaterialObject SetAlpha(float alpha)
	{
		SetProperty(ALPHA, alpha);
		return this;
	}
	
	public MaterialObject SetTint(Color color)
	{
		SetProperty(TINT, color);
		return this;
	}
}