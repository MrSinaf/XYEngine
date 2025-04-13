using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class Image : UIElement
{
	public Image(Texture2D texture, Region? uvs = null)
	{
		base.mesh = MeshFactory.CreateQuad(Vector2.one, uvs).Apply();
		base.material = new MaterialUI().SetTexture(texture);
		base.size = texture.size;
	}
	
	public Image(Material material, Region? uvs = null)
	{
		base.mesh = MeshFactory.CreateQuad(Vector2.one, uvs).Apply();
		base.material = material;
	}
}