namespace XYEngine.UI;

public sealed class RootElement : UIElement
{
	public const string MODIFICATION_NOT_ALLOWED = "Modification of '{0}' is not allowed on RootElement.";
	
	public override Vector2Int size
	{
		get => base.size;
		set => throw new AccessViolationException(string.Format(MODIFICATION_NOT_ALLOWED, nameof(size)));
	}
	
	public override Vector2 scale
	{
		get => base.scale;
		set => throw new AccessViolationException(string.Format(MODIFICATION_NOT_ALLOWED, nameof(scale)));
	}
	
	public RootElement(Canvas canvas) => SetCanvas(canvas);
	
	protected override void OnAdded()
		=> throw new AccessViolationException("You can't add a RootElement to a UIElement!");
	
	public override void MarkMatrixIsDirty()
	{
		base.MarkMatrixIsDirty();
		UnmarkMatrixIsDirty();
	}
	
	protected override void OnDestroy() => SetCanvas(null);
	
	internal void UpdateSize(Vector2Int size)
	{
		clipArea = new RegionInt(Vector2Int.zero, size);
		base.size = size;
	}
}