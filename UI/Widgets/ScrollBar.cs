using XYEngine.Inputs;
using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public enum ScrollBarOrientation { Vertical, Horizontal }

public class ScrollBar : UIElement
{
	private const float VELOCITY_MIN = 0.01f;
	
	public readonly UIElement cursor;
	public readonly ScrollBarOrientation orientation;
	public event Action<int> onCursorChanged;
	public int wheelStep;
	
	private bool isDragging;
	private float clickOffset;
	private bool isDirty;
	
	private float velocity;
	private float virtualPosition;
	
	public int cursorPosition
	{
		get;
		set
		{
			field = Math.Clamp(value, 0, maxCursorPosition);
			cursor.position = orientation == ScrollBarOrientation.Vertical
				? new Vector2Int(0, (int)(size.y * cursorPosition / (float)contentLength))
				: new Vector2Int((int)(size.x * cursorPosition / (float)contentLength), 0);
			
			onCursorChanged?.Invoke(field);
		}
	}
	
	public int contentLength
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			isDirty = true;
		}
	}
	
	public void AddWheelImpulse(float axis)
	{
		if (axis == 0f)
			return;
		
		velocity += axis * wheelStep;
	}
	
	private int maxCursorPosition => contentLength -
									 (int)((orientation == ScrollBarOrientation.Vertical
										 ? cursor.scale.y
										 : cursor.scale.x) * contentLength);
	
	public ScrollBar(Action<int> onCursorChanged, ScrollBarOrientation orientation, string prefab = null)
	{
		if (onCursorChanged != null)
			this.onCursorChanged += onCursorChanged;
		
		cursor = new UIElement();
		this.orientation = orientation;
		base.AddChild(cursor);
		
		Input.clickDown += OnClickDown;
		Input.clickUp += OnClickUp;
		Input.mouseMove += OnMouseMove;
		Input.mouseScroll += OnMouseScroll;
		elementChanged += _ => isDirty = true;
		
		UIPrefab.Apply(this, prefab);
	}
	
	protected override void OnEndDraw()
	{
		var absVelocity = MathF.Abs(velocity);
		if (absVelocity <= VELOCITY_MIN && MathF.Abs(virtualPosition - cursorPosition) > 0.5f)
			virtualPosition = cursorPosition;
		
		if (absVelocity > VELOCITY_MIN)
		{
			virtualPosition += velocity;
			velocity *= 0.7F;
			
			if (absVelocity <= VELOCITY_MIN)
				velocity = 0f;
			
			cursorPosition = (int)MathF.Round(virtualPosition);
		}
		else
			virtualPosition = cursorPosition;
		
		if (!isDirty)
			return;
		
		var cursorLength = orientation == ScrollBarOrientation.Vertical ? cursor.size.y : cursor.size.x;
		var scaleRatio = contentLength < cursorLength ? 1F : 1F * cursorLength / contentLength;
		cursor.scale = orientation == ScrollBarOrientation.Vertical
			? new Vector2(1, scaleRatio)
			: new Vector2(scaleRatio, 1);
		
		cursorPosition = Math.Min(cursorPosition, maxCursorPosition);
	}
	
	private void OnClickDown(MouseButton obj)
	{
		var mousePosition = canvas.mousePosition;
		if (isDragging || !cursor.ContainsPoint(mousePosition))
			return;
		
		isDragging = true;
		clickOffset = orientation == ScrollBarOrientation.Vertical
			? mousePosition.y - cursor.realPosition.y
			: mousePosition.x - cursor.realPosition.x;
		
		// Ne pas avoir d'inertie pendant le Drag
		velocity = 0f;
		virtualPosition = cursorPosition;
	}
	
	private void OnMouseMove(Vector2 delta)
	{
		if (!isDragging)
			return;
		
		var mousePosition = canvas.mousePosition;
		if (orientation == ScrollBarOrientation.Vertical)
		{
			var relativeMousePos = mousePosition.y - realPosition.y - clickOffset;
			var maxCursorPos = size.y - cursor.size.y * cursor.scale.y;
			cursorPosition = (int)(relativeMousePos / maxCursorPos * maxCursorPosition);
		}
		else
		{
			var relativeMousePos = mousePosition.x - realPosition.x - clickOffset;
			var maxCursorPos = size.x - cursor.size.x * cursor.scale.x;
			cursorPosition = (int)(relativeMousePos / maxCursorPos * maxCursorPosition);
		}
		
		virtualPosition = cursorPosition;
	}
	
	private void OnClickUp(MouseButton obj) => isDragging = false;
	
	private void OnMouseScroll(Vector2 scroll)
	{
		if (!ContainsPoint(canvas.mousePosition))
			return;
		
		var axis = orientation == ScrollBarOrientation.Vertical ? scroll.y : scroll.x;
		if (axis != 0f)
			velocity += axis * wheelStep;
	}
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(ScrollBar e)
	{
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel).SetCornerRadius(new RegionInt(5));
		e.opacity = 0.25F;
		e.size = e.orientation == ScrollBarOrientation.Vertical ? new Vector2Int(10, 200) : new Vector2Int(200, 10);
		
		e.cursor.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.cursor.material = new MaterialUI().SetTexture(Primitif.whitePixel).SetCornerRadius(new RegionInt(5));
		e.cursor.anchorMin = Vector2.zero;
		e.cursor.anchorMax = Vector2.one;
		e.cursor.tint = new Color(0x2EAB64);
		
		e.wheelStep = 20;
	}
}