using Silk.NET.OpenGL;
using static XYEngine.Graphics;

namespace XYEngine.Rendering;

public class GBuffer : IDisposable
{
	private static uint? currentBound;
	
	public readonly uint handle;
	public readonly uint sizeInBytes;
	public BufferType type => (BufferType)target;
	
	private readonly BufferTargetARB target;
	
	public bool isDisposed { get; protected set; }
	
	public static unsafe GBuffer Create<T>(BufferType type, T[] data, bool dynamic = false) where T : unmanaged
	{
		fixed (void* ptr = data)
		{
			return new GBuffer(type, (uint)(data.Length * sizeof(T)), ptr, dynamic);
		}
	}
	
	private unsafe GBuffer(BufferType type, uint sizeInBytes, void* data, bool dynamic)
	{
		this.sizeInBytes = sizeInBytes;
		target = (BufferTargetARB)type;
		handle = gl.GenBuffer();
		
		Bind();
		gl.BufferData(target, sizeInBytes, data, BufferUsageARB.DynamicDraw);
	}
	
	public unsafe void Set<T>(uint offsetInBytes, T[] data) where T : unmanaged
	{
		Bind();
		fixed (void* ptr = data)
		{
			gl.BufferSubData(target, (nint)offsetInBytes, (uint)(data.Length * sizeof(T)), ptr);
		}
	}
	
	private void Bind()
	{
		if (currentBound == handle)
			return;
		
		gl.BindBuffer(target, handle);
		currentBound = handle;
	}
	
	public void Dispose()
	{
		if (isDisposed)
			return;
		
		if (currentBound == handle)
			currentBound = null;
		
		gl.DeleteBuffer(handle);
		isDisposed = true;
	}
}