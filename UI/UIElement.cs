using XYEngine.Resources;

namespace XYEngine.UI;

public class UIElement
{
	public UIElement parent { get; private set; }
	
	public int nChild => children.Count;
	public UIElement[] childrenArray => children.ToArray();
	
	public bool isActif => active && parentActive && !isDestroyed;
	public bool canDraw => material != null && mesh is { isValid: true };
	public bool isDestroyed { get; private set; }
	
	protected Vector2Int realPosition { get; private set; }
	protected bool scaleWithoutSize;
	
	private readonly List<UIElement> children = [];
	private bool dirtyMatrix;
	private bool dirtyScaledSize;
	
	#region GetSet
	
	public virtual bool active
	{
		get;
		set
		{
			field = value;
			foreach (var child in children)
				child.parentActive = value;
		}
	} = true;
	
	public virtual Mesh mesh { get; set; }
	
	public virtual Material material { get; set; }
	
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
	
	public virtual Color tint { get; set; } = Color.white;
	
	public virtual float opacity { get; set; } = 1;
	
	#endregion
	
	protected virtual void OnAdded() { }
	
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
	
	public virtual T GetParentType<T>() where T : UIElement
	{
		if (parent is T element)
			return element;
		
		foreach (var child in children)
			return child.GetParentType<T>();
		
		return null;
	}
	
	public virtual T GetChildType<T>() where T : UIElement
	{
		foreach (var child in children)
			if (child is T element)
				return element;
		
		foreach (var child in children)
			return child.GetChildType<T>();
		
		return null;
	}
	
	public virtual T[] GetChildrenType<T>() where T : UIElement
	{
		var list = new List<T>();
		
		foreach (var child in children)
			if (child is T element)
				list.Add(element);
		
		foreach (var child in children)
		foreach (var childBis in child.GetChildrenType<T>())
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
	
	public bool ContainsPoint(Vector2Int point)
	{
		point -= realPosition;
		return point >= Vector2Int.zero && point <= scaledSize;
	}
	
	public void MarkMatrixIsDirty()
	{
		dirtyMatrix = true;
		foreach (var child in children)
			child.MarkMatrixIsDirty();
	}
	
	public void UnmarkMatrixIsDirty()
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
		foreach (var child in children)
			child.Destroy();
		
		UIEvent.UnRegisterAllEvents(this);
	}
	
	public void UpdateMatrix()
	{
		if (GetType() == typeof(RootElement))
			return;
		
		if (parent == null)
			throw new InvalidOperationException("To update the matrix, a parent is required.");
		
		parent.UpdateMatrix();
		BuildMatrix();
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
		
		OnBeginDraw();
		
		if (dirtyMatrix)
			BuildMatrix();
		
		if (canDraw)
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
	
	protected virtual void OnBeginDraw() { }
	protected virtual void OnEndDraw() { }
	protected virtual void OnDestroy() { }
	protected virtual void OnRemoved() { }
	
	private void BuildMatrix()
	{
		var scaledPivotSize = (pivot * scaledSize).ToVector2Int();
		var calculatePosition = Vector2Int.zero;
		
		if (anchorMin != anchorMax)
		{
			var anchorSize = new Vector2(MathF.Abs(anchorMin.x - anchorMax.x), MathF.Abs(anchorMin.y - anchorMax.y)) * parent.size;
			
			if (anchorSize.x == 0) anchorSize.x = size.x;
			else
			{
				anchorSize.x -= margin.position00.x + margin.position11.x;
				calculatePosition.x += margin.position00.x;
				scaledPivotSize.x = 0;
			}
			
			if (anchorSize.y == 0) anchorSize.y = size.y;
			else
			{
				anchorSize.y -= margin.position00.y + margin.position11.y;
				calculatePosition.y += margin.position00.y;
				scaledPivotSize.y = 0;
			}
			
			size = anchorSize.ToVector2Int();
		}
		
		realPosition = calculatePosition += position + parent.realPosition - scaledPivotSize + (parent.scaledSize * anchorMin).ToVector2Int();
		calculatePosition -= mesh is { isValid: true } ? (!scaleWithoutSize ? scaledSize * mesh.bounds.position : mesh.bounds.position).ToVector2Int() : Vector2Int.zero;
		var matrixScale = scaleWithoutSize ? scale : scaledSize;
		matrix = Matrix3X3.CreateScale(matrixScale) *
				 Matrix3X3.CreateRotation(float.DegreesToRadians(rotation)) *
				 Matrix3X3.CreateTranslation(calculatePosition);
	}
}