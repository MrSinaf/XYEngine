using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class ProgressBar : UIElement
{
	public readonly UIElement cursor;
	public event Action<float> onValueChanged = delegate { };
	
	public float maxValue
	{
		get;
		set
		{
			field = value;
			CheckValue();
		}
	}
	
	public float minValue
	{
		get;
		set
		{
			field = value;
			CheckValue();
		}
	}
	
	public float value
	{
		get;
		set
		{
			field = value > maxValue ? maxValue : value < minValue ? minValue : value;
			cursor.scale = new Vector2((field - minValue) / (-minValue + maxValue), 1);
			onValueChanged(field);
		}
	}
	
	public ProgressBar(float value, float minValue = 0, float maxValue = 1, string prefab = null)
	{
		base.AddChild(cursor = new UIElement { name = "cursor" });
		
		this.maxValue = maxValue;
		this.minValue = minValue;
		this.value = value;
		
		UIPrefab.Apply(this, prefab);
	}
	
	private void CheckValue() => value = value;
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(ProgressBar e)
	{
		e.cursor.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.cursor.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.cursor.anchorMin = Vector2.zero;
		e.cursor.anchorMax = Vector2.one;
		e.cursor.tint = new Color(0x00FF00);
		
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.size = new Vector2Int(200, 10);
	}
}