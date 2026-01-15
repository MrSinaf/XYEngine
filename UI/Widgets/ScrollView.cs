using XYEngine.Inputs;
using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class ScrollView : UIElement
{
	public readonly Mask mask;
	public readonly ScrollBar scrollBarVertical;
	public readonly ScrollBar scrollBarHorizontal;
	
	public readonly bool withVertical;
	public readonly bool withHorizontal;
	
	public int wheelStep
	{
		get
		{
			if (withHorizontal)
				return scrollBarHorizontal.wheelStep;
			if (withVertical)
				return scrollBarVertical.wheelStep;
			
			return 0;
		}
		set
		{
			if (withHorizontal)
				scrollBarHorizontal.wheelStep = value;
			if (withVertical)
				scrollBarVertical.wheelStep = value;
		}
	}
	
	private UIElement content;
	private bool isDirty;
	
	public ScrollView(bool vertical = true, bool horizontal = true, string prefab = null)
	{
		base.AddChild(mask = new Mask());
		withVertical = vertical;
		withHorizontal = horizontal;
		
		base.AddChild(scrollBarVertical = new ScrollBar(OnCursorChangedV, ScrollBarOrientation.Vertical));
		base.AddChild(scrollBarHorizontal = new ScrollBar(OnCursorChangedH, ScrollBarOrientation.Horizontal));
		
		Input.mouseScroll += OnMouseScroll;
		elementChanged += OnElementChanged;
		
		UIPrefab.Apply(this, prefab);
	}
	
	private void OnMouseScroll(Vector2 scroll)
	{
		if (!ContainsPoint(canvas.mousePosition))
			return;
		
		if (withVertical)
			scrollBarVertical.AddWheelImpulse(scroll.y);
		if (withHorizontal)
			scrollBarHorizontal.AddWheelImpulse(scroll.x);
	}
	
	private void OnElementChanged(UIElement obj) => isDirty = true;
	
	private void OnCursorChangedV(int value) => content.position = new Vector2Int(content.position.x, -value);
	
	private void OnCursorChangedH(int value) => content.position = new Vector2Int(-value, content.position.y);
	
	public override void AddChild(UIElement element)
	{
		content?.Destroy();
		
		mask.AddChild(content = element);
		content.position = Vector2Int.zero;
		content.elementChanged += OnElementChanged;
		isDirty = true;
	}
	
	public override void RemoveChild(UIElement element)
	{
		content.elementChanged -= OnElementChanged;
		content = null;
		isDirty = true;
		
		mask.RemoveChild(element);
	}
	
	protected override void OnEndDraw()
	{
		if (!isDirty)
			return;
		
		isDirty = false;
		if (content != null)
		{
			scrollBarVertical.contentLength = content.size.y;
			scrollBarHorizontal.contentLength = content.size.x;
		}
	}
	
	protected override void OnRemoved() => elementChanged -= OnElementChanged;
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(ScrollView e)
	{
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.tint = new Color(0x0B1926) { a = 120 };
		
		e.mask.anchorMin = Vector2.zero;
		e.mask.anchorMax = Vector2.one;
		e.mask.margin = new RegionInt(0, e.withHorizontal ? 10 : 0, e.withVertical ? 10 : 0, 0);
		
		if (e.withVertical)
		{
			e.scrollBarVertical.size = new Vector2Int(10, 0);
			e.scrollBarVertical.anchorMin = Vector2.right;
			e.scrollBarVertical.anchorMax = Vector2.one;
			e.scrollBarVertical.pivot = Vector2.right;
			e.scrollBarVertical.margin = new RegionInt(0, e.withHorizontal ? 10 : 0, 0, 0);
		}
		else
			e.scrollBarVertical.active = false;
		
		if (e.withHorizontal)
		{
			e.scrollBarHorizontal.size = new Vector2Int(0, 10);
			e.scrollBarHorizontal.anchorMin = Vector2.zero;
			e.scrollBarHorizontal.anchorMax = Vector2.right;
			e.scrollBarHorizontal.margin = new RegionInt(0, 0, e.withVertical ? 10 : 0, 0);
		}
		else
			e.scrollBarHorizontal.active = false;
		
		e.wheelStep = 20;
	}
}