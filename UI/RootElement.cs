namespace XYEngine.UI;

public sealed class RootElement : UIElement
{
	private const string ERROR_ACCESS = "You can't access this property directly!";
	
	public override RegionInt clipArea { get => base.clipArea; set => throw new AccessViolationException(ERROR_ACCESS); }
	public override bool active { get => base.active; set => throw new AccessViolationException(ERROR_ACCESS); }
	public override Vector2Int size { get => base.size; set => throw new AccessViolationException(ERROR_ACCESS); }
	public override Vector2 anchorMin { get => base.anchorMin; set => throw new AccessViolationException(ERROR_ACCESS); }
	public override Vector2 anchorMax { get => base.anchorMax; set => throw new AccessViolationException(ERROR_ACCESS); }
	
	protected override void OnAdded() => throw new AccessViolationException("You can't add a RootElement to a UIElement!");
	
	public override void MarkMatrixIsDirty() { } // ignore
	
	internal void UpdateSize(RegionInt clipArea, Vector2Int size)
	{
		base.clipArea = clipArea;
		base.size = size;
	}
}