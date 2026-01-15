using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class Image : UIElement
{
	public Texture2D texture
	{
		get;
		set
		{
			field = value;
			material.SetProperty(MaterialUI.TEXTURE, field);
		}
	}
	
	public Image(Texture2D texture)
	{
		base.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		base.material = new MaterialUI();
		base.size = texture.size;
		
		this.texture = texture;
	}
	
	public Image(Material material)
	{
		base.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		base.material = material;
	}
	
	public void SetUV(RectInt rect)
	{
		if (material == null || texture == null)
			return;
		
		material.SetProperty(MaterialUI.UV_RECT, texture.GetUVRect(rect));
	}
	
	public void SetUV(Rect rect) => material?.SetProperty(MaterialUI.UV_RECT, rect);
}