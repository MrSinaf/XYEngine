namespace XYEngine.Utils;

public partial class Grid<T>
{
	public ref T this[int x, int y, int z] => ref GetObject(x, y, z);
	public ref T this[Vector2Int position, int depth] => ref GetObject(position.x, position.y, depth);
	
	public Grid(int width, int height, int depth, Func<int, int, int, T> func, bool startVertical = true)
	{
		this.width = width;
		this.height = height;
		this.depth = depth;
		
		gridArray = new T[width, height, depth];
		if (startVertical)
			for (var x = 0; x < width; x++)
			for (var y = 0; y < height; y++)
			for (var z = 0; z < depth; z++)
				gridArray[x, y, z] = func(x, y, z);
		else
			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
			for (var z = 0; z < depth; z++)
				gridArray[x, y, z] = func(x, y, z);
	}
	
	public Grid(int width, int height, int depth)
	{
		this.width = width;
		this.height = height;
		this.depth = depth;
		
		gridArray = new T[width, height, depth];
	}
	
	public void SetObject(int x, int y, int z, T obj)
	{
		try
		{
			gridArray[x, y, z] = obj;
		}
		catch (IndexOutOfRangeException)
		{
			throw new Exception($"The position: ({x}:{y}:{z}) is out of the grid's boundaries.");
		}
	}
	
	public bool TrySetObject(int x, int y, int z, T obj)
	{
		if (CheckOutOfRangeArray(x, y, z))
			return false;
		
		gridArray[x, y, z] = obj;
		return true;
	}
	
	public T[] GetZObjects(int x, int y)
	{
		var objects = new List<T>();
		if (CheckOutOfRangeArray(x, y)) return objects.ToArray();
		
		for (var z = 0; z < depth; z++)
		{
			var obj = gridArray[x, y, z];
			if (obj != null) objects.Add(obj);
		}
		
		return objects.ToArray();
	}
	
	public ref T GetObject(int x, int y, int z)
	{
		try
		{
			return ref gridArray[x, y, z];
		}
		catch (IndexOutOfRangeException)
		{
			throw new Exception($"The position: ({x}:{y}:{z}) is out of the grid's boundaries.");
		}
	}
	
	public bool TryGetObject(int x, int y, int z, out T obj)
	{
		if (CheckOutOfRangeArray(x, y, z))
		{
			obj = default;
			return false;
		}
		
		obj = gridArray[x, y, z];
		return obj != null;
	}
	
	public bool CheckOutOfRangeArray(int x, int y, int z)
		=> x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth;
}