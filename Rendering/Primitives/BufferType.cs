using Silk.NET.OpenGL;

namespace XYEngine.Rendering;

public enum BufferType
{
	VertexBuffer = BufferTargetARB.ArrayBuffer, ElementsBuffer = BufferTargetARB.ElementArrayBuffer,
	UniformBuffer = BufferTargetARB.UniformBuffer,
	StructuredBuffer = BufferTargetARB.ShaderStorageBuffer
}