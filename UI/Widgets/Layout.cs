namespace XYEngine.UI.Widgets;

public class Layout : UIElement
{
	public bool vertical
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	public bool inverse
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	public int spacing
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	public float? alignment
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	private bool isDirty;
	
	public override void AddChild(UIElement element)
	{
		base.AddChild(element);
		element.elementChanged += OnElementChanged;
		isDirty = true;
	}
	
	public override void RemoveChild(UIElement element)
	{
		base.RemoveChild(element);
		element.elementChanged -= OnElementChanged;
		isDirty = true;
	}
	
	protected override void OnBeginDraw()
	{
		if (isDirty)
			ReArrange();
	}
	
	protected override void OnEndDraw()
	{
		isDirty = false;
	}
	
	private void OnElementChanged(UIElement element)
	{
		isDirty = true;
	}
	
	private void ReArrange()
	{
		size = Vector2Int.zero;
		
		var array = childrenArray;
		if (inverse && !vertical || !inverse && vertical)
			Array.Reverse(array);
		
		for (var i = 0; i < array.Length; i++)
		{
			array[i].SimuleDraw();
			SetChild(array[i], i);
		}
	}
	
	private void SetChild(UIElement element, int index)
	{
		var spacing = index == 0 ? 0 : this.spacing;
		
		if (vertical)
		{
			element.position = new Vector2Int(0, size.y + spacing);
			if (alignment.HasValue)
			{
				element.pivot = new Vector2(alignment.Value, 0);
				element.anchorMin = new Vector2(alignment.Value, 0);
				element.anchorMax = new Vector2(alignment.Value, 0);
			}
			else
			{
				element.pivot = new Vector2(element.pivot.x, 0);
				element.anchorMin = new Vector2(element.anchorMin.x, 0);
				element.anchorMax = new Vector2(element.anchorMax.x, 0);
			}
			
			size = new Vector2Int(element.scaledSize.x > size.x ? (int)element.scaledSize.x : size.x, (int)element.scaledSize.y + size.y + spacing);
		}
		else
		{
			element.position = new Vector2Int(size.x + spacing, 0);
			if (alignment.HasValue)
			{
				element.pivot = new Vector2(0, alignment.Value);
				element.anchorMin = new Vector2(0, alignment.Value);
				element.anchorMax = new Vector2(0, alignment.Value);
			}
			else
			{
				element.pivot = new Vector2(0, element.pivot.y);
				element.anchorMin = new Vector2(0, element.anchorMin.y);
				element.anchorMax = new Vector2(0, element.anchorMax.y);
			}
			
			size = new Vector2Int((int)element.scaledSize.x + size.x + spacing, element.scaledSize.y > size.y ? (int)element.scaledSize.y : size.y);
		}
	}
}