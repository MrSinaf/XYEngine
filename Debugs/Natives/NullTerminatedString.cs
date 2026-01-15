using System.Text;

namespace XYEngine.Debugs;

public readonly unsafe struct NullTerminatedString(byte* data)
{
	public readonly byte* data = data;
	
	public override string ToString()
	{
		var length = 0;
		var ptr = data;
		while (*ptr != 0)
		{
			length += 1;
			ptr += 1;
		}
		
		return Encoding.ASCII.GetString(data, length);
	}
	
	public static implicit operator string(NullTerminatedString nts) => nts.ToString();
}