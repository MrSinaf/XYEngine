using System.Runtime.CompilerServices;

namespace XYEngine.Debugs;

public readonly unsafe struct ImVector(int size, int capacity, IntPtr data)
{
	public readonly int size = size;
	public readonly int capacity = capacity;
	public readonly IntPtr data = data;
	
	public ref T Ref<T>(int index) => ref Unsafe.AsRef<T>((byte*)data + index * Unsafe.SizeOf<T>());
	
	public IntPtr Address<T>(int index) => (IntPtr)((byte*)data + index * Unsafe.SizeOf<T>());
}

public unsafe struct ImVector<T>
{
	public readonly int Size;
	public readonly int Capacity;
	public readonly IntPtr Data;
	
	public ImVector(ImVector vector)
	{
		Size = vector.size;
		Capacity = vector.capacity;
		Data = vector.data;
	}
	
	public ImVector(int size, int capacity, IntPtr data)
	{
		Size = size;
		Capacity = capacity;
		Data = data;
	}
	
	public ref T this[int index] => ref Unsafe.AsRef<T>((byte*)Data + index * Unsafe.SizeOf<T>());
}

public unsafe struct ImPtrVector<T>
{
	public readonly int Size;
	public readonly int Capacity;
	public readonly IntPtr Data;
	private readonly int _stride;
	
	public ImPtrVector(ImVector vector, int stride)
		: this(vector.size, vector.capacity, vector.data, stride) { }
	
	public ImPtrVector(int size, int capacity, IntPtr data, int stride)
	{
		Size = size;
		Capacity = capacity;
		Data = data;
		_stride = stride;
	}
	
	public T this[int index]
	{
		get
		{
			var address = (byte*)Data + index * _stride;
			var ret = Unsafe.Read<T>(&address);
			return ret;
		}
	}
}