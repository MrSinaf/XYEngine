using System.Collections;

namespace XYEngine.Utils;

public partial class Grid<T> : IEnumerable<T>
{
	private readonly T[,,] gridArray;
	public readonly int height;
	public readonly int width;
	public readonly int depth;
	
	public ref T this[int x, int y] => ref GetObject(x, y);
	public ref T this[Vector2Int position] => ref GetObject(position.x, position.y);
	
	public Grid(int width, int height, Func<int, int, T> func, bool startVertical = true)
	{
		this.width = width;
		this.height = height;
		depth = 1;
		
		gridArray = new T[width, height, depth];
		if (startVertical)
			for (var x = 0; x < width; x++)
			for (var y = 0; y < height; y++)
				gridArray[x, y, 0] = func(x, y);
		else
			for (var y = 0; y < height; y++)
			for (var x = 0; x < width; x++)
				gridArray[x, y, 0] = func(x, y);
	}
	
	public Grid(int width, int height)
	{
		this.width = width;
		this.height = height;
		depth = 1;
		
		gridArray = new T[width, height, depth];
	}
	
	public void GetXY(Vector2 value, out int x, out int y)
	{
		x = (int)Math.Floor(value.x);
		y = (int)Math.Floor(value.y);
	}
	
	public void EchangePosition(int xA, int yA, int xB, int yB)
		=> (gridArray[xA, yA, 0], gridArray[xB, yB, 0]) = (gridArray[xB, yB, 0], gridArray[xA, yA, 0]);
	
	public void SetObject(int x, int y, T obj)
	{
		try
		{
			gridArray[x, y, 0] = obj;
		}
		catch (IndexOutOfRangeException)
		{
			throw new Exception($"The position: ({x}:{y}) is out of the grid's boundaries.");
		}
	}
	
	public bool TrySetObject(int x, int y, T obj)
	{
		if (CheckOutOfRangeArray(x, y))
			return false;
		
		gridArray[x, y, 0] = obj;
		return true;
	}
	
	public ref T GetObject(int x, int y)
	{
		try
		{
			return ref gridArray[x, y, 0];
		}
		catch (IndexOutOfRangeException)
		{
			throw new Exception($"The position: ({x}:{y}) is out of the grid's boundaries.");
		}
	}
	
	public bool TryGetObject(int x, int y, out T obj)
	{
		if (CheckOutOfRangeArray(x, y))
		{
			obj = default;
			return false;
		}
		
		obj = gridArray[x, y, 0];
		return obj != null;
	}
	
	public bool CheckOutOfRangeArray(int x, int y) => x < 0 || x >= width || y < 0 || y >= height;
	
	public IEnumerator<T> GetEnumerator()
	{
		for (var y = 0; y < height; y++)
		for (var x = 0; x < width; x++)
		for (var z = 0; z < depth; z++)
			yield return gridArray[x, y, z];
	}
	
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}