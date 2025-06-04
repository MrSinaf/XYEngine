using System.Runtime.InteropServices;

namespace XYEngine.Debugs;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public unsafe delegate int ImGuiInputTextCallback(ImGuiInputTextCallbackData* data);