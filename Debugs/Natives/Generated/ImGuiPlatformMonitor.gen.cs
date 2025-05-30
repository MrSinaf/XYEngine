using System.Runtime.CompilerServices;

namespace XYEngine.Debugs
{
    public unsafe partial struct ImGuiPlatformMonitor
    {
        public System.Numerics.Vector2 MainPos;
        public System.Numerics.Vector2 MainSize;
        public System.Numerics.Vector2 WorkPos;
        public System.Numerics.Vector2 WorkSize;
        public float DpiScale;
        public void* PlatformHandle;
    }
    public unsafe partial struct ImGuiPlatformMonitorPtr
    {
        public ImGuiPlatformMonitor* NativePtr { get; }
        public ImGuiPlatformMonitorPtr(ImGuiPlatformMonitor* nativePtr) => NativePtr = nativePtr;
        public ImGuiPlatformMonitorPtr(IntPtr nativePtr) => NativePtr = (ImGuiPlatformMonitor*)nativePtr;
        public static implicit operator ImGuiPlatformMonitorPtr(ImGuiPlatformMonitor* nativePtr) => new ImGuiPlatformMonitorPtr(nativePtr);
        public static implicit operator ImGuiPlatformMonitor* (ImGuiPlatformMonitorPtr wrappedPtr) => wrappedPtr.NativePtr;
        public static implicit operator ImGuiPlatformMonitorPtr(IntPtr nativePtr) => new ImGuiPlatformMonitorPtr(nativePtr);
        public ref System.Numerics.Vector2 MainPos => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->MainPos);
        public ref System.Numerics.Vector2 MainSize => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->MainSize);
        public ref System.Numerics.Vector2 WorkPos => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->WorkPos);
        public ref System.Numerics.Vector2 WorkSize => ref Unsafe.AsRef<System.Numerics.Vector2>(&NativePtr->WorkSize);
        public ref float DpiScale => ref Unsafe.AsRef<float>(&NativePtr->DpiScale);
        public IntPtr PlatformHandle { get => (IntPtr)NativePtr->PlatformHandle; set => NativePtr->PlatformHandle = (void*)value; }
        public void Destroy()
        {
            ImGuiNative.ImGuiPlatformMonitor_destroy((ImGuiPlatformMonitor*)(NativePtr));
        }
    }
}
