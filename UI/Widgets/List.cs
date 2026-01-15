namespace XYEngine.UI.Widgets;

public class List : UIElement
{
	public readonly ScrollView scrollView;
	public readonly Layout layout;
	
	public List(string prefab = null)
	{
		base.AddChild(scrollView = new ScrollView(horizontal: false));
		scrollView.AddChild(layout = new Layout { vertical = true });
		
		UIPrefab.Apply(this, prefab);
	}
	
	public override void AddChild(UIElement element) => layout.AddChild(element);
	
	public override void RemoveChild(UIElement element) => layout.RemoveChild(element);
	
	public override void RemoveChildren() => layout.RemoveChildren();
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(List e)
	{
		e.size = new Vector2Int(300, 200);
		
		e.scrollView.anchorMin = Vector2.zero;
		e.scrollView.anchorMax = Vector2.one;
		
		e.layout.anchorMin = Vector2.zero;
		e.layout.anchorMax = Vector2.right;
	}
}