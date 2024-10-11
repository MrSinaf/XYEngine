namespace XYEngine.UI.Widgets;

public class Layout : UIElement
{
    public bool vertical { get; init; }
    public int spacing { get; init; }

    public void ReArrange()
    {
        size = Vector2Int.zero;
        foreach (var child in childrenArray)
            SetChild(child);
    }
    
    public override void AddChild(UIElement element)
    {
        base.AddChild(element);
        SetChild(element);
    }
    
    public override void RemoveChild(UIElement element)
    {
        base.RemoveChild(element);
        ReArrange();
    }
    
    private void SetChild(UIElement element)
    {
        var spacing = nChild > 0 ? this.spacing : 0;
        if (vertical)
        {
            element.position = new Vector2Int(0, size.y + spacing);
            element.pivot = new Vector2(element.pivot.x, 0);
            element.anchorMin = new Vector2(element.anchorMin.x, 0);
            element.anchorMax = new Vector2(element.anchorMax.x, 0);
            
            size = new Vector2Int(element.scaledSize.x > size.x ?element.scaledSize.x : size.x, element.scaledSize.y + spacing + size.y + spacing);
        }
        else
        {
            element.position = new Vector2Int(size.x + spacing, 0);
            element.pivot = new Vector2(0, element.pivot.y);
            element.anchorMin = new Vector2(0, element.anchorMin.y);
            element.anchorMax = new Vector2(0, element.anchorMax.y);

            size = new Vector2Int(element.scaledSize.x + spacing + size.x + spacing, element.scaledSize.y > size.y ? element.scaledSize.y : size.y);
        }
    }
}