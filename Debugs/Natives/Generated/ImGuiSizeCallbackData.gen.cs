using System.Runtime.CompilerServices;

namespace XYEngine.Debugs
{
    public unsafe partial struct ImGuiSizeCallbackData
    {
        public void* UserData;
        public System.Numerics.Vector2 Pos;
        public System.Numerics.Vector2 CurrentSize;
        public System.Numerics.Vector2 DesiredSize;
    }
    public unsafe partial struct ImGuiSizeCallbackDataPtr
    {
        public ImGuiSizeCallbackData* NativePtr { get; }
        public ImGuiSizeCallbackDataPtr(ImGuiSizeCallbackData* nativePtr) => NativePtr = nativePtr;
        public ImGuiSizeCallbackDataPtr(IntPtr nativePtr) => NativePtr = (ImGuiSizeCallbackData*)nativePtr;
        public static implicit operator ImGuiSizeCallbackDataPtr(ImGuiSizeCallbackData* nativePtr) => new ImGuiSizeCallbackDataPtr(nativePtr);
        public static implicit operator ImGuiSizeCallbackData* (ImGuiSizeCallbackDataPtr wrappedPtr) => wrappedPtr.NativePtr;
        public static implicit operator ImGuiSizeCallbackDataPtr(IntPtr nativePtr) => new ImGuiSizeCallbackDataPtr(nativePtr);
        public IntPtr UserData { get => (IntPtr)NativePtr->UserData; set => NativePtr->UserData = (void*)value; }
        public ref System.Numerics.Vector2 Pos => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->Pos);
        public ref System.Numerics.Vector2 CurrentSize => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->CurrentSize);
        public ref System.Numerics.Vector2 DesiredSize => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->DesiredSize);
    }
}
