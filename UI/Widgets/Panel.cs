using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class Panel : UIElement
{
	public Panel(string prefab = null) => UIPrefab.Apply(this, prefab);
	
	public static void DefaultStyle(Panel e)
	{
		e.material = new Material(Shader.GetDefaultUI(), ("mainTex", AssetManager.GetEmbeddedAsset<Texture2D>("textures.white_pixel.png")));
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.anchorMin = Vector2.zero;
		e.anchorMax = Vector2.one;
		e.margin = new RegionInt(10);
		e.opacity = 0.5F;
	}
}