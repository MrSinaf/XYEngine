using XYEngine.Inputs;
using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class Button : UIElement
{
	public readonly Label label;
	
	public string text { get => label.text; set => label.text = value; }
	
	public event Action onClick;
	
	public Button(string text, Action onClick, string prefab = null)
	{
		base.AddChild(label = new Label(text));
		this.onClick = onClick ?? delegate { };
		
		Input.clickDown += OnClickDown;
		
		UIPrefab.Apply(this, prefab);
	}
	
	private void OnClickDown(MouseButton mouseButton)
	{
		if (!isActif || mouseButton != MouseButton.Left || !ContainsPoint(canvas.mousePosition))
			return;
		
		onClick.Invoke();
	}
	
	protected override void OnDestroy() => Input.clickDown -= OnClickDown;
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(Button e)
	{
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.size = new Vector2Int(200, 30);
		e.tint = new Color(30, 30, 45);
		
		e.label.pivotAndAnchors = new Vector2(0.5F);
		e.label.tint = Color.white;
		
		UIEvent.Register(e, UIEvent.Type.MouseEnter, () => e.tint = new Color(40, 40, 55));
		UIEvent.Register(e, UIEvent.Type.MouseExit, () => e.tint = new Color(30, 30, 45));
	}
}