using System.Runtime.CompilerServices;

namespace XYEngine.Debugs
{
    public unsafe partial struct ImDrawCmdHeader
    {
        public System.Numerics.Vector4 ClipRect;
        public IntPtr TextureId;
        public uint VtxOffset;
    }
    public unsafe partial struct ImDrawCmdHeaderPtr
    {
        public ImDrawCmdHeader* NativePtr { get; }
        public ImDrawCmdHeaderPtr(ImDrawCmdHeader* nativePtr) => NativePtr = nativePtr;
        public ImDrawCmdHeaderPtr(IntPtr nativePtr) => NativePtr = (ImDrawCmdHeader*)nativePtr;
        public static implicit operator ImDrawCmdHeaderPtr(ImDrawCmdHeader* nativePtr) => new ImDrawCmdHeaderPtr(nativePtr);
        public static implicit operator ImDrawCmdHeader* (ImDrawCmdHeaderPtr wrappedPtr) => wrappedPtr.NativePtr;
        public static implicit operator ImDrawCmdHeaderPtr(IntPtr nativePtr) => new ImDrawCmdHeaderPtr(nativePtr);
        public ref System.Numerics.Vector4 ClipRect => ref Unsafe.AsRef<System.Numerics.Vector4>(&NativePtr->ClipRect);
        public ref IntPtr TextureId => ref Unsafe.AsRef<IntPtr>(&NativePtr->TextureId);
        public ref uint VtxOffset => ref Unsafe.AsRef<uint>(&NativePtr->VtxOffset);
    }
}
