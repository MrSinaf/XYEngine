using XYEngine.Inputs;
using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class CheckBox : UIElement
{
	public readonly Label label;
	
	public string text { get => label.text; set => label.text = value; }
	
	public bool isCheck
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			onChecked(field);
		}
	}
	
	public event Action<bool> onChecked;
	
	public CheckBox(string text, Action<bool> onChecked, string prefab = null)
	{
		base.AddChild(label = new Label(text));
		this.onChecked = onChecked ?? delegate { };
		
		Input.clickDown += OnClickDown;
		
		UIPrefab.Apply(this, prefab);
	}
	
	private void OnClickDown(MouseButton mouseButton)
	{
		if (!isActif || mouseButton != MouseButton.Left || !ContainsPoint(canvas.mousePosition))
			return;
		
		isCheck = !isCheck;
	}
	
	protected override void OnDestroy() => Input.clickDown -= OnClickDown;
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(CheckBox e)
	{
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.size = new Vector2Int(200, 30);
		e.tint = new Color(30, 30, 45);
		e.padding = new RegionInt(10, 0);
		
		var ui = Primitif.uiTexture;
		var checker = new UIElement
		{
			material = new MaterialUI().SetTexture(ui),
			mesh = MeshFactory.CreateQuad(Vector2.one, uvs: ui.GetUVRegion(new RectInt(8, 8, 8))).Apply(),
			size = new Vector2Int(16),
			pivotAndAnchors = new Vector2(1, 0.5F)
		};
		e.AddChild(checker);
		
		e.label.pivotAndAnchors = new Vector2(0, 0.5F);
		e.label.tint = Color.white;
		
		UIEvent.Register(e, UIEvent.Type.MouseClick, () => checker.tint = e.isCheck ? Color.green : Color.white);
	}
}