using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class Panel : UIElement
{
	public Panel(string prefab = null) => UIPrefab.Apply(this, prefab);
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(Panel e)
	{
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel).SetCornerRadius(new RegionInt(8));
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.anchorMin = Vector2.zero;
		e.anchorMax = Vector2.one;
		e.margin = new RegionInt(10);
		e.opacity = 0.5F;
	}
}