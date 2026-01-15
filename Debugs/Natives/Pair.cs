using System.Runtime.InteropServices;

namespace XYEngine.Debugs;

public struct ImGuiStoragePair
{
	public uint key;
	public UnionValue value;
}

public readonly unsafe struct ImGuiStoragePairPtr(ImGuiStoragePair* nativePtr)
{
	public ImGuiStoragePair* nativePtr { get; } = nativePtr;
	public ImGuiStoragePairPtr(IntPtr nativePtr) : this((ImGuiStoragePair*)nativePtr) { }
	
	public static implicit operator ImGuiStoragePairPtr(ImGuiStoragePair* nativePtr) => new (nativePtr);
	public static implicit operator ImGuiStoragePair*(ImGuiStoragePairPtr wrappedPtr) => wrappedPtr.nativePtr;
	public static implicit operator ImGuiStoragePairPtr(IntPtr nativePtr) => new (nativePtr);
}

[StructLayout(LayoutKind.Explicit)]
public struct UnionValue
{
	[FieldOffset(0)] public int valueI32;
	[FieldOffset(0)] public float valueF32;
	[FieldOffset(0)] public IntPtr valuePtr;
}