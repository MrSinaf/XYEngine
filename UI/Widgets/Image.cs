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
	
	public Image(Texture2D texture, Region? uvs = null)
	{
		base.mesh = MeshFactory.CreateQuad(Vector2.one, uvs).Apply();
		base.material = new MaterialUI();
		base.size = texture.size;
		
		this.texture = texture;
	}
	
	public Image(Material material, Region? uvs = null)
	{
		base.mesh = MeshFactory.CreateQuad(Vector2.one, uvs).Apply();
		base.material = material;
	}
}