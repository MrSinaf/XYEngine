using XYEngine.Resources;

namespace XYEngine.UI;

public class UIElement
{
	public string name;
	public UIElement parent { get; private set; }
	
	public int nChild => children.Count;
	public UIElement[] childrenArray => children.ToArray();
	
	public bool isActif => active && parentActive && !isDestroyed;
	public bool canDraw => (!overflowHidden || isObservable) && material != null && mesh is { isValid: true };
	public bool isObservable => clipArea.position11 != Vector2Int.zero;
	
	public bool isDestroyed { get; private set; }
	public Vector2Int realPosition { get; private set; }
	public Matrix3X3 inversedMatrix { get; private set; }
	public RegionInt clipArea { get; protected set; }
	
	public bool overflowHidden { get; set; } = true;
	
	protected bool scaleWithoutSize;
	
	private readonly List<UIElement> children = [];
	private bool dirtyMatrix;
	private bool dirtyScaledSize;
	
	public event Action<UIElement> elementChanged = _ => { };
	
	#region GetSet
	
	private bool parentActive
	{
		get;
		set
		{
			field = value;
			foreach (var child in children)
				child.parentActive = value;
		}
	} = true;
	
	protected virtual Vector2Int offset
	{
		get;
		set
		{
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual Mesh mesh { get; set; }
	
	public virtual Material material { get; set; }
	
	public virtual bool active
	{
		get;
		set
		{
			field = value;
			foreach (var child in children)
				child.parentActive = value;
			
			elementChanged(this);
		}
	} = true;
	
	public virtual Vector2Int position
	{
		get;
		set
		{
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual Vector2Int size
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			dirtyScaledSize = true;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual int rotation
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual Vector2 scale
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			dirtyScaledSize = true;
			MarkMatrixIsDirty();
		}
	} = Vector2.one;
	
	public virtual Vector2 scaledSize
	{
		get
		{
			if (dirtyScaledSize)
			{
				field = size * scale;
				dirtyScaledSize = false;
			}
			
			return field;
		}
	}
	
	public virtual RegionInt margin
	{
		get;
		set
		{
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual RegionInt padding
	{
		get;
		set
		{
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual Vector2 pivot
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public Vector2 pivotAndAnchors
	{
		set
		{
			pivot = value;
			anchorMin = value;
			anchorMax = value;
		}
	}
	
	public Vector2 anchors
	{
		set
		{
			anchorMin = value;
			anchorMax = value;
		}
	}
	
	public virtual Vector2 anchorMin
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual Vector2 anchorMax
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			MarkMatrixIsDirty();
		}
	}
	
	public virtual Matrix3X3 matrix
	{
		private set
		{
			field = value;
			UnmarkMatrixIsDirty();
		}
		get
		{
			if (dirtyMatrix)
				BuildMatrix();
			
			return field;
		}
	} = Matrix3X3.Identity();
	
	public virtual bool visible { get; set; } = true;
	
	public virtual Color tint { get; set; } = Color.white;
	
	public virtual float opacity { get; set; } = 1;
	
	#endregion
	
	public virtual void AddChild(UIElement element)
	{
		if (element.isDestroyed)
			throw new Exception("Cannot add the element as a child because it is destroyed.");
		
		element.parent = this;
		element.parentActive = active;
		element.MarkMatrixIsDirty();
		element.OnAdded();
		
		children.Add(element);
	}
	
	public virtual void RemoveChild(UIElement element)
	{
		element.parent = null;
		element.OnRemoved();
		children.Remove(element);
	}
	
	public virtual T GetParent<T>() where T : UIElement
	{
		if (parent is T element)
			return element;
		
		foreach (var child in children)
			return child.GetParent<T>();
		
		return null;
	}
	
	public virtual T GetChild<T>(bool recursif = false) where T : UIElement
	{
		foreach (var child in children)
			if (child is T element)
				return element;
		
		if (recursif)
			foreach (var child in children)
				return child.GetChild<T>(true);
		
		return null;
	}
	
	public virtual T GetChild<T>(string name, bool recursif = false) where T : UIElement
	{
		foreach (var child in children)
			if (child is T element && element.name == name)
				return element;
		
		if (recursif)
			foreach (var child in children)
				return child.GetChild<T>(name, true);
		
		return null;
	}
	
	public virtual T[] GetChildren<T>(bool recursif = false) where T : UIElement
	{
		var list = new List<T>();
		
		foreach (var child in children)
			if (child is T element)
				list.Add(element);
		
		if (recursif)
			foreach (var child in children)
			foreach (var childBis in child.GetChildren<T>(true))
				list.Add(childBis);
		
		return list.ToArray();
	}
	
	public void SetOrphan() => parent?.RemoveChild(this);
	
	public void MoveInFront()
	{
		if (parent == null)
			throw new NullReferenceException("No parent has been assigned !");
		
		parent.children.Remove(this);
		parent.children.Add(this);
	}
	
	public void MoveInBack()
	{
		if (parent == null)
			throw new NullReferenceException("No parent has been assigned !");
		
		parent.children.Remove(this);
		parent.children.Insert(0, this);
	}
	
	public bool ContainsPoint(Vector2 point)
	{
		if (mesh is not { isValid: true })
			return false;
		
		if (overflowHidden)
			if (point.IsOutsideBounds(clipArea.position00, clipArea.position11))
				return false;
		
		var localPoint = inversedMatrix.TransformPoint(point - mesh.bounds.position);
		return scaleWithoutSize
				   ? localPoint.IsInsideBounds(Vector2.zero, scaledSize)
				   : localPoint.IsInsideBounds(mesh.bounds.position, -mesh.bounds.position);
	}
	
	public virtual void MarkMatrixIsDirty()
	{
		dirtyMatrix = true;
		foreach (var child in children)
			child.MarkMatrixIsDirty();
	}
	
	public virtual void UnmarkMatrixIsDirty()
	{
		if (!dirtyMatrix)
			return;
		
		dirtyMatrix = false;
	}
	
	public void Destroy()
	{
		OnDestroy();
		material = null;
		mesh = null;
		isDestroyed = true;
		
		if (parent is { isDestroyed: false })
			parent.RemoveChild(this);
		
		foreach (var child in children)
			child.Destroy();
		
		UIEvent.UnRegisterAllEvents(this);
	}
	
	public void SimuleDraw()
	{
		OnBeginDraw();
		// Ne fait rien ༼ つ ◕_◕ ༽つ
		OnEndDraw();
	}
	
	internal void Draw()
	{
		if (!isActif)
			return;
		
		if (dirtyMatrix)
			BuildMatrix();
		
		OnBeginDraw();
		
		if (canDraw && visible)
		{
			var program = material.shader.gProgram;
			program.SetUniform("modelSize", scaledSize);
			program.SetUniform("model", matrix);
			program.SetUniform("alpha", opacity);
			program.SetUniform("tint", tint);
			
			Graphics.DrawMesh(mesh, material);
		}
		
		foreach (var child in children)
			child.Draw();
		
		OnEndDraw();
	}
	
	protected virtual void OnAdded() { }
	protected virtual void OnBeginDraw() { }
	protected virtual void OnEndDraw() { }
	protected virtual void OnDestroy() { }
	protected virtual void OnRemoved() { }
	
	public void BuildMatrix()
	{
		var scaledPivotSize = (pivot * scaledSize).ToVector2Int();
		var parentBoundsPadding = parent.scaledSize - (parent.padding.position00 + parent.padding.position11);
		var calculatePosition = Vector2Int.zero;
		
		var boundsPosition = Vector2.zero;
		if (anchorMin != anchorMax)
		{
			var anchorSize = new Vector2(MathF.Abs(anchorMin.x - anchorMax.x), MathF.Abs(anchorMin.y - anchorMax.y)) * parentBoundsPadding;
			
			if (anchorSize.x == 0)
				anchorSize.x = size.x;
			else
			{
				anchorSize.x -= margin.position00.x + margin.position11.x;
				calculatePosition.x += margin.position00.x;
				scaledPivotSize.x = 0;
			}
			
			if (anchorSize.y == 0)
				anchorSize.y = size.y;
			else
			{
				anchorSize.y -= margin.position00.y + margin.position11.y;
				calculatePosition.y += margin.position00.y;
				scaledPivotSize.y = 0;
			}
			
			boundsPosition = anchorSize * pivot;
			size = anchorSize.ToVector2Int();
		}
		
		calculatePosition += position;
		
		realPosition = calculatePosition += parent.realPosition + parent.padding.position00 - scaledPivotSize + (parentBoundsPadding * anchorMin).ToVector2Int();
		CalculeClipArea();
		
		if (mesh is { isValid: true } && !scaleWithoutSize)
		{
			var boundsScaled = scaledSize * mesh.bounds.position;
			calculatePosition -= boundsScaled.ToVector2Int();
			boundsPosition += boundsScaled;
		}
		boundsPosition += scaledPivotSize;
		
		var matrixScale = scaleWithoutSize ? scale : scaledSize;
		matrix = Matrix3X3.CreateScale(matrixScale);
		if (rotation != 0)
		{
			matrix *= Matrix3X3.CreateTranslation(-boundsPosition) *
					  Matrix3X3.CreateRotation(float.DegreesToRadians(rotation)) *
					  Matrix3X3.CreateTranslation(boundsPosition);
		}
		matrix *= Matrix3X3.CreateTranslation(calculatePosition + offset);
		inversedMatrix = matrix.Inverse();
		
		elementChanged(this);
	}
	
	private void CalculeClipArea()
	{
		var topLeft = new Vector2(realPosition.x, realPosition.y);
		var topRight = new Vector2(realPosition.x + scaledSize.x, realPosition.y);
		var bottomLeft = new Vector2(realPosition.x, realPosition.y + scaledSize.y);
		var bottomRight = new Vector2(realPosition.x + scaledSize.x, realPosition.y + scaledSize.y);
		
		var pivotPosition = realPosition + (scaledSize * pivot).ToVector2Int();
		topLeft = RotatePointAroundPivot(topLeft, pivotPosition, float.DegreesToRadians(rotation));
		topRight = RotatePointAroundPivot(topRight, pivotPosition, float.DegreesToRadians(rotation));
		bottomLeft = RotatePointAroundPivot(bottomLeft, pivotPosition, float.DegreesToRadians(rotation));
		bottomRight = RotatePointAroundPivot(bottomRight, pivotPosition, float.DegreesToRadians(rotation));
		
		clipArea = new RegionInt(new Vector2Int((int)MathF.Floor(MathF.Min(topLeft.x, MathF.Min(topRight.x, MathF.Min(bottomLeft.x, bottomRight.x)))),
												(int)MathF.Floor(MathF.Min(topLeft.y, MathF.Min(topRight.y, MathF.Min(bottomLeft.y, bottomRight.y))))),
								 new Vector2Int((int)MathF.Ceiling(MathF.Max(topLeft.x, MathF.Max(topRight.x, MathF.Max(bottomLeft.x, bottomRight.x)))),
												(int)MathF.Ceiling(MathF.Max(topLeft.y, MathF.Max(topRight.y, MathF.Max(bottomLeft.y, bottomRight.y))))));
		if (overflowHidden)
			clipArea = clipArea.Intersection(parent.clipArea);
		
		Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float radians)
		{
			var cos = MathF.Cos(radians);
			var sin = MathF.Sin(radians);
			var translatedPoint = point - pivot;
			var rotatedX = translatedPoint.x * cos - translatedPoint.y * sin;
			var rotatedY = translatedPoint.x * sin + translatedPoint.y * cos;
			return new Vector2(rotatedX, rotatedY) + pivot;
		}
	}
}