namespace XYEngine.UI;

public sealed class RootElement : UIElement
{
	protected override void OnAdded() => throw new AccessViolationException("You can't add a RootElement to a UIElement!");
	
	public override void MarkMatrixIsDirty()
	{
		base.MarkMatrixIsDirty();
		UnmarkMatrixIsDirty();
	}
	
	internal void UpdateSize(RegionInt clipArea, Vector2Int size)
	{
		this.clipArea = clipArea;
		this.size = size;
	}
}