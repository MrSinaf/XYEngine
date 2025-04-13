using XYEngine.Inputs;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public enum ScrollBarOrientation { Vertical, Horizontal }

public class ScrollBar : UIElement
{
	public readonly UIElement cursor;
	public readonly ScrollBarOrientation orientation;
	public event Action<int> onCursorChanged;
	
	private bool isDragging;
	private float clickOffset;
	
	
	public int cursorPosition
	{
		get;
		set
		{
			var clampedValue = Math.Clamp(value, 0, maxCursorPosition);
			if (field != clampedValue)
			{
				field = clampedValue;
				cursor.position = orientation == ScrollBarOrientation.Vertical
									  ? new Vector2Int(0, (int)(size.y * cursorPosition / (float)contentLength))
									  : new Vector2Int((int)(size.x * cursorPosition / (float)contentLength), 0);
				onCursorChanged?.Invoke(field);
			}
		}
	}
	
	
	public int contentLength
	{
		get;
		set
		{
			if (field == value) return;
			
			field = value;
			var cursorLength = orientation == ScrollBarOrientation.Vertical ? cursor.size.y : cursor.size.x;
			var scaleRatio = contentLength < cursorLength ? 1F : 1F * cursorLength / contentLength;
			cursor.scale = orientation == ScrollBarOrientation.Vertical ? new Vector2(1, scaleRatio) : new Vector2(scaleRatio, 1);
			
			cursorPosition = Math.Min(cursorPosition, maxCursorPosition);
		}
	}
	
	private int maxCursorPosition => contentLength - (int)((orientation == ScrollBarOrientation.Vertical ? cursor.scale.y : cursor.scale.x) * contentLength);
	
	
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
		
		UIPrefab.Apply(this, prefab);
	}
	
	private void OnClickDown(MouseButton obj)
	{
		if (isDragging || !cursor.ContainsPoint(Input.mousePosition))
			return;
		
		isDragging = true;
		clickOffset = orientation == ScrollBarOrientation.Vertical ? Input.mousePosition.y - cursor.realPosition.y : Input.mousePosition.x - cursor.realPosition.x;
	}
	
	private void OnMouseMove(Vector2 delta)
	{
		if (!isDragging)
			return;
		
		if (orientation == ScrollBarOrientation.Vertical)
		{
			var relativeMousePos = Input.mousePosition.y - realPosition.y - clickOffset;
			var maxCursorPos = size.y - cursor.size.y * cursor.scale.y;
			cursorPosition = (int)(relativeMousePos / maxCursorPos * maxCursorPosition);
		}
		else
		{
			var relativeMousePos = Input.mousePosition.x - realPosition.x - clickOffset;
			var maxCursorPos = size.x - cursor.size.x * cursor.scale.x;
			cursorPosition = (int)(relativeMousePos / maxCursorPos * maxCursorPosition);
		}
	}
	
	private void OnClickUp(MouseButton obj) => isDragging = false;
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(ScrollBar e)
	{
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.size = e.orientation == ScrollBarOrientation.Vertical ? new Vector2Int(10, 200) : new Vector2Int(200, 10);
		
		e.cursor.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.cursor.material = new MaterialUI().SetTexture(Primitif.whitePixel);
		e.cursor.anchorMin = Vector2.zero;
		e.cursor.anchorMax = Vector2.one;
		e.cursor.tint = new Color(0x00FF00);
	}
}