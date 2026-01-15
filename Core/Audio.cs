using Silk.NET.OpenAL;
using XYEngine.Utils;

namespace XYEngine;

public static unsafe class Audio
{
	public static AL al { get; private set; }
	public static ALContext alContext { get; private set; }
	private static Device* device;
	private static Context* context;
	
	public static float volume
	{
		get;
		set
		{
			field = MathUtils.Range(value, 0, 1);
			al.SetListenerProperty(ListenerFloat.Gain, field);
			CheckError();
		}
	}
	
	internal static void Initialize()
	{
		al = AL.GetApi();
		alContext = ALContext.GetApi();
		
		device = alContext.OpenDevice("");
		context = alContext.CreateContext(device, null);
		
		alContext.MakeContextCurrent(context);
		
		al.GetListenerProperty(ListenerFloat.Gain, out var gVolume);
		volume = gVolume;
	}
	
	public static uint CreateBuffer(byte[] data, BufferFormat format, int frequency)
	{
		var buffer = al.GenBuffer();
		al.BufferData(buffer, format, data, frequency);
		CheckError();
		
		return buffer;
	}
	
	public static uint CreateSource(uint buffer)
	{
		var source = al.GenSource();
		al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
		CheckError();
		
		return source;
	}
	
	private static void CheckError()
	{
		var error = al.GetError();
		if (error != AudioError.NoError)
			throw new Exception(error.ToString());
	}
	
	internal static void Dispose()
	{
		alContext.MakeContextCurrent(null);
		alContext.DestroyContext(context);
		context = null;
		
		alContext.CloseDevice(device);
		device = null;
		
		alContext.Dispose();
		al.Dispose();
	}
}