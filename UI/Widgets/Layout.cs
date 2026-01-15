namespace XYEngine.UI.Widgets;

public class Layout : UIElement
{
	#region Properties
	
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
	
	public bool fitAlignment
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	} = true;
	
	public float? alignment
	{
		get;
		set
		{
			field = value;
			isDirty = true;
		}
	}
	
	#endregion
	
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
	
	public override void RemoveChildren()
	{
		var array = childrenArray;
		
		foreach (var element in array)
			element.elementChanged -= OnElementChanged;
		base.RemoveChildren();
		isDirty = true;
	}
	
	protected override void OnBeginDraw()
	{
		if (isDirty)
			ReArrange();
	}
	
	private void OnElementChanged(UIElement element) => isDirty = true;
	
	private void ReArrange()
	{
		size = new Vector2Int(!fitAlignment && vertical ? size.x : 0, !fitAlignment && !vertical ? size.y : 0);
		
		var array = childrenArray;
		if (inverse && !vertical || !inverse && vertical)
			Array.Reverse(array);
		
		for (var i = 0; i < array.Length; i++)
		{
			var child = array[i];
			if (child.isActif)
			{
				child.SimuleDraw();
				SetChild(child, i);
			}
		}
		
		size += new Vector2Int(!vertical ? padding.position00.x + padding.position11.x : 0,
							   vertical ? padding.position00.y + padding.position11.y : 0);
		
		BuildMatrix();
		foreach (var child in array)
			child.BuildMatrix();
		
		isDirty = false;
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
				if (element.anchorMin != element.anchorMax)
					element.size = new Vector2Int(0, element.size.y);
				
				element.pivot = new Vector2(element.pivot.x, 0);
				element.anchorMin = new Vector2(element.anchorMin.x, 0);
				element.anchorMax = new Vector2(element.anchorMax.x, 0);
			}
			
			size = new Vector2Int(fitAlignment && element.scaledSize.x > size.x ? (int)element.scaledSize.x : size.x,
								  size.y + spacing + (int)element.scaledSize.y);
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
				if (element.anchorMin != element.anchorMax)
					element.size = new Vector2Int(element.size.x, 0);
				
				element.pivot = new Vector2(0, element.pivot.y);
				element.anchorMin = new Vector2(0, element.anchorMin.y);
				element.anchorMax = new Vector2(0, element.anchorMax.y);
			}
			
			size = new Vector2Int(size.x + spacing + (int)element.scaledSize.x,
								  element.scaledSize.y > size.y ? (int)element.scaledSize.y : size.y);
		}
	}
}