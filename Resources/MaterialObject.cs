namespace XYEngine.Resources;

public class MaterialObject(Shader shader = null, params (string, object)[] properties) : Material(shader ?? Shader.GetDefault(), properties)
{
	public const string TEXTURE = "mainTex";
	
	public MaterialObject SetTexture(Texture2D texture)
	{
		SetProperty(TEXTURE, texture);
		return this;
	}
}